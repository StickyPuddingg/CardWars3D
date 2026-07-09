using System;
using UnityEngine;

public class TestChestLoader : MonoBehaviour
{
    // Paste your exact resource paths here in the inspector to test them
    public string halloweenPath = "props/HalloweenChest";
    public string christmasPath = "props/ChristmasChest";

    private void Start()
    {
        Logger.Log("--- Starting Gacha Chest Path Test ---");

        TestLoad(halloweenPath, "Halloween");
        TestLoad(christmasPath, "Christmas");
    }

    private void TestLoad(string path, string chestName)
    {
        if (string.IsNullOrEmpty(path))
        {
            Logger.Warn(string.Format("{0} Test: Path is empty!", chestName));
            return;
        }

        // Attempt to load the prefab from the Resources folder
        GameObject prefab = Resources.Load<GameObject>(path);

        if (prefab != null)
        {
            Logger.Log(string.Format("SUCCESS: Found {0} prefab at: Assets/Resources/{1}", chestName, path));

            // Instantiates it right where this test object is sitting
            GameObject instance = (GameObject)Instantiate(prefab, transform.position, transform.rotation);
            instance.name = "TEST_" + chestName;
        }
        else
        {
            Logger.Error(string.Format("FAILED: Could not find {0} prefab at: Assets/Resources/{1}.prefab. Double check your folder structure and typos!", chestName, path));
        }
    }
}