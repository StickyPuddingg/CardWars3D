using System;
using System.IO;
using UnityEngine;
using UnityEngine.N3DS;

public class SessionManager : MonoBehaviour
{
	public enum States
	{
		WAITING_FOR_USERID,
		LOGGING_IN,
		LOAD_DATA,
		CHECK_SAVE_CONFLICT,
		LOADING,
		VALIDATE_PATCH,
		VERSION_CHECK,
		WAITING_FOR_RESTART,
		PATCHING,
		MESSAGE_FETCH,
		SAVING,
		QUERYING,
		READY
	}

	public delegate void AssignFacebookIDToUserCallback(bool success);

	public delegate void OnReadyDelegate();

	public delegate void OnSaveDelegate(bool success);

	private const string DEVICEID_FILE = "deviceName";

	private const int CurrentVersion = 1;

	public GameObject BusyIcon;

	public string PlayerID;

	public string LoginID;

	public string NetState;

	public string DeviceID;

	private States state;

	private bool isPatched;

	public bool LocalRemoteSaveGameConflict;

	private bool checkSaveConflictFinished;

	private bool loadingDataFinished;

	private OnReadyDelegate myOnReadyCallback;

	private OnSaveDelegate saveToServerCallback;

	private string saveToServerData;

	private int? saveToServerResponse;

	private OnReadyDelegate attemptConnectionCallback;

	private int? attemptConnectionResponse;

	public static bool loginCompletedWithoutError;

	private bool checkedVersion;

	private static SessionManager instance;

	private Session session;

	public bool NeedsForcedUpdate { get; private set; }

	public bool HasNewMessagesReady
	{
		get
		{
			return session != null && session.TheGame != null && session.TheGame.MyMessages != null && session.TheGame.MyMessages.Count > 0;
		}
	}

	private States State
	{
		get
		{
			return state;
		}
		set
		{
			TFUtils.DebugLog("Changing State: " + state.ToString() + " ==> " + value.ToString() + ", Time: " + DateTime.Now.ToString("HH:mm:ss.ff"), "session");
			state = value;
			NetState = Enum.GetName(typeof(States), value);
		}
	}

	public OnReadyDelegate OnReadyCallback
	{
		get
		{
			return myOnReadyCallback;
		}
		set
		{
			myOnReadyCallback = value;
		}
	}

	public Session theSession
	{
		get
		{
			return session;
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.transform.gameObject);
	}

	public static SessionManager GetInstance()
	{
		return instance;
	}

	private void Start()
	{
		TFUtils.Init();
		GameObject[] array = GameObject.FindGameObjectsWithTag("SessionMgr");
		if (array.Length > 1)
		{
			GameObject[] array2 = array;
			foreach (GameObject gameObject in array2)
			{
				SessionManager component = gameObject.GetComponent<SessionManager>();
				if (!component || component != instance)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
			}
		}
		DeviceID = LoadDeviceId();
		session = null;
		State = States.WAITING_FOR_USERID;

        // --- Place at the very end of SessionManager.Start() ---

        try
        {
            if (session != null)
            {
                System.Type type = session.GetType();
                Logger.Error("[SessionManager][SessionCheck] Reflecting fields on Session type: " + type.Name);

                // Print all boolean and string values on the session to see what is missing
                System.Reflection.FieldInfo[] fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                foreach (System.Reflection.FieldInfo field in fields)
                {
                    if (field.FieldType == typeof(bool) || field.FieldType == typeof(string))
                    {
                        Logger.Error(string.Format("[SessionManager][SessionCheck] Field '{0}' = {1}", field.Name, field.GetValue(session)));
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Logger.Error("[SessionManager][SessionCheck] Reflection failed: " + ex.Message);
        }

    }


	public string LoadDeviceId()
	{
		string path = Path.Combine(UnityEngine.Application.persistentDataPath, "deviceName");
		if (File.Exists(path))
		{
			return File.ReadAllText(path);
		}
		string text = Guid.NewGuid().ToString();
		File.WriteAllText(path, text);
		return text;
	}

public void Login(string name)
{
    if (session == null || !session.IsAuthenticated())
    {
        LoginID = name;
        State = States.LOGGING_IN;
        session = new Session(1, DeviceID);
        session.TheGame = new Game();

            if (session.ThePlayer == null)
            {
				Logger.Error("[SessionManager] WARNING: session.ThePlayer is null.");
            }
        }
}

	// Helper to safely get the 64-bit device ID as a string and determine if it's a new player
	public string Get3DSDeviceId(out bool isNewPlayer)
	{
		string deviceIdStr = "3DS_Fallback_ID";

		try
		{
			// Seed it with an application unique ID (e.g., 12345 or your title ID seed)
			ulong rawId = Config.GetTransferableId();
			deviceIdStr = rawId.ToString();
		}
		catch (System.Exception ex)
		{
			Logger.Error("[PlayerInfoScript] Failed to fetch Config.GetTransferableId: " + ex.Message);
		}

		// Check if the system already has a saved file or name for this user to determine 'isNew'
		isNewPlayer = !PlayerPrefs.HasKey("SocialLogin") && !PlayerPrefs.HasKey("SavePlayerName");

		return deviceIdStr;
	}
    public string Build3DSPlayerName()
    {
        // -s Use the requested 3DS config user-name API directly.
        try
        {
            string userName;
            bool isNameProfane;
            Config.GetUserName(out userName, out isNameProfane);
            return userName;
        }
        catch (Exception)
        {
            // -s Fall back to a generated value when the config call is unavailable.
            return null;
        }
    }

    public void Logout()
	{
		session = null;
		State = States.WAITING_FOR_USERID;
	}

    public bool IsLoggedIn()
    {
        // If the session doesn't exist, or the internal player object hasn't spun up yet,
        // we are NOT ready to progress.
        if (session == null || session.ThePlayer == null)
        {
            return false;
        }

        // If the player object exists, fall back to the session's internal login verification
        return session.IsLoggedIn();
    }
    public bool IsAuthenticated()
    {
        return session != null && session.IsAuthenticated();
    }

    public bool IsReady()
	{
		return IsLoggedIn() && State == States.READY;
	}

	public void StartSyncStreamingAssets()
	{
		session.StartPatch();
	}

	public bool IsPatchingSyncDone()
	{
		return session.IsPatchDone();
	}

	public bool IsMessageSyncDone()
	{
		return session.IsMessagelistLoaded();
	}

	private bool IsSaveDone()
	{
			return true;

	}

	public string GetStreamingAssetsPath(string fname)
	{
		string result = Path.Combine(UnityEngine.Application.streamingAssetsPath, fname);
		if (DebugFlagsScript.GetInstance().UseLocalJsonFiles)
		{
			return result;
		}
		string persistentAssetsPath = TFUtils.GetPersistentAssetsPath();
		if (!string.IsNullOrEmpty(persistentAssetsPath))
		{
			string text = Path.Combine(persistentAssetsPath, fname);
			if (File.Exists(text))
			{
				return text;
			}
		}
		return result;
	}

	public string GetPlayerDataPath(string fname)
	{
		return session.ThePlayer.CacheFile(fname);
	}

	public void SetGameStateJson(string gameData)
	{
		if (session != null && session.TheGame != null)
		{
			session.TheGame.SaveLocally(gameData);
		}
	}

	public void SaveToServer(string gameData, OnSaveDelegate callback = null)
	{
		if (session != null && session.TheGame != null)
		{
			if (callback == null)
			{
				session.TheGame.SaveToServer(session, gameData);
				return;
			}
			saveToServerData = gameData;
			saveToServerCallback = callback;
			AttemptSave();
		}
	}

	private void AttemptSave()
	{
		if (GetInstance().LocalRemoteSaveGameConflict)
		{
			TFUtils.DebugLog("LocalRemoteSaveGameConflict, not saving to server until resolved", "saveload");
		}
		else if (UnityEngine.Application.internetReachability == NetworkReachability.NotReachable)
		{
			STDErrorDialog sTDErrorDialog = STDErrorDialog.GetInstance();
			if (sTDErrorDialog != null)
			{
				sTDErrorDialog.ShowError("Error N01: Connection Interrupted", AttemptSave);
			}
			else
			{
				saveToServerCallback(false);
			}
		}
		else
		{
			session.WebFileServer.SaveGameData(saveToServerData, RecordSaveResponse, session);
		}
	}

	public void RecordSaveResponse(TFWebFileResponse response)
	{
		saveToServerResponse = (int)response.StatusCode;
	}

	public void HandleSaveResponse()
	{
		int? num = saveToServerResponse;
		int num2 = (num.HasValue ? num.Value : 0);
		saveToServerResponse = null;
		if (num2 != 200 && num2 != 201)
		{
			string error = string.Format("HTTP Status {0}: There was a problem accessing the server.", num2);
			STDErrorDialog sTDErrorDialog = STDErrorDialog.GetInstance();
			if (sTDErrorDialog != null)
			{
				sTDErrorDialog.ShowError(error, AttemptSave);
			}
			else
			{
				saveToServerCallback(false);
			}
		}
		else
		{
			saveToServerCallback(true);
		}
	}

	public void AttemptConnection(OnReadyDelegate callback)
	{
		if (session != null && session.TheGame != null && callback != null)
		{
			attemptConnectionCallback = callback;
			AttemptConnection();
		}
	}

	private void AttemptConnection()
	{
		if (UnityEngine.Application.internetReachability == NetworkReachability.NotReachable)
		{
			STDErrorDialog.GetInstance().ShowError("Error N01: Connection Interrupted", AttemptConnection);
		}
		else
		{
			session.WebFileServer.GetServerVersion(RecordConnectionResponse);
		}
	}

	public void RecordConnectionResponse(TFWebFileResponse response)
	{
		attemptConnectionResponse = (int)response.StatusCode;
	}

	public void HandleConnectionResponse()
	{
		int? num = attemptConnectionResponse;
		int num2 = (num.HasValue ? num.Value : 0);
		attemptConnectionResponse = null;
		if (num2 != 200)
		{
			string error = string.Format("HTTP Status {0}: There was a problem accessing the server.", num2);
			STDErrorDialog.GetInstance().ShowError(error, AttemptConnection);
		}
		else
		{
			attemptConnectionCallback();
		}
	}

	public void LoadFromServer()
	{
		if (session != null && session.TheGame != null)
		{
			session.LoadGameFromNetwork();
		}
	}

	public void CheckFromServer()
	{
		if (session != null && session.TheGame != null)
		{
			session.CheckGameFromNetwork();
		}
	}

	public void DeleteFromServer()
	{
		if (session != null && session.TheGame != null)
		{
			session.DeleteGameFromNetwork();
		}
	}

	public void ClearSaveStateLocal()
	{
		if (session != null && session.TheGame != null)
		{
			session.TheGame.ClearCachedSaveState(session);
		}
	}

	public void DeleteLocal()
	{
		if (session != null && session.TheGame != null)
		{
			session.TheGame.DestroyCache(session.ThePlayer);
		}
	}

	public string GetGameStateJson()
	{
		if (session != null && session.TheGame != null)
		{
			return session.TheGame.LoadLocally();
		}
		return null;
	}

	public void RequestMyUserInfo()
	{
		if (State == States.READY)
		{
			State = States.QUERYING;
			if (session != null && session.TheGame != null)
			{
				session.GetUserInfo();
			}
		}
	}

	public Version GetServerVersion()
	{
		if (session != null && session.TheGame != null)
		{
			return session.TheGame.MyServerVersion;
		}
		return null;
	}

	public void TestConnectivity()
	{
		if (session != null && session.TheGame != null)
		{
			session.TestConnectivity();
		}
	}

	public string GetMyUserInfoJson()
	{
		if (session != null && session.TheGame != null)
		{
			return session.TheGame.MyUserInfo;
		}
		return null;
	}

	public void RequestMyMessagesList()
	{
		if (State == States.READY)
		{
			State = States.QUERYING;
			if (session != null && session.TheGame != null)
			{
				session.GetMessagesList();
			}
		}
	}

	public void RequestMyMessage(string id)
	{
		if (State == States.READY)
		{
			State = States.QUERYING;
			if (session != null && session.TheGame != null)
			{
				session.GetMessage(id);
			}
		}
	}

    private void StartLoadingData()
    {
        loadingDataFinished = false;
        Logger.Error("[SessionManager] StartLoadingData invoked manually. Handing off to LoadingManager.");
        StartCoroutine(LoadingManager.Instance.LoadAll(FinishedLoadingData));
    }

	public void FinishedLoadingData()
	{
		isPatched = true;
		loadingDataFinished = true;
	}

	public bool IsLoadDataDone()
	{
		return loadingDataFinished;
	}

	public void FinishedCheckSaveConflict()
	{
		checkSaveConflictFinished = true;
	}

	public bool IsCheckSaveConflictDone()
	{
		return checkSaveConflictFinished;
	}

    private void Update()
    {
        if (session != null)
        {
            session.Update();
        }

        // --- Replace the LOGGING_IN block in SessionManager.Update() with this ---

        if (State == States.LOGGING_IN)
        {
            bool loggedIn = IsLoggedIn();
            bool hasSession = (session != null);
            bool sessionAuthenticated = (session != null && session.IsAuthenticated());

            if (!loggedIn)
            {
                if (Time.frameCount % 300 == 0)
                {
                    Logger.Error(string.Format(
                        "[SessionManager][GateCheck] Stuck in LOGGING_IN. IsLoggedIn(): {0} | session != null: {1} | IsAuthenticated(): {2}",
                        loggedIn,
                        hasSession,
                        sessionAuthenticated
                    ));
                }
            }
            else
            {
                Logger.Log("[SessionManager][GateCheck] SUCCESS: IsLoggedIn() passed! Initializing data handoff.");
                PlayerID = session.ThePlayer.playerId;
                State = States.LOAD_DATA;
                StartLoadingData();
                return;
            }
        }
        else
        {
            // Failsafe: What if the state changed somewhere else entirely?
            if (State != States.READY && State != States.LOAD_DATA && Time.frameCount % 300 == 0)
            {
                Logger.Error(string.Format("[SessionManager][GateCheck] Warning: Current state is '{0}', NOT LOGGING_IN. Initialization bypassed.", State));
            }
        }


        if (State == States.LOAD_DATA && IsLoadDataDone())
        {
            // BYPASS REMOTE SERVER CHECK: Instead of moving to CHECK_SAVE_CONFLICT 
            // and calling the broken network method, jump directly to LOADING locally.

            TFUtils.DebugLog("[SessionManager] Bypassing remote server ETag check. Forcing local data load pipeline.");

            // Explicitly handle what the conflict state was supposed to trigger locally
            PlayerInfoScript.ValidateAndFixLocalSave();
            PlayerInfoScript.Load();

            // Initialize game systems
            QuestManager.Instance.InitializeQuestStates();
            SideQuestManager.Instance.InitializeQuestStates();
            Singleton<AnalyticsManager>.Instance.LogTotalXP();
            Singleton<AnalyticsManager>.Instance.LogTotalCoins();

            // Jump straight to the local save verification phase
            State = States.SAVING;
            PlayerInfoScript.GetInstance().Save();
            return; // Force frame exit to allow saving to process safely
        }

        if (State == States.CHECK_SAVE_CONFLICT)
        {
            PlayerInfoScript.ValidateAndFixLocalSave();
            State = States.LOADING;
            PlayerInfoScript.Load();
            QuestManager.Instance.InitializeQuestStates();
            SideQuestManager.Instance.InitializeQuestStates();
            Singleton<AnalyticsManager>.Instance.LogTotalXP();
            Singleton<AnalyticsManager>.Instance.LogTotalCoins();
            State = States.SAVING;
            PlayerInfoScript.GetInstance().Save();
            return; // <--- FORCE EXIT FRAME
        }

        if (State == States.LOADING && IsSaveDone())
        {
            State = States.SAVING;
            PlayerInfoScript.GetInstance().Save();
            return; // <--- FORCE EXIT FRAME
        }
        if (State == States.MESSAGE_FETCH && IsMessageSyncDone())
        {
            State = States.SAVING;
            PlayerInfoScript.GetInstance().Save();
        }
        if (State == States.SAVING && IsSaveDone())
        {
            State = States.READY;
            if (myOnReadyCallback != null)
            {
                myOnReadyCallback();
            }
        }
        if (State == States.QUERYING && IsSaveDone())
        {
            State = States.READY;
            if (myOnReadyCallback != null)
            {
                myOnReadyCallback();
            }
        }
        if (saveToServerResponse.HasValue)
        {
            HandleSaveResponse();
        }
        if (attemptConnectionResponse.HasValue)
        {
            HandleConnectionResponse();
        }
        if (session != null && session.TheGame != null)
        {
            if (session.TheGame.needsSaveSuccessfulDialog)
            {
                session.TheGame.needsSaveSuccessfulDialog = false;
                DebugPopupScript.CreateSavePopup(true);
            }
            if (session.TheGame.needsSaveFailedDialog)
            {
                session.TheGame.needsSaveFailedDialog = false;
                DebugPopupScript.CreateSavePopup(false);
            }
        }
    }

    public void OnApplicationPause(bool paused)
	{
		TFUtils.DebugLog("Application pausing" + paused);
		if (!paused && session != null)
		{
			session.GetServerTime();
		}
		if (!paused && IsReady() && IsLoggedIn())
		{
			StartSyncStreamingAssets();
			Singleton<AnalyticsManager>.Instance.LogTotalXP();
			Singleton<AnalyticsManager>.Instance.LogTotalCoins();
		}
	}

	public void Clear()
	{
		instance = null;
	}
}
