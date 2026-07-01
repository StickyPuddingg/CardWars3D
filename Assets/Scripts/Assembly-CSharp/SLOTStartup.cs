using System;
using UnityEngine;

/* =================================================================================
 * 3DS PORT MIGRATION SUMMARY:
 * - DELETED `GooglePlayDownloader`: Completely removed all Android JNI hooks, 
 * OBB path strings, and Google Play Store server checks.
 * - PURGED COROUTINE OVERHEAD: Eliminated `loadLevel()` and its `WaitForSeconds` 
 * polling loop, saving precious CPU cycles on the 3DS ARM processor.
 * - DIRECT BOOTSTRAPPING: Configured `Update()` to transition directly to the 
 * main game scene on frame one since assets are already stored locally
 * ================================================================================= */

public class SLOTStartup : MonoBehaviour
{
    public string startupScene = "AdventureTime";
    private bool started;

    private void Update()
    {
        if (!started)
        {
            started = true;
            Startup();
        }
    }

    private void Startup()
    {
        if (!string.IsNullOrEmpty(startupScene))
        {
            try
            {
                // Launch straight into your main scene manager
                SLOTGameSingleton<SLOTSceneManager>.GetInstance().LoadLevel(startupScene);
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to boot startup scene on 3DS: " + ex.Message);
            }
        }
    }
}