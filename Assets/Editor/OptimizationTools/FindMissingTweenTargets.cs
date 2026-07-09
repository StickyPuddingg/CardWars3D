using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

public class FindMissingTweenTargets : EditorWindow
{
    private Vector2 scrollPosition;
    private List<GameObject> brokenObjects = new List<GameObject>();
    private int totalChecked = 0;
    private bool hasScanned = false;

    [MenuItem("Tools/Find Missing Tween Targets")]
    public static void ShowWindow()
    {
        // Opens the window, or focuses it if it's already open
        FindMissingTweenTargets window = EditorWindow.GetWindow<FindMissingTweenTargets>("Tween Target Finder");
        window.minSize = new Vector2(400, 300);
        window.RunScan();
    }

    public void RunScan()
    {
        brokenObjects.Clear();
        totalChecked = 0;

        UIButtonTween[] tweens = Resources.FindObjectsOfTypeAll<UIButtonTween>();

        for (int i = 0; i < tweens.Length; i++)
        {
            UIButtonTween tween = tweens[i];

            if (tween == null || tween.gameObject == null)
                continue;

            if (tween.gameObject.scene.name == null || string.IsNullOrEmpty(tween.gameObject.scene.name))
                continue;

            if (tween.gameObject.hideFlags == HideFlags.NotEditable || tween.gameObject.hideFlags == HideFlags.HideAndDontSave)
                continue;

            totalChecked++;

            if (tween.tweenTarget == null)
            {
                // Add the GameObject directly to our list so we can reference it later
                if (!brokenObjects.Contains(tween.gameObject))
                {
                    brokenObjects.Add(tween.gameObject);
                }
            }
        }

        hasScanned = true;
    }

    // This handles rendering everything inside the window automatically
    void OnGUI()
    {
        GUILayout.Label("Tween Target Finder (Unity 5.6 Compatible)", EditorStyles.boldLabel);

        if (GUILayout.Button("Rescan Scene", GUILayout.Height(30)))
        {
            RunScan();
        }

        EditorGUILayout.Space();

        if (hasScanned)
        {
            string summary = string.Format("Checked {0} components. Found {1} broken targets.", totalChecked, brokenObjects.Count);
            GUILayout.Label(summary, EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space();
            GUILayout.Label("Broken GameObjects (Click to select in Hierarchy):", EditorStyles.boldLabel);

            // --- START SCROLL VIEW ---
            // This captures everything inside it and adds a scrollbar if it exceeds the window height
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (brokenObjects.Count == 0)
            {
                GUILayout.Label("No missing targets found! Everything looks good.", EditorStyles.miniLabel);
            }
            else
            {
                for (int i = 0; i < brokenObjects.Count; i++)
                {
                    GameObject obj = brokenObjects[i];
                    if (obj == null) continue;

                    string path = obj.name;
                    try
                    {
                        path = NGUITools.GetHierarchy(obj);
                    }
                    catch (Exception) { }

                    // Draw a clickable layout button for each broken item
                    if (GUILayout.Button(path, EditorStyles.label))
                    {
                        // Ping and select the object in the Hierarchy panel when clicked
                        Selection.activeGameObject = obj;
                        EditorGUIUtility.PingObject(obj);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            // --- END SCROLL VIEW ---
        }
        else
        {
            GUILayout.Label("Click 'Rescan Scene' to start.", EditorStyles.miniLabel);
        }
    }
}