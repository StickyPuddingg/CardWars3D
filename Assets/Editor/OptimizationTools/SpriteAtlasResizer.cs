using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteAtlasResizer : EditorWindow
{
    private Texture2D targetTexture;
    private float scaleFactor = 0.25f; // 2048 down to 512 is 1/4 (0.25)

    [MenuItem("Tools/3DS/Sprite Atlas Resizer")]
    public static void ShowWindow()
    {
        GetWindow<SpriteAtlasResizer>("Atlas Resizer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Resize Sprite Sheet Metadata", EditorStyles.boldLabel);

        targetTexture = (Texture2D)EditorGUILayout.ObjectField("Texture/Atlas", targetTexture, typeof(Texture2D), false);
        scaleFactor = EditorGUILayout.FloatField("Scale Factor (e.g. 0.25)", scaleFactor);

        if (GUILayout.Button("Resize Sprites") && targetTexture != null)
        {
            ResizeSprites();
        }
    }

    private void ResizeSprites()
    {
        string path = AssetDatabase.GetAssetPath(targetTexture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer == null)
        {
            Debug.LogError("Selected object is not a valid texture asset.");
            return;
        }

        // Force sprite mode to Multiple to access sheets safely
        if (importer.spriteImportMode != SpriteImportMode.Multiple)
        {
            Debug.LogError("Texture is not configured with Sprite Mode: Multiple.");
            return;
        }

        SpriteMetaData[] metaData = importer.spritesheet;

        for (int i = 0; i < metaData.Length; i++)
        {
            Rect rect = metaData[i].rect;

            // Multiply pixel coordinates by the scale factor
            rect.x *= scaleFactor;
            rect.y *= scaleFactor;
            rect.width *= scaleFactor;
            rect.height *= scaleFactor;

            metaData[i].rect = rect;

            // Optional: If you use sprite borders (sliced UI images), scale them too
            Vector4 border = metaData[i].border;
            border *= scaleFactor;
            metaData[i].border = border;
        }

        // Apply changes back to the asset importer
        importer.spritesheet = metaData;

        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();

        Debug.Log("Successfully resized "+ metaData.Length +" sprites on " + targetTexture.name + "!");
    }
}