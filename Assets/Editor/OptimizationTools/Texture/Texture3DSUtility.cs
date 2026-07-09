using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

public static class Texture3DSUtility
{
    // -----------------------------
    // SIZE RULES (3DS SAFE LIMITS)
    // -----------------------------
    public const int MAX_WIDTH = 1024;
    public const int MAX_HEIGHT = 512;

    // -----------------------------
    // SIZE CHECKS
    // -----------------------------
    public static bool IsPowerOfTwo(int value)
    {
        return (value & (value - 1)) == 0;
    }

    public static bool IsNPOT(int w, int h)
    {
        return !IsPowerOfTwo(w) || !IsPowerOfTwo(h);
    }

    public static bool IsOversized(int w, int h)
    {
        return w > MAX_WIDTH || h > MAX_HEIGHT || w > MAX_HEIGHT || h > MAX_WIDTH;
    }

    public static string GetSizeFolder(int w, int h)
    {
        return w + "x" + h;
    }

    // -----------------------------
    // ALPHA DETECTION (IMPORTER)
    // -----------------------------
    public static bool HasAlpha(TextureImporter importer)
    {
        if (importer == null) return false;
        return importer.DoesSourceTextureHaveAlpha();
    }

    // -----------------------------
    // VRAM ESTIMATION (VERY ROUGH BUT USEFUL)
    // -----------------------------
    public static int EstimateVRAM(int width, int height, bool hasAlpha)
    {
        // 3DS ETC2-like compression approximation
        int bitsPerPixel = hasAlpha ? 8 : 4;
        return (width * height * bitsPerPixel) / 8;
    }

    // -----------------------------
    // TEXTURE SCORING SYSTEM
    // -----------------------------
    public static int GetTextureScore(TextureImporter importer, int w, int h)
    {
        int score = 100;

        if (IsOversized(w, h)) score -= 30;
        if (IsNPOT(w, h)) score -= 10;
        if (importer.mipmapEnabled) score -= 10;
        if (importer.isReadable) score -= 20;
        if (importer.textureCompression == TextureImporterCompression.Uncompressed) score -= 25;

        return Mathf.Clamp(score, 0, 100);
    }

    public static string GetScoreLabel(int score)
    {
        if (score >= 90) return "A+";
        if (score >= 80) return "A";
        if (score >= 70) return "B";
        if (score >= 60) return "C";
        if (score >= 50) return "D";
        return "F";
    }

    // -----------------------------
    // DUPLICATE DETECTION (HASH)
    // -----------------------------
    public static string GetTextureHash(string path)
    {
        byte[] data = File.ReadAllBytes(path);

        MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(data);

        return System.BitConverter.ToString(hash);
    }

    // -----------------------------
    // ORGANIZATION HELPERS
    // -----------------------------
    public static string GetBaseFolder(string root)
    {
        return root.TrimEnd('/');
    }

    public static string GetAlphaFolder(string root)
    {
        return GetBaseFolder(root) + "/Alpha";
    }

    public static string GetOpaqueFolder(string root)
    {
        return GetBaseFolder(root) + "/Opaque";
    }

    public static string GetNPOTFolder(string root)
    {
        return GetBaseFolder(root) + "/NPOT";
    }

    public static string GetUIFolder(string root)
    {
        return GetBaseFolder(root) + "/UI";
    }

    public static string GetSpriteFolder(string root)
    {
        return GetBaseFolder(root) + "/Sprites";
    }

    // -----------------------------
    // FOLDER CREATION
    // -----------------------------
    public static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string name = Path.GetFileName(path);

        if (!AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    // -----------------------------
    // CATEGORY DETECTION
    // -----------------------------
    public static string GetTextureCategory(TextureImporter importer)
    {
        if (importer.textureType == TextureImporterType.GUI)
            return "UI";

        if (importer.textureType == TextureImporterType.Sprite)
            return "Sprites";

        if (importer.textureType == TextureImporterType.NormalMap)
            return "NormalMaps";

        return "Default";
    }

    // -----------------------------
    // FILTER HELPERS
    // -----------------------------
    public static void ForcePointFilter(TextureImporter importer)
    {
        importer.filterMode = FilterMode.Point;
    }

    public static void ForceBilinear(TextureImporter importer)
    {
        importer.filterMode = FilterMode.Bilinear;
    }

    // -----------------------------
    // WRAP MODE HELPERS
    // -----------------------------
    public static void ForceClamp(TextureImporter importer)
    {
        importer.wrapMode = TextureWrapMode.Clamp;
    }

    public static void ForceRepeat(TextureImporter importer)
    {
        importer.wrapMode = TextureWrapMode.Repeat;
    }
}