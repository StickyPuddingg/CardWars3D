using UnityEngine;
using UnityEngine.SceneManagement;

public class SLOTSceneManager : SLOTGameSingleton<SLOTSceneManager>
{
    public delegate void LoadLevelAsyncCallback();

    public bool useLocalScenes;

    private AssetBundle assetBundle;

    private LoadLevelAsyncCallback loadLevelAsyncCallback;

    private AsyncOperation asyncOperation;

    // Automatically register to Unity's scene loaded callback
    protected void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Unsubscribe from event to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (assetBundle != null)
        {
            assetBundle.Unload(true);
            assetBundle = null;
        }
    }

    // This automatically triggers whenever ANY scene finishes loading successfully
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string currentActiveScene = SceneManager.GetActiveScene().name;
        Logger.Error(string.Format("SLOTSceneManager STATE: Finished loading scene '{0}' (Mode: {1}). Current Active Scene is now: '{2}'", scene.name, mode, currentActiveScene));
    }

    // Checks for the target scene
    private bool IsMenuScene(string name)
    {
        if (name != "AdventureTime3DS")
        {
            Logger.Error(string.Format("SLOTSceneManager: Blocked attempt to '{0}'. Remaining dormant for AdventureTime3DS.", name));
            return false;
        }
        return true;
    }

    public bool SetAssetBundle(AssetBundle bundle)
    {
        if (bundle != null && !useLocalScenes)
        {
            assetBundle = bundle;
            return true;
        }
        return false;
    }

    private static string GetLevelName(string name)
    {
        if (SLOTGame.IsLowEndDevice())
        {
            return name;
        }
        return name;
    }

    public void LoadLevel(string name)
    {
        if (name != null && name.Length > 0)
        {
            string targetScene = GetLevelName(name);
            Logger.Log(string.Format("SLOTSceneManager ATTEMPT: Standard loading level '{0}' (Current active: '{1}')", targetScene, SceneManager.GetActiveScene().name));

            CheckAsyncOperationDone(true);
            SceneManager.LoadScene(targetScene);
        }
    }

    public void LoadLevelAdditive(string name)
    {
        if (name != null && name.Length > 0)
        {
            string targetScene = GetLevelName(name);
            Logger.Log(string.Format("SLOTSceneManager ATTEMPT: Additive loading level '{0}' (Current active: '{1}')", targetScene, SceneManager.GetActiveScene().name));

            CheckAsyncOperationDone(true);
            SceneManager.LoadScene(targetScene, LoadSceneMode.Additive);
        }
    }

    public AsyncOperation LoadLevelAsync(string name)
    {
        return LoadLevelAsync(name, null);
    }

    public AsyncOperation LoadLevelAsync(string name, LoadLevelAsyncCallback cb)
    {
        if (name == null || name.Length <= 0)
        {
            return null;
        }

        string targetScene = GetLevelName(name);
        Logger.Log(string.Format("SLOTSceneManager ATTEMPT: Async loading level '{0}' (Current active: '{1}')", targetScene, SceneManager.GetActiveScene().name));

        CheckAsyncOperationDone(true);

        AsyncOperation result = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Single);

        if (cb != null)
        {
            loadLevelAsyncCallback = cb;
            asyncOperation = result;
        }
        else
        {
            loadLevelAsyncCallback = null;
            asyncOperation = null;
        }
        return result;
    }

    public AsyncOperation LoadLevelAdditiveAsync(string name)
    {
        return LoadLevelAdditiveAsync(name, null);
    }

    public AsyncOperation LoadLevelAdditiveAsync(string name, LoadLevelAsyncCallback cb)
    {
        if (name == null || name.Length <= 0)
        {
            return null;
        }

        string targetScene = GetLevelName(name);
        Logger.Log(string.Format("SLOTSceneManager ATTEMPT: Additive Async loading level '{0}' (Current active: '{1}')", targetScene, SceneManager.GetActiveScene().name));

        CheckAsyncOperationDone(true);
        AsyncOperation result = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
        if (cb != null)
        {
            loadLevelAsyncCallback = cb;
            asyncOperation = result;
        }
        else
        {
            loadLevelAsyncCallback = null;
            asyncOperation = null;
        }
        return result;
    }

    private void Update()
    {
        CheckAsyncOperationDone(false);
    }

    private void CheckAsyncOperationDone(bool forceDone)
    {
        if (asyncOperation != null && loadLevelAsyncCallback != null && (asyncOperation.isDone || forceDone))
        {
            loadLevelAsyncCallback();
            asyncOperation = null;
            loadLevelAsyncCallback = null;
        }
    }
}