using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ParticleShaderSwapper : EditorWindow
{
    private enum TargetScope
    {
        SelectedObjectsOnly,
        EntireActiveScene,
        ProjectFolderPrefabsAndMaterials
    }

    private TargetScope targetScope = TargetScope.SelectedObjectsOnly;
    private const string TargetShaderName = "Mobile/Particles/Alpha Blended";

    [MenuItem("Tools/3DS/Materials/ParticleShaderSwapper")]
    public static void ShowWindow()
    {
        GetWindow<ParticleShaderSwapper>("Particle Shader Swapper");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Particle Shader Swapper", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        targetScope = (TargetScope)EditorGUILayout.EnumPopup("Target Scope", targetScope);

        EditorGUILayout.Space();
        if (GUILayout.Button("Swap Shaders to Alpha Blended", GUILayout.Height(30)))
        {
            ExecuteShaderSwap();
        }
    }

    private void ExecuteShaderSwap()
    {
        Shader targetShader = Shader.Find(TargetShaderName);
        if (targetShader == null)
        {
            Debug.LogError(string.Format("[Shader Swapper] Could not find shader: '{0}'. Make sure it is included in your project built-in shaders.", TargetShaderName));
            return;
        }

        int changedCount = 0;

        switch (targetScope)
        {
            case TargetScope.SelectedObjectsOnly:
                changedCount = ProcessObjects(Selection.gameObjects, targetShader);
                break;

            case TargetScope.EntireActiveScene:
                // Find all root objects in the current active scene
                GameObject[] sceneObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                changedCount = ProcessObjects(sceneObjects, targetShader);
                break;

            case TargetScope.ProjectFolderPrefabsAndMaterials:
                changedCount = ProcessProjectFiles(targetShader);
                break;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success!", string.Format("Successfully swapped {0} materials/renderers to '{1}'.", changedCount, TargetShaderName), "OK");
    }

    private int ProcessObjects(GameObject[] rootObjects, Shader targetShader)
    {
        int count = 0;
        HashSet<Material> processedMaterials = new HashSet<Material>();

        foreach (GameObject obj in rootObjects)
        {
            if (obj == null) continue;

            // Grab ParticleSystemRenderers and MeshRenderers (in case of legacy/mesh particles)
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer ren in renderers)
            {
                // Use sharedMaterials to modify the actual asset/scene file rather than instancing
                Material[] sharedMats = ren.sharedMaterials;
                bool rendererChanged = false;

                for (int i = 0; i < sharedMats.Length; i++)
                {
                    Material mat = sharedMats[i];
                    if (mat != null && mat.shader != targetShader)
                    {
                        // Filter to make sure we are primarily affecting particle shaders or unlit elements
                        if (mat.shader.name.ToLower().Contains("particle") || mat.name.ToLower().Contains("particle"))
                        {
                            Undo.RecordObject(mat, "Swap Particle Shader");
                            mat.shader = targetShader;
                            processedMaterials.Add(mat);
                            rendererChanged = true;
                            count++;
                        }
                    }
                }

                if (rendererChanged)
                {
                    Undo.RecordObject(ren, "Update Renderer Materials");
                    EditorUtility.SetDirty(ren);
                }
            }
        }

        // Mark all unique modified materials as dirty so Unity saves them
        foreach (Material mat in processedMaterials)
        {
            EditorUtility.SetDirty(mat);
        }

        return count;
    }

    private int ProcessProjectFiles(Shader targetShader)
    {
        int count = 0;

        // 1. Find and process all Material assets in the project folder
        string[] matGuids = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in matGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat != null && mat.shader != targetShader)
            {
                if (mat.shader.name.ToLower().Contains("particle") || mat.name.ToLower().Contains("particle"))
                {
                    mat.shader = targetShader;
                    EditorUtility.SetDirty(mat);
                    count++;
                }
            }
        }

        // 2. Find and process all Prefab assets in the project folder to catch embedded material references
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
                bool prefabDirty = false;

                foreach (Renderer ren in renderers)
                {
                    Material[] sharedMats = ren.sharedMaterials;
                    for (int i = 0; i < sharedMats.Length; i++)
                    {
                        Material mat = sharedMats[i];
                        if (mat != null && mat.shader != targetShader)
                        {
                            if (mat.shader.name.ToLower().Contains("particle") || mat.name.ToLower().Contains("particle"))
                            {
                                mat.shader = targetShader;
                                EditorUtility.SetDirty(mat);
                                prefabDirty = true;
                                count++;
                            }
                        }
                    }
                }

                if (prefabDirty)
                {
                    EditorUtility.SetDirty(prefab);
                }
            }
        }

        return count;
    }
}