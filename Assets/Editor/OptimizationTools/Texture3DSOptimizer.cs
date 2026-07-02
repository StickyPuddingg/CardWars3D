using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class Texture3DSOptimizer : EditorWindow
{
    private bool generateMipmaps = false;
    private bool extremeCompression = false;
    private string rootFolder = "Assets/Textures";

    [MenuItem("Tools/3DS/Texture Optimizer")]
    static void Open()
    {
        GetWindow<Texture3DSOptimizer>("3DS Texture Optimizer");
    }

    void OnGUI()
    {
        GUILayout.Label("Nintendo 3DS Texture Optimizer", EditorStyles.boldLabel);

        generateMipmaps = EditorGUILayout.Toggle(
            "Generate Mip Maps",
            generateMipmaps);

        extremeCompression = EditorGUILayout.Toggle(
            "Extreme Compression",
            extremeCompression);

        rootFolder = EditorGUILayout.TextField(
            "Organization Root",
            rootFolder);

        GUILayout.Space(10);

        if (GUILayout.Button("Process Selected Textures"))
        {
            ProcessSelection();
        }
    }

    void ProcessSelection()
    {
        Object[] selection = Selection.GetFiltered(
            typeof(Texture2D),
            SelectionMode.DeepAssets);

        int processed = 0;

        foreach (Object obj in selection)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
                continue;

            OptimizeTexture(importer, path);

            processed++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Processed " + processed + " textures.");
    }

    void OptimizeTexture(TextureImporter importer, string path)
    {
        Texture2D tex =
            AssetDatabase.LoadAssetAtPath<Texture2D>(path);

        if (tex == null)
            return;

        int width = tex.width;
        int height = tex.height;

        bool npot =
            !IsPowerOfTwo(width) ||
            !IsPowerOfTwo(height);

        bool hasAlpha = importer.DoesSourceTextureHaveAlpha();

        //--------------------------------------------------
        // General
        //--------------------------------------------------

        importer.mipmapEnabled = generateMipmaps;

        importer.alphaSource =
            hasAlpha ?
            TextureImporterAlphaSource.FromInput :
            TextureImporterAlphaSource.None;

        importer.alphaIsTransparency = hasAlpha;

        importer.textureCompression =
            TextureImporterCompression.Compressed;

        //--------------------------------------------------
        // 3DS platform override
        //--------------------------------------------------

        TextureImporterPlatformSettings n3ds =
            importer.GetPlatformTextureSettings("Nintendo 3DS");

        n3ds.overridden = true;

        n3ds.maxTextureSize =
            CalculateMaxSize(width, height);

        n3ds.format =
            TextureImporterFormat.ETC_RGB4;

        importer.SetPlatformTextureSettings(n3ds);

        //--------------------------------------------------
        // Move file
        //--------------------------------------------------

        MoveTexture(path, width, height, npot);

        importer.SaveAndReimport();
    }

    int CalculateMaxSize(int width, int height)
    {
        int largest = Mathf.Max(width, height);

        if (extremeCompression)
            return Mathf.Max(32, largest / 4);

        return Mathf.Max(32, largest / 2);
    }

    void MoveTexture(
        string path,
        int width,
        int height,
        bool npot)
    {
        string folder;

        if (npot)
        {
            folder =
                rootFolder +
                "/NPOT/" +
                width + "x" + height;
        }
        else
        {
            folder =
                rootFolder +
                "/" +
                width + "x" + height;
        }

        CreateFolderRecursive(folder);

        string filename = Path.GetFileName(path);

        string destination =
            folder + "/" + filename;

        if (path != destination)
        {
            AssetDatabase.MoveAsset(
                path,
                destination);
        }
    }

    static bool IsPowerOfTwo(int value)
    {
        return (value & (value - 1)) == 0;
    }

    static void CreateFolderRecursive(string folder)
    {
        string[] parts = folder.Split('/');

        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];

            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(
                    current,
                    parts[i]);
            }

            current = next;
        }
    }
}