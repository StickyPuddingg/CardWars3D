using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MiniJSON;
using UnityEngine;

/// ponytail: removed server asset download logic, analytics, network stuff
public class SLOTGame_dbl : BusyIconController
{
    public enum AssetBundleType { Local, Disabled }

    public AssetBundleType assetBundleType;
    public string errorPopupPrefab;

    private static SLOTGame_dbl the_instance;

    public static SLOTGame_dbl GetInstance()
    {
        if (the_instance == null)
        {
            the_instance = FindObjectOfType(typeof(SLOTGame_dbl)) as SLOTGame_dbl;
        }
        if (the_instance == null && Application.isEditor && Application.isPlaying)
        {
            GameObject go = Resources.Load("SLOTGame") as GameObject;
            if (go == null)
                go = new GameObject();
            else
                go = (GameObject)Instantiate(go);

            if (go != null)
            {
                go.transform.position = new Vector3(999999f, 999999f, 999999f);
                go.name = "AutomaticallyCreatedGame";
                DontDestroyOnLoad(go);
                the_instance = go.AddComponent<SLOTGame_dbl>();
            }
        }
        return the_instance;
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        AuthScreenController.AuthStarted = true;
        UnityEngine.Object[] instances = FindObjectsOfType(typeof(SLOTGame_dbl));
        for (int i = 0; i < instances.Length; i++)
        {
            SLOTGame_dbl inst = instances[i] as SLOTGame_dbl;
            if (inst != null && inst.gameObject.name == "AutomaticallyCreatedGame" && inst != this)
                DestroyImmediate(inst.gameObject);
        }
        DontDestroyOnLoad(base.gameObject);
        ShowBusyIcon(false);
    }

    private void Update()
    {
        SetupScreen();
    }

    private void SetupScreen()
    {
        string device = SystemInfo.deviceModel.ToLower();
        if (device.Contains("amazon"))
        {
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
    }

    // Local persistence
    public static byte[] StringToBytes(string str)
    {
        return str == null ? null : Encoding.UTF8.GetBytes(str);
    }

    public static string BytesToString(byte[] bytes)
    {
        return bytes == null ? null : Encoding.UTF8.GetString(bytes);
    }

    public static void SavePersistentData(string filename, byte[] bytes)
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, filename);
            new FileInfo(path).Directory.Create();
            File.WriteAllBytes(path, bytes);
        }
        catch { }
    }

    public static byte[] LoadPersistentData(string filename)
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, filename);
            if (File.Exists(path))
                return File.ReadAllBytes(path);
            return null;
        }
        catch { return null; }
    }

    public static bool DoesPersistentDataExist(string filename)
    {
        try
        {
            return File.Exists(Path.Combine(Application.persistentDataPath, filename));
        }
        catch { return false; }
    }

    // Utility
    public static bool IsLowEndDevice()
    {
        return KFFLODManager.IsLowEndDevice();
    }

    public static Vector3 GetScale(Transform t)
    {
        Vector3 result = new Vector3(1f, 1f, 1f);
        if (t != null)
        {
            result.x *= t.localScale.x;
            result.y *= t.localScale.y;
            result.z *= t.localScale.z;
            if (t.parent != null)
            {
                Vector3 parentScale = GetScale(t.parent);
                result.x *= parentScale.x;
                result.y *= parentScale.y;
                result.z *= parentScale.z;
            }
        }
        return result;
    }

    public static Bounds GetBoundsRecursive(Transform t)
    {
        Collider[] colliders = t.GetComponentsInChildren<Collider>();
        Bounds result = new Bounds(t.position, Vector3.zero);
        bool first = true;
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider col = colliders[i];
            if (col != null && col.bounds.size.magnitude > 0)
            {
                if (first) { result = col.bounds; first = false; }
                else result.Encapsulate(col.bounds);
            }
        }
        return result;
    }

    public static void SetLayerRecursive(GameObject obj, int layer)
    {
        if (obj == null) return;
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    public static GameObject InstantiateGO(GameObject original)
    {
        GameObject go = PoolManager.Fetch(original);
        if (go != null) OnInstantiate(go);
        return go;
    }

    public static GameObject InstantiateGO(GameObject original, Vector3 position, Quaternion rotation)
    {
        GameObject go = PoolManager.Fetch(original, position, rotation);
        if (go != null) OnInstantiate(go);
        return go;
    }

    public static GameObject InstantiateGO(string poolName, Vector3 position, Quaternion rotation)
    {
        GameObject go = PoolManager.Fetch(poolName, position, rotation);
        if (go != null) OnInstantiate(go);
        return go;
    }

    private static void OnInstantiate(UnityEngine.Object clone)
    {
        GameObject go = clone as GameObject;
        if (go != null)
            SLOTGameSingleton<SLOTAudioManager>.GetInstance().UpdateAudioVolumes(go);
    }

    public SimpleErrorPopup ShowErrorPopup(string title, string message, int buttonCount, string[] buttonStrings, SimpleErrorPopup.ErrorPopupButtonClickedCallback callback)
    {
        GameObject prefab = SLOTGameSingleton<SLOTResourceManager>.GetInstance().LoadResource(errorPopupPrefab, typeof(GameObject)) as GameObject;
        if (prefab == null) return null;

        GameObject popup = InstantiateFX(prefab) as GameObject;
        if (popup == null) return null;

        SimpleErrorPopup popupScript = popup.GetComponent<SimpleErrorPopup>();
        if (popupScript != null)
            popupScript.Setup(title, message, buttonCount, buttonStrings, callback);
        else
            Destroy(popup);

        return popupScript;
    }

    public static UnityEngine.Object InstantiateFX(UnityEngine.Object original)
    {
        return Instantiate(original);
    }

    public static UnityEngine.Object InstantiateFX(UnityEngine.Object original, Vector3 position, Quaternion rotation)
    {
        return Instantiate(original, position, rotation);
    }
}
