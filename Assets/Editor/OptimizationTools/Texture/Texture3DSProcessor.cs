using UnityEngine;
using UnityEditor;
using System.IO;

public static class Texture3DSProcessor
{
    // -----------------------------
    // MAIN ENTRY
    // -----------------------------
    public static void Process(TextureImporter importer, string path, Texture3DSOptimizer settings)
    {
        if (importer == null)
            return;

        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null)
            return;

        int width = tex.width;
        int height = tex.height;

        bool hasAlpha = Texture3DSUtility.HasAlpha(importer);
        bool npot = Texture3DSUtility.IsNPOT(width, height);
        bool oversized = Texture3DSUtility.IsOversized(width, height);

        // -----------------------------
        // OVERSIZE WARNING (3DS RULE)
        // -----------------------------
        if (oversized)
        {
            Debug.LogWarning(
                "[3DS Texture Warning] " + path +
                " is " + width + "x" + height +
                " (exceeds 1024x512 limit)"
            );
        }

        // -----------------------------
        // BASIC SETTINGS
        // -----------------------------

        importer.mipmapEnabled = settings.GenerateMipmaps;

        // FIX: properly disable mipmaps when OFF
        if (!settings.GenerateMipmaps)
        {
            importer.mipmapEnabled = false;
        }

        importer.isReadable = !settings.DisableReadWrite ? importer.isReadable : false;

        importer.alphaIsTransparency = hasAlpha;

        importer.alphaSource =
            hasAlpha
            ? TextureImporterAlphaSource.FromInput
            : TextureImporterAlphaSource.None;

        importer.textureCompression =
            TextureImporterCompression.Compressed;

        // -----------------------------
        // FILTER MODE
        // -----------------------------
        if (settings.ForcePointFilter)
        {
            importer.filterMode = FilterMode.Point;
        }

        // -----------------------------
        // PLATFORM SETTINGS (3DS)
        // -----------------------------
        TextureImporterPlatformSettings n3ds =
            importer.GetPlatformTextureSettings("Nintendo 3DS");

        n3ds.overridden = true;

        if (settings.ModifyMaxSize)
        {
            n3ds.maxTextureSize =
                CalculateMaxSize(width, height, settings.ExtremeCompression);
        }

        if (hasAlpha)
        {
            n3ds.format = TextureImporterFormat.ETC2_RGBA8;
        }
        else
        {
            n3ds.format = TextureImporterFormat.ETC_RGB4;
        }

        importer.SetPlatformTextureSettings(n3ds);

        // -----------------------------
        // ORGANIZATION
        // -----------------------------
        if (settings.OrganizeFolders)
        {
            MoveTexture(importer, path, width, height, hasAlpha, npot, settings);
        }

        // -----------------------------
        // APPLY
        // -----------------------------
    }

    // -----------------------------
    // MAX SIZE RULES
    // -----------------------------
    private static int CalculateMaxSize(int w, int h, bool extreme)
    {
        int largest = Mathf.Max(w, h);

        if (extreme)
            return Mathf.Clamp(largest / 4, 32, 1024);

        return Mathf.Clamp(largest / 2, 32, 1024);
    }

    // -----------------------------
    // MOVE / ORGANIZE
    // -----------------------------
    private static void MoveTexture(
        TextureImporter importer,
        string path,
        int width,
        int height,
        bool hasAlpha,
        bool npot,
        Texture3DSOptimizer settings)
    {
        string root = settings.RootFolder;

        string category =
            Texture3DSUtility.GetTextureCategory(importer);

        string baseFolder = root;

        // Alpha / Opaque separation
        if (settings.SeparateAlpha)
        {
            baseFolder += hasAlpha
                ? "/Alpha"
                : "/Opaque";
        }

        // Add category (UI / Sprites / NormalMaps)
        if (category != "Default")
        {
            baseFolder += "/" + category;
        }

        // NPOT handling
        if (npot)
        {
            baseFolder += "/NPOT";
        }

        // Size folder
        baseFolder += "/" + Texture3DSUtility.GetSizeFolder(width, height);

        Texture3DSUtility.EnsureFolder(baseFolder);

        string fileName = Path.GetFileName(path);
        string destination = baseFolder + "/" + fileName;

        if (path != destination)
        {
            AssetDatabase.MoveAsset(path, destination);
        }
    }
}