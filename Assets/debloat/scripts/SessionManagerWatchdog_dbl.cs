using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

/// ponytail: this is a single-player offline debloat build - there is no server.
/// Session's real constructor calls TFUtils.Init/SQSettings.Init/SQAuth.AuthUser, which
/// hit the filesystem and network and can throw or hang waiting on a server that doesn't
/// exist. Instead of fighting that pipeline, skip Session's constructor entirely
/// (FormatterServices.GetUninitializedObject) and build a minimal local-only session by
/// hand, then drop SessionManager straight into READY. No login, no retries, no timers.
public class SessionManagerWatchdog_dbl : MonoBehaviour
{
	private static bool forced;
	private bool seenInstance;
	private bool makeLocalReadyStarted;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void Watch()
	{
		if (forced)
		{
			Logger.Log("[SessionManagerWatchdog_dbl] Watch() skipped: forced already true (stale static from previous Play session - stop Play fully and re-enter, or enable Reload Domain)");
			return;
		}
		if (Object.FindObjectOfType<SessionManagerWatchdog_dbl>() != null)
		{
			Logger.Log("[SessionManagerWatchdog_dbl] Watch() skipped: instance already exists");
			return;
		}
		Logger.Log("[SessionManagerWatchdog_dbl] Watch() creating watchdog GameObject");
		GameObject go = new GameObject("[SessionManagerWatchdog_dbl]");
		Object.DontDestroyOnLoad(go);
		go.AddComponent<SessionManagerWatchdog_dbl>();
	}

    private void Update()
    {
        if (forced)
        {
            Destroy(gameObject);
            return;
        }

        SessionManager instance = SessionManager.GetInstance();
        if (instance == null)
        {
            return;
        }

        if (!seenInstance)
        {
            seenInstance = true;
            return;
		}


if (makeLocalReadyStarted)
{
    return;
}

		try
		{
			
			
			Logger.Log("[SessionManagerWatchdog_dbl] Starting MakeLocalReady coroutine");
			StartCoroutine(MakeLocalReady(instance));
			makeLocalReadyStarted = true;
		}
		catch (System.Exception e)
		{
			// This will now print the EXACT error stack trace telling you why the text/scene broken
			Logger.Error("[SessionManagerWatchdog_dbl] CRITICAL CRASH IN INITIALIZATION: " + e.ToString());
			forced = true;
		}
    }

	private IEnumerator MakeLocalReady(SessionManager instance)
	{
		Logger.Log("[SessionManagerWatchdog_dbl] MakeLocalReady started");
		// ponytail: SQSettings.Init() is normally called by Session's real constructor,
		// which we skip. Without it, SQSettings.MANIFEST_URL/CDN_URL stay null, and
		// SessionManager.OnApplicationPause auto-triggers a patch sync whenever the app
		// regains focus once ready - that crashes on `new Uri(null)` deep in SQContentPatcher.
		SQSettings.Init();

		Session session = (Session)FormatterServices.GetUninitializedObject(typeof(Session));

		Player player = Player.LoadFromFilesystem();
		session.ThePlayer = player;
		session.TheGame = new Game();
		session.TheGame.SetPlayer(player);

		CookieContainer cookies = new CookieContainer();
		SetField(session, "auth", new SQAuth(Application.platform));
		SetField(session, "server", new SQServer(cookies));
		SetField(session, "webFileServer", new SQWebFileServer(cookies, instance.DeviceID));
		SetField(session, "queuedResponses", new List<string>());
		SetField(session, "externalRequests", new Dictionary<string, TFServer.JsonResponseHandler>());
		SetField(session, "asyncRequests", new Dictionary<string, object>());
		SetField(session, "asyncFileRequests", new Dictionary<string, TFWebFileResponse>());

		object authorizing = FormatterServices.GetUninitializedObject(typeof(Session.Authorizing));
		SetField(authorizing, "_finishedLogin", true);
		SetField(session, "authorizing", authorizing);

		SetField(instance, "session", session);
		instance.PlayerID = player.playerId;
		SetField(instance, "state", SessionManager.States.READY);

		// ponytail: CWMenuEnvironmentAdditiveLoad.Start() (an AsyncLoader that
		// MenuController.IsAsyncLoadComplete() waits on) loops forever on
		// SessionManager.IsLoadDataDone(), which is normally flipped true by the LOADING
		// state we skip. Without this, the menu's own async-load gate never clears even
		// though SessionManager itself is READY.
		instance.FinishedLoadingData();

		yield return StartCoroutine(LoadingManager.Instance.LoadAll(null));

        Logger.Error("[SessionManagerWatchdog_dbl] LOADED PLAYER INFO");

        Logger.Error("[AsyncSceneLoader] STARTUP: Executing PlayerInfoScript.Load()...");
        PlayerInfoScript.Load();
        Logger.Error("[AsyncSceneLoader] STARTUP: PlayerInfoScript.Load() processed successfully.");

        Logger.Log("[SessionManagerWatchdog_dbl] MakeLocalReady completed");
        forced = true;
        Logger.Log("[SessionManagerWatchdog_dbl] Session forced fully local - no login, no network");
    }

	private static void SetField(object target, string fieldName, object value)
	{
		FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
		field.SetValue(target, value);
	}

}
