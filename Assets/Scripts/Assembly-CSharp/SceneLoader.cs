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
    public string nextScene = "LoadingScreen";

    // Made static so it survives scene transitions and is accessible globally
    public static string DestinationScene = "AdventureTime";
    public float minimumWaitTime = 8f;

    // Helper method to safely initiate a transition from buttons
    public static void InitiateLoad(string targetScene)
    {
        DestinationScene = targetScene;
        SLOTSceneManager sceneManager = SLOTGameSingleton<SLOTSceneManager>.GetInstance();
        if (sceneManager != null)
        {
            // First, load the dedicated intermediate Loading Screen scene
            sceneManager.LoadLevel("LoadingScreen");
        }
        else
        {
            Logger.Error("[AsyncSceneLoader] CRITICAL: SLOTSceneManager instance not found!");
        }
    }

    private void Awake()
    {
        Logger.Error("[AsyncSceneLoader] AWAKE: Component active on: " + gameObject.name);
    }

    private IEnumerator Start()
    {
        Logger.Error(string.Format("[AsyncSceneLoader] START: Processing mode: {0}", mode));

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
                Logger.Error(string.Format("[AsyncSceneLoader] SUCCESS: Loaded level '{0}'", sceneName));
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

        Logger.Error("[AsyncSceneLoader] BUFFER: Clearing memory states to prevent crashes...");
        GC.Collect();
        yield return null;
        yield return Resources.UnloadUnusedAssets();
        yield return null; // Additional frames ease memory spikes

        float elapsed = Time.realtimeSinceStartup - startTime;
        float waitTime = Mathf.Max(0f, minimumWaitTime - elapsed);

        if (waitTime > 0f)
        {
            Logger.Error(string.Format("[AsyncSceneLoader] BUFFER: Holding for {0:F2} seconds...", waitTime));
            yield return new WaitForSeconds(waitTime);
        }
    }
}