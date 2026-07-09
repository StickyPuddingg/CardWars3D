using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class Texture3DSAnalyzer
{
    public class Report
    {
        public int total;
        public int alpha;
        public int opaque;
        public int npot;
        public int oversized;
        public int readable;
        public int mipmaps;
        public int uncompressed;
        public int duplicates;

        public long estimatedBefore;
        public long estimatedAfter;

        public int worstScore = 100;
        public string worstTexture = "";

        public Dictionary<string, string> hashMap = new Dictionary<string, string>();
    }

    // -----------------------------
    // MAIN ANALYZE ENTRY
    // -----------------------------
    public static Report Analyze(List<string> paths)
    {
        Report report = new Report();

        foreach (string path in paths)
        {
            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;

            Texture2D tex =
                AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            if (importer == null || tex == null)
                continue;

            int w = tex.width;
            int h = tex.height;

            report.total++;

            bool hasAlpha = Texture3DSUtility.HasAlpha(importer);

            if (hasAlpha) report.alpha++;
            else report.opaque++;

            if (Texture3DSUtility.IsNPOT(w, h))
                report.npot++;

            if (Texture3DSUtility.IsOversized(w, h))
                report.oversized++;

            if (importer.isReadable)
                report.readable++;

            if (importer.mipmapEnabled)
                report.mipmaps++;

            if (importer.textureCompression == TextureImporterCompression.Uncompressed)
                report.uncompressed++;

            // VRAM
            report.estimatedBefore += Texture3DSUtility.EstimateVRAM(w, h, hasAlpha);

            bool wouldHaveAlpha = hasAlpha;
            int estimatedAfterPixelReduction = GetReducedSize(w, h);
            report.estimatedAfter += Texture3DSUtility.EstimateVRAM(
                estimatedAfterPixelReduction,
                estimatedAfterPixelReduction,
                wouldHaveAlpha
            );

            // Score
            int score = Texture3DSUtility.GetTextureScore(importer, w, h);

            if (score < report.worstScore)
            {
                report.worstScore = score;
                report.worstTexture = path;
            }

            // Duplicate detection
            string hash = Texture3DSUtility.GetTextureHash(path);

            if (report.hashMap.ContainsKey(hash))
            {
                report.duplicates++;
            }
            else
            {
                report.hashMap.Add(hash, path);
            }
        }

        return report;
    }

    // -----------------------------
    // SIMPLE DOWNGRADE MODEL
    // -----------------------------
    private static int GetReducedSize(int w, int h)
    {
        int max = Mathf.Max(w, h);

        if (max > 1024)
            return 512;

        if (max > 512)
            return 256;

        return Mathf.Max(w, h);
    }

    // -----------------------------
    // FORMAT REPORT STRING
    // -----------------------------
    public static string BuildReportString(Report r)
    {
        return
            "========== 3DS TEXTURE REPORT ==========\n" +
            "Total Textures: " + r.total + "\n\n" +

            "Alpha: " + r.alpha + "\n" +
            "Opaque: " + r.opaque + "\n" +
            "NPOT: " + r.npot + "\n" +
            "Oversized (>1024x512): " + r.oversized + "\n" +
            "Read/Write Enabled: " + r.readable + "\n" +
            "Mipmaps Enabled: " + r.mipmaps + "\n" +
            "Uncompressed: " + r.uncompressed + "\n" +
            "Duplicates: " + r.duplicates + "\n\n" +

            "Worst Texture:\n" + r.worstTexture + "\n" +
            "Score: " + r.worstScore + "\n\n" +

            "Estimated VRAM Before: " +
            (r.estimatedBefore / 1024f / 1024f).ToString("F2") + " MB\n" +

            "Estimated VRAM After: " +
            (r.estimatedAfter / 1024f / 1024f).ToString("F2") + " MB\n\n" +

            "Savings: " +
            ((r.estimatedBefore - r.estimatedAfter) / 1024f / 1024f).ToString("F2") + " MB";
    }
}