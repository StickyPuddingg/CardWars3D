using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using MiniJSON;
using UnityEngine;

public class Session : IDisposable
{
    public class FramerateWatcher
    {
        public float frequency = 0.5f;
        private float accum;
        private int frames;
        private float waitTime;
        private float currentFPS;

        public float Framerate
        {
            get
            {
                return currentFPS;
            }
        }

        public void Update()
        {
            // Optimization: Avoid dividing by deltaTime if it's 0 to prevent NaN errors
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            accum += Time.timeScale / dt;
            frames++;
            waitTime += dt;

            if (waitTime > frequency)
            {
                currentFPS = accum / (float)frames;
                waitTime = 0f;
                accum = 0f;
                frames = 0;
            }
        }
    }

    public class Authorizing
    {
        private bool _finishedLogin;

        Action failEvent = Session.OnSessionUserLoginFail;

        public void OnEnter(Session session)
        {
            TFUtils.DebugLog("Starting User login");
            _finishedLogin = false;
        }

        public void OnLeave(Session session) { }

        public void OnUpdate(Session session)
        {
            if (_finishedLogin) return;

            if (session.PlayerIsLoggedIn())
            {
                TFUtils.DebugLog("User logged In");
                _finishedLogin = true;
                return;
            }

            Dictionary<string, object> dictionary = (Dictionary<string, object>)session.CheckAsyncRequest("userLogin");
            if (dictionary == null) return;

            // CRITICAL 3DS OPTIMIZATION: Removed the heavy foreach loop string allocation debug logs.

            bool NetworkError = session.Server.IsNetworkError(dictionary);
            bool UsernameExists = PlayerPrefs.GetString("user") == "";

            if (!NetworkError || UsernameExists)
            {
                if (failEvent != null)
                {
                    failEvent();
                }
                session.Server.SetLoggedOut();
                session.ThePlayer = Player.LoadFromFilesystem();
            }
            else
            {
                if (failEvent != null)
                {
                    failEvent();
                }
                session.ThePlayer = Player.LoadFromResponse(PlayerPrefs.GetString("user"), true);
            }
            session.TheGame.SetPlayer(session.ThePlayer);
            session.ThePlayer.SaveLocally();
            session.WebFileServer.SetPlayerInfo(session.ThePlayer);
        }

        public bool IsLoggedIn()
        {
            return _finishedLogin;
        }
    }

    public delegate void GameloopAction();
    public delegate void AsyncAction();

    private const string LOAD_GAME = "loadGame";
    private const string LOAD_GAME_CHECK = "loadGameCheck";
    private const string DELETE_GAME = "deleteGame";
    private const string GET_USERINFO = "getUserInfo";
    private const string GET_SERVERVERSION = "getServerVersion";
    private const string GET_MESSAGES_LIST = "getMessagesList";
    private const string GET_MESSAGE = "getMessage";
    private const string TEST_CONNECTIVITY = "testConnectivity";
    private const string USER_LOGIN = "userLogin";

    private Player player;
    private SQServer server;
    private SQWebFileServer webFileServer;
    private SQAuth auth;
    private Game game;
    private Authorizing authorizing;
    private int currentVersion;
    private bool messageListLoaded;

    // Reused for response processing to eliminate per-frame GC garbage allocations
    private List<string> queuedResponses = new List<string>();
    private List<string> processingCache = new List<string>();

    private bool needsReload;
    private Dictionary<string, TFServer.JsonResponseHandler> externalRequests = new Dictionary<string, TFServer.JsonResponseHandler>();
    private Dictionary<string, object> asyncRequests = new Dictionary<string, object>();
    private Dictionary<string, TFWebFileResponse> asyncFileRequests = new Dictionary<string, TFWebFileResponse>();
    private SQContentPatcher contentPatcher;
    private string LocalManifestVersion;
    private bool _finishedPatching;
    private Thread _validationThread;
    private readonly object _validationLock = new object();

    public SQServer Server
    {
        get
        {
            return server;
        }
    }
    public SQWebFileServer WebFileServer
    {
        get
        {
            return webFileServer;
        }
    }

    public string Username
    {
        set
        {
            webFileServer.Username = value;
        }
    }

    public SQAuth Auth
    {
        get
        {
            return auth;
        }
    }

    public Game TheGame
    {
        get
        {
            return game;
        }
        set
        {
            game = value;
        }
    }

    public Player ThePlayer
    {
        get
        {
            return player;
        }
        set
        {
            player = value;
        }
    }

    public string UpdateUrl { get; private set; }

    public bool NeedsReload
    {
        get
        {
            return needsReload;
        }
    }

    public bool ValidatingLastPatch
    {
        get
        {
            return _validationThread != null;
        }
    }

    [method: MethodImpl(32)]
    public static event Action OnSessionUserLoginFail;

    [method: MethodImpl(32)]
    public static event Action OnSessionUserLoginSucceed;

    // Stripped out deviceId/fbid logic non-functional on 3DS
    public Session(int currentVersion, string deviceId)
    {
        TFUtils.Init();
        SQSettings.Init();
        authorizing = new Authorizing();
        CookieContainer cookies = new CookieContainer();
        server = new SQServer(cookies);
        webFileServer = new SQWebFileServer(cookies, deviceId);
        auth = new SQAuth(Application.platform);
        this.currentVersion = currentVersion;
        OnInit();
        authorizing.OnEnter(this);
    }

    public void ProcessAsyncResponse(string key)
    {
        TFWebFileResponse tFWebFileResponse = CheckAsyncFileRequest(key);
        if (tFWebFileResponse == null) return;

        switch (key)
        {
            case "loadGameCheck":
                SessionManager.GetInstance().LocalRemoteSaveGameConflict = false;
                if (tFWebFileResponse.StatusCode == HttpStatusCode.OK)
                {
                    Dictionary<string, object> asJSONDict2 = tFWebFileResponse.GetAsJSONDict();
                    bool flag = asJSONDict2 != null && asJSONDict2.ContainsKey("PlayerName");
                    SessionManager.GetInstance().LocalRemoteSaveGameConflict = flag && game.GameExists(player) && !webFileServer.HasLocalDeviceTag(tFWebFileResponse);
                }
                SessionManager.GetInstance().FinishedCheckSaveConflict();
                break;

            case "loadGame":
                if (tFWebFileResponse.StatusCode == HttpStatusCode.OK)
                {
                    Dictionary<string, object> asJSONDict = tFWebFileResponse.GetAsJSONDict();
                    if (asJSONDict != null && asJSONDict.ContainsKey("PlayerName"))
                    {
                        if (!webFileServer.HasLocalDeviceTag(tFWebFileResponse))
                        {
                            game.SaveLocally(tFWebFileResponse.Data);
                            Singleton<AnalyticsManager>.Instance.LogDebug("sever_override");
                        }
                        SessionManager.loginCompletedWithoutError = true;
                    }
                    break;
                }
                SessionManager.loginCompletedWithoutError = tFWebFileResponse.StatusCode == HttpStatusCode.NotFound || tFWebFileResponse.StatusCode == HttpStatusCode.NotModified;
                if (!game.GameExists(player) && tFWebFileResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    WebFileServer.DeleteETagFile();
                }
                break;

            case "deleteGame":
                break;

            case "getMessagesList":
                if (tFWebFileResponse.StatusCode == HttpStatusCode.OK)
                {
                    if (game != null) game.MyMessagesList = ProcessMessageListData(tFWebFileResponse.Data);
                }
                else
                {
                    messageListLoaded = true;
                }
                break;

            case "getServerVersion":
            case "testConnectivity":
                if (tFWebFileResponse.StatusCode == HttpStatusCode.OK && game != null)
                {
                    game.MyServerVersion = ProcessVersionData(tFWebFileResponse.Data);
                }
                else if (game != null)
                {
                    game.MyServerVersion = new Version(0, 0, 0);
                }
                break;

            case "getUserInfo":
            case "getMessage":
                if (tFWebFileResponse.StatusCode == HttpStatusCode.OK && game != null)
                {
                    if (key == "getUserInfo") game.MyUserInfo = tFWebFileResponse.Data;
                    else game.MyMessages.Add(ProcessMessageData(tFWebFileResponse.Data));
                }
                else if (game != null)
                {
                    game.MyUserInfo = "{\"error\":\"no data\"}";
                }
                break;
        }
        game.AccessDone = true;
    }

    public void Update()
    {
        OnUpdate();
    }

    private void OnUpdate()
    {
        authorizing.OnUpdate(this);
        ProcessAsyncResponses();
    }

    public void Dispose()
    {
        OnDispose();
    }

    public void ClearNeedsReload()
    {
        needsReload = false;
    }

    public void CheckGameFromNetwork()
    {
        game.CheckFromNetwork("loadGameCheck", this);
    }

    public void LoadGameFromNetwork()
    {
        game.LoadFromNetwork("loadGame", this);
    }

    public void DeleteGameFromNetwork()
    {
        game.DeleteFromNetwork("deleteGame", this);
    }

    public void GetMessagesList()
    {
        game.GetMessagesList("getMessagesList", this);
    }

    public void GetMessage(string id)
    {
        game.GetMessage("getMessage", id, this);
    }

    public void GetUserInfo()
    {
        game.GetUserInfo("getUserInfo", this);
    }

    public void GetServerVersion()
    {
        game.GetServerVersion("getServerVersion", this);
    }

    public void TestConnectivity()
    {
        game.GetServerVersion("testConnectivity", this);
    }

    public void GetServerTime()
    {
        Server.GetTime(delegate (Dictionary<string, object> data, HttpStatusCode status)
        {
            if (status == HttpStatusCode.OK)
            {
                Dictionary<string, object> dictionary = (Dictionary<string, object>)data["data"];
                DateTime dateTime = (TFUtils.lastServerTimeUpdate = DateTime.Parse(dictionary["server_time"].ToString()));
                TimeSpan timeSpan = dateTime.Subtract(DateTime.Now);
                if (Math.Abs((timeSpan - TFUtils.serverTimeDiff).TotalSeconds) > 10.0)
                {
                    TFUtils.serverTimeDiff = timeSpan;
                }
            }
        });
    }
    public bool IsLoggedIn()
    {
        return authorizing.IsLoggedIn();
    }

    public bool IsAuthenticated()
    {
        return auth.IsAuthenticated();
    }

    public bool IsMessagelistLoaded()
    {
        return messageListLoaded;
    }

    public int GetLocalVersion()
    {
        return currentVersion;
    }

    public bool PlayerIsLoggedIn()
    {
        return player != null;
    }

    public void registerExternalCallback(string requestId, TFServer.JsonResponseHandler callback)
    {
        externalRequests[requestId] = callback;
    }

    private List<string> ProcessMessageListData(string data)
    {
        List<string> list = GiftMessage.ProcessMessageListData(data);
        GetNextMessage(list);
        return list;
    }

    private string ProcessMessageData(string data)
    {
        game.MyMessagesList.RemoveAt(0);
        GetNextMessage(game.MyMessagesList);
        return data;
    }

    private void GetNextMessage(List<string> list)
    {
        if (list == null || list.Count == 0) messageListLoaded = true;
        else GetMessage(list[0]);
    }

    private Version ProcessVersionData(string response)
    {
        // Optimized to avoid excessive splitting configurations
        string[] version = response.Split('.');
        return new Version(int.Parse(version[0]), int.Parse(version[1]), int.Parse(version[2]));
    }

    protected void ProcessAsyncResponses()
    {
        int count = queuedResponses.Count;
        if (count <= 0) return;

        // 3DS GC SAFE CONCURRENCY CONVERSION:
        // We cache data into an already-allocated collection rather than making a `new List<string>()` on every tick.
        processingCache.Clear();
        for (int i = 0; i < count; i++)
        {
            processingCache.Add(queuedResponses[i]);
        }
        queuedResponses.Clear();

        for (int i = 0; i < count; i++)
        {
            ProcessAsyncResponse(processingCache[i]);
        }
    }

    protected void QueueResponse(string key)
    {
        queuedResponses.Add(key);
    }

    public void AddAsyncResponse(string key, object val)
    {
        lock (asyncRequests)
        {
            asyncRequests[key] = val;
        }
    }

    public object CheckAsyncRequest(string key)
    {
        object result = null;
        lock (asyncRequests)
        {
            if (asyncRequests.TryGetValue(key, out result))
            {
                asyncRequests.Remove(key);
            }
        }
        return result;
    }

    public TFServer.JsonResponseHandler AsyncResponder(string key)
    {
        return delegate (Dictionary<string, object> response, HttpStatusCode status)
        {
            AddAsyncResponse(key, response);
        };
    }

    public void AddAsyncFileResponse(string key, TFWebFileResponse val)
    {
        lock (asyncFileRequests)
        {
            asyncFileRequests[key] = val;
            game.AccessDone = false;
            QueueResponse(key);
        }
    }

    public TFWebFileResponse CheckAsyncFileRequest(string key)
    {
        TFWebFileResponse result = null;
        lock (asyncFileRequests)
        {
            if (asyncFileRequests.TryGetValue(key, out result))
            {
                asyncFileRequests.Remove(key);
            }
        }
        return result;
    }

    public TFWebFileServer.FileCallbackHandler AsyncFileResponder(string key)
    {
        return delegate (TFWebFileResponse response)
        {
            AddAsyncFileResponse(key, response);
        };
    }

    private void OnInit()
    {
        _validationThread = null;
        _finishedPatching = false;
    }

    private void OnDispose()
    {
        lock (_validationLock)
        {
            if (_validationThread != null)
            {
                _validationThread.Abort();
                _validationThread.Join();
                _validationThread = null;
            }
        }
    }

    public string GetLocalManifestVersion()
    {
        return LocalManifestVersion;
    }

    public bool IsPatchDone()
    {
        return _finishedPatching;
    }

    public void ValidateLastPatch()
    {
        lock (_validationLock)
        {
            if (_validationThread != null) return;

            SQContentPatcher patcher = new SQContentPatcher();
            Session me = this;
            _validationThread = new Thread((ThreadStart)delegate
            {
                patcher.ValidateAndFixDownloadedManifests();
                lock (me._validationLock)
                {
                    me._validationThread = null;
                }
            });
            _validationThread.Start();
        }
    }

    public bool UpdatePatching()
    {
        if (contentPatcher != null || ValidatingLastPatch) return contentPatcher != null;

        contentPatcher = new SQContentPatcher();
        contentPatcher.AddListener(OnPatchingEvent);
        contentPatcher.ReadManifests();
        LocalManifestVersion = contentPatcher.LocalManifestVersion();
        return true;
    }

    private void OnPatchingEvent(string eventStr)
    {
        switch (eventStr)
        {
            case "patchingNecessary":
                _finishedPatching = true;
                contentPatcher.StartDownloadingPatchedContent();
                break;
            case "patchingDone":
                _finishedPatching = true;
                if (contentPatcher != null && contentPatcher.ContentChanged && SessionManager.GetInstance().IsLoadDataDone())
                {
                    needsReload = true;
                }
                contentPatcher = null;
                break;
            case "patchingNotNecessary":
                contentPatcher = null;
                break;
        }
    }

    public void StartPatch()
    {
        _finishedPatching = false;
        UpdatePatching();
    }
}