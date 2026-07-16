using System;
using System.Collections;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public enum LoaderMode
    {
        Startup,
        LoadingBuffer
    }

    public LoaderMode mode = LoaderMode.Startup;
    public string nextScene = "LoadingScreen3DS";

    // Track both the destination and the historical source scene across frames
    public static string DestinationScene = "AdventureTime3DS";
    public static string PreviousScene = ""; // <--- NEW: Track where we came from

    public float minimumWaitTime = 8f;

    // Updated helper to capture the current active scene before transitioning
    public static void InitiateLoad(string targetScene)
    {
        // 1. Record our current location before we swap levels
        try
        {
            PreviousScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }
        catch (Exception)
        {
            // Fallback for older Unity configurations where GetActiveScene might lack wrapper bindings
            PreviousScene = Application.loadedLevelName;
        }

        DestinationScene = targetScene;

        SLOTSceneManager sceneManager = SLOTGameSingleton<SLOTSceneManager>.GetInstance();
        if (sceneManager != null)
        {
            // Note: Swapped to load your local 3DS loading screen asset name safely
            sceneManager.LoadLevel("LoadingScreen3DS");
        }
        else
        {
            Logger.Error("[AsyncSceneLoader] CRITICAL: SLOTSceneManager instance not found!");
        }
    }

    private IEnumerator Start()
    {
        if (mode == LoaderMode.Startup)
        {
            yield return StartCoroutine(InitializeStartup());

            if (!string.IsNullOrEmpty(nextScene))
            {
                LoadSceneViaManager(nextScene);
            }
        }
        else if (mode == LoaderMode.LoadingBuffer)
        {
            // 2. RUN ROUTING OVERRIDE LOGIC HERE:
            // Detect if our historical origin requires a redirection to the Battle Scene
            if (PreviousScene == "AdventureTime3DS")
            {
                Logger.Error("[AsyncSceneLoader] Route Intercept: Came from AdventureTime3DS. Rerouting destination to BattleScene3DS.");
                DestinationScene = "BattleScene3DS";
            }

            yield return StartCoroutine(RunLoadingBuffer());

            if (!string.IsNullOrEmpty(DestinationScene))
            {
                LoadSceneViaManager(DestinationScene);
            }
            else
            {
                Logger.Error("[AsyncSceneLoader] BUFFER FAILED: DestinationScene is null or empty!");
            }
        }
    }

    private void LoadSceneViaManager(string sceneName)
    {
        try
        {
            SLOTSceneManager sceneManager = SLOTGameSingleton<SLOTSceneManager>.GetInstance();
            if (sceneManager != null)
            {
                sceneManager.LoadLevel(sceneName);
            }
            else
            {
                Logger.Error("[AsyncSceneLoader] CRITICAL ERROR: SLOTSceneManager is NULL!");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(string.Format("[AsyncSceneLoader] EXCEPTION during LoadLevel: {0}\n{1}", ex.Message, ex.StackTrace));
        }
    }

    private IEnumerator InitializeStartup()
    {
        int loopCount = 0;
        while (SessionManager.GetInstance() == null || PlayerInfoScript.GetInstance() == null)
        {
            loopCount++;
            if (loopCount % 100 == 0)
            {
                Logger.Error("[AsyncSceneLoader] LOOP STUCK: Waiting for dependencies!");
            }
            yield return null;
        }
        Logger.Error("[AsyncSceneLoader] STARTUP: Dependencies resolved successfully.");
    }

    private IEnumerator RunLoadingBuffer()
    {
        float startTime = Time.realtimeSinceStartup;

        GC.Collect();
        yield return null;
        yield return Resources.UnloadUnusedAssets();
        yield return null;

        float elapsed = Time.realtimeSinceStartup - startTime;
        float waitTime = Mathf.Max(0f, minimumWaitTime - elapsed);

        if (waitTime > 0f)
        {
            yield return new WaitForSeconds(waitTime);
        }
    }
}