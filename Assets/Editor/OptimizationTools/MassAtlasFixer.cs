using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class NGUIAtlasProjectFixer : ScriptableWizard
{
    public enum AtlasResolution
    {
        _16 = 16,
        _32 = 32,
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        Custom_NPOT = -1
    }

    [Header("Source Detection")]

    public bool autoDetectSourceSize = false;

    public AtlasResolution sourceWidth = AtlasResolution._2048;
    public AtlasResolution sourceHeight = AtlasResolution._2048;

    public int customSourceWidth = 2048;
    public int customSourceHeight = 2048;

    [Header("Target Atlas Resolution")]

    public AtlasResolution finalWidth = AtlasResolution._512;
    public AtlasResolution finalHeight = AtlasResolution._512;

    public int customFinalWidth = 512;
    public int customFinalHeight = 512;

    private static GameObject[] targets;

    [MenuItem("Tools/3DS/Fix Selected NGUI Atlas")]
    public static void OpenWizard()
    {
        targets = Selection.gameObjects;

        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning("No NGUI Atlas prefab selected.");
            return;
        }

        DisplayWizard<NGUIAtlasProjectFixer>(
            "Scale NGUI Atlas",
            "Apply Scaling"
        );
    }

    private void OnWizardCreate()
    {
        float finalW =
            finalWidth == AtlasResolution.Custom_NPOT
            ? customFinalWidth
            : (float)(int)finalWidth;

        float finalH =
            finalHeight == AtlasResolution.Custom_NPOT
            ? customFinalHeight
            : (float)(int)finalHeight;

        if (finalW <= 0 || finalH <= 0)
        {
            Debug.LogError("Target resolutions must be greater than zero.");
            return;
        }

        int alteredCount = 0;

        foreach (GameObject prefabGo in targets)
        {
            if (prefabGo == null)
                continue;

            UIAtlas atlas = prefabGo.GetComponent<UIAtlas>();

            if (atlas == null)
                continue;

            float sourceW;
            float sourceH;

            if (autoDetectSourceSize)
            {
                if (atlas.spriteMaterial == null)
                {
                    Debug.LogWarning(
                        prefabGo.name +
                        " has no sprite material assigned."
                    );
                    continue;
                }

                Texture mainTex = atlas.spriteMaterial.mainTexture;

                if (mainTex == null)
                {
                    Debug.LogWarning(
                        prefabGo.name +
                        " sprite material has no texture assigned."
                    );
                    continue;
                }

                sourceW = mainTex.width;
                sourceH = mainTex.height;
            }
            else
            {
                sourceW =
                    sourceWidth == AtlasResolution.Custom_NPOT
                    ? customSourceWidth
                    : (float)(int)sourceWidth;

                sourceH =
                    sourceHeight == AtlasResolution.Custom_NPOT
                    ? customSourceHeight
                    : (float)(int)sourceHeight;
            }

            if (sourceW <= 0 || sourceH <= 0)
            {
                Debug.LogWarning(
                    prefabGo.name +
                    " has invalid source dimensions."
                );
                continue;
            }

            float scaleX = finalW / sourceW;
            float scaleY = finalH / sourceH;

            Debug.Log(
                string.Format(
                    "{0}: {1}x{2} -> {3}x{4} (X={5:F4}, Y={6:F4})",
                    prefabGo.name,
                    sourceW,
                    sourceH,
                    finalW,
                    finalH,
                    scaleX,
                    scaleY
                )
            );

            List<UIAtlas.Sprite> sprites = atlas.spriteList;

            if (sprites == null || sprites.Count == 0)
                continue;

            Undo.RecordObject(
                atlas,
                "Scale NGUI Atlas Sprites"
            );

            foreach (UIAtlas.Sprite sprite in sprites)
            {
                Rect outer = sprite.outer;

                outer.x *= scaleX;
                outer.y *= scaleY;
                outer.width *= scaleX;
                outer.height *= scaleY;

                sprite.outer = outer;

                Rect inner = sprite.inner;

                inner.x *= scaleX;
                inner.y *= scaleY;
                inner.width *= scaleX;
                inner.height *= scaleY;

                sprite.inner = inner;

                sprite.paddingLeft *= scaleX;
                sprite.paddingRight *= scaleX;

                sprite.paddingTop *= scaleY;
                sprite.paddingBottom *= scaleY;
            }

            atlas.MarkAsDirty();

            EditorUtility.SetDirty(atlas);
            EditorUtility.SetDirty(prefabGo);

            alteredCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            string.Format(
                "Successfully updated {0} atlas prefab(s).",
                alteredCount
            )
        );
    }

    void OnWizardUpdate()
    {
        helpString =
            autoDetectSourceSize
            ? "Source atlas size will be detected automatically from the atlas texture. Only enter the target resolution."
            : "Manual source and target sizes will be used.";

        isValid = true;
    }
}