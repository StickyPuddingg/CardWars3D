using UnityEngine;
using UnityEditor;
using System.IO;

public class Audio3DSOrganizer : EditorWindow
{
    [MenuItem("Tools/3DS/Audio/Audio Organizer")]
    static void Init()
    {
        GetWindow<Audio3DSOrganizer>("3DS Audio");
    }

    void OnGUI()
    {
        GUILayout.Label("Nintendo 3DS Audio Optimizer", EditorStyles.boldLabel);

        if (GUILayout.Button("Optimize Selected Audio"))
        {
            OptimizeSelectedAudio();
        }
    }

    static void OptimizeSelectedAudio()
    {
        Object[] selected = Selection.objects;

        if (selected == null || selected.Length == 0)
        {
            Debug.LogWarning("No audio clips selected.");
            return;
        }

        CreateFolderIfMissing("Assets/Audio");
        CreateFolderIfMissing("Assets/Audio/DecompressOnLoad");
        CreateFolderIfMissing("Assets/Audio/CompressedInMemory");
        CreateFolderIfMissing("Assets/Audio/Streaming");

        int processed = 0;

        foreach (Object obj in selected)
        {
            AudioClip clip = obj as AudioClip;

            if (clip == null)
                continue;

            string assetPath = AssetDatabase.GetAssetPath(clip);

            AudioImporter importer =
                AssetImporter.GetAtPath(assetPath) as AudioImporter;

            if (importer == null)
                continue;

            float length = clip.length;

            AudioImporterSampleSettings settings =
                importer.defaultSampleSettings;

            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.ADPCM;
            settings.sampleRateSetting =
                AudioSampleRateSetting.OverrideSampleRate;
            settings.sampleRateOverride = 22050;

            importer.forceToMono = true;
            importer.loadInBackground = true;
            importer.preloadAudioData = true;

            string targetFolder =
                "Assets/Audio/DecompressOnLoad";

            if (length < 5f)
            {
                settings.loadType =
                    AudioClipLoadType.DecompressOnLoad;

                settings.compressionFormat =
                    AudioCompressionFormat.ADPCM;

                settings.sampleRateOverride = 22050;

                targetFolder =
                    "Assets/Audio/DecompressOnLoad";
            }
            else if (length < 15f)
            {
                settings.loadType =
                    AudioClipLoadType.CompressedInMemory;

                settings.compressionFormat =
                    AudioCompressionFormat.ADPCM;

                settings.sampleRateOverride = 22050;

                targetFolder =
                    "Assets/Audio/CompressedInMemory";
            }
            else if (length < 30f)
            {
                settings.loadType =
                    AudioClipLoadType.CompressedInMemory;

                settings.compressionFormat =
                    AudioCompressionFormat.Vorbis;

                settings.sampleRateOverride = 32000;

                targetFolder =
                    "Assets/Audio/CompressedInMemory";
            }
            else
            {
                settings.loadType =
                    AudioClipLoadType.Streaming;

                settings.compressionFormat =
                    AudioCompressionFormat.Vorbis;

                settings.sampleRateOverride = 32000;

                importer.preloadAudioData = false;

                targetFolder =
                    "Assets/Audio/Streaming";
            }

            importer.defaultSampleSettings = settings;

            AssetDatabase.ImportAsset(
                assetPath,
                ImportAssetOptions.ForceUpdate);

            string fileName = Path.GetFileName(assetPath);
            string newPath = targetFolder + "/" + fileName;

            if (assetPath != newPath)
            {
                AssetDatabase.MoveAsset(assetPath, newPath);
            }

            processed++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "3DS Audio Optimizer processed " +
            processed +
            " clips.");
    }

    static void CreateFolderIfMissing(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent =
            Path.GetDirectoryName(path).Replace("\\", "/");

        string folder =
            Path.GetFileName(path);

        AssetDatabase.CreateFolder(parent, folder);
    }
}