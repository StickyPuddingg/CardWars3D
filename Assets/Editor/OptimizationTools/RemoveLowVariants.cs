using UnityEngine;
using UnityEditor;
using System.IO;

public class RemoveLowVariants
{
    [MenuItem("Tools/3DS/Remove Redundant low_ Files")]
    static void RemoveFiles()
    {
        Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

        int deleted = 0;
        int kept = 0;

        foreach (Object obj in selection)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);

            if (string.IsNullOrEmpty(assetPath))
                continue;

            string fileName = Path.GetFileName(assetPath);

            if (!fileName.StartsWith("low_"))
                continue;

            string directory = Path.GetDirectoryName(assetPath);
            string normalName = fileName.Substring(4); // Remove "low_"
            string normalPath = Path.Combine(directory, normalName).Replace("\\", "/");

            if (AssetDatabase.LoadAssetAtPath<Object>(normalPath) != null)
            {
                if (AssetDatabase.DeleteAsset(assetPath))
                {
                    Debug.Log("Deleted: " + assetPath);
                    deleted++;
                }
            }
            else
            {
                Debug.Log("Kept (no normal version): " + assetPath);
                kept++;
            }
        }

        AssetDatabase.Refresh();

        Debug.Log("Finished. Deleted " + deleted + " low_ files. Kept " + kept + ".");
    }
}