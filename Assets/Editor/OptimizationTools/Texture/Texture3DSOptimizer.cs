using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class Texture3DSOptimizer : EditorWindow
{
    // -----------------------------
    // SETTINGS
    // -----------------------------
    private bool showKnownIssues = false;

    private bool generateMipmaps = false;
    private bool organizeFolders = true;
    private bool separateAlpha = true;
    private bool extremeCompression = false;
    private bool analyzeOnly = true;

    private bool forcePointFilter = true;
    private bool disableReadWrite = true;
    private bool modifyMaxSize = true;

    private string rootFolder = "Assets/Texture2D";

    private Vector2 scroll;

    private string lastReport = "";

    // -----------------------------
    // MENU
    // -----------------------------
    [MenuItem("Tools/3DS/Texture Optimizer")]
    public static void Open()
    {
        GetWindow<Texture3DSOptimizer>("3DS Texture Optimizer");
    }

    // -----------------------------
    // UI
    // -----------------------------
    void OnGUI()
    {
        GUILayout.Label("Nintendo 3DS Texture Optimizer", EditorStyles.boldLabel);

        scroll = GUILayout.BeginScrollView(scroll);
        if (showKnownIssues)
        {
            GUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "⚠️ Known Unity 5.6 / 3DS Issue:\n\n" +
                "After optimizing, the texture inspector may show RGBA32 or prompt 'Apply Changes'.\n\n" +
                "This is a Unity importer UI sync bug. The actual 3DS platform compression is still applied correctly.\n\n" +
                "Verify results in texture preview (bottom of inspector) or final build output.\n\n" +
                "Tip: Organizing folders helps avoid repeated reimport confusion.",
                MessageType.Warning
            );


        }

        showKnownIssues = EditorGUILayout.Foldout(showKnownIssues, "Known Issues (3DS / Unity 5.6)");

        analyzeOnly = EditorGUILayout.Toggle("Analyze Only", analyzeOnly);

        GUILayout.Space(5);

        organizeFolders = EditorGUILayout.Toggle("Organize Into Folders", organizeFolders);
        separateAlpha = EditorGUILayout.Toggle("Separate Alpha / Opaque", separateAlpha);

        generateMipmaps = EditorGUILayout.Toggle("Generate Mipmaps", generateMipmaps);
        disableReadWrite = EditorGUILayout.Toggle("Disable Read/Write", disableReadWrite);

        modifyMaxSize = EditorGUILayout.Toggle("Modify Max Size", modifyMaxSize);

        forcePointFilter = EditorGUILayout.Toggle("Force Point Filter", forcePointFilter);

        extremeCompression = EditorGUILayout.Toggle("Extreme Compression", extremeCompression);

        rootFolder = EditorGUILayout.TextField("Root Folder", rootFolder);

        GUILayout.Space(10);

        if (GUILayout.Button("ANALYZE SELECTED TEXTURES"))
        {
            AnalyzeSelection();
        }

        if (!analyzeOnly)
        {
            if (GUILayout.Button("OPTIMIZE SELECTED TEXTURES"))
            {
                OptimizeSelection();
            }
        }

        GUILayout.Space(10);

        GUILayout.Label("Last Report:", EditorStyles.boldLabel);
        GUILayout.TextArea(lastReport, GUILayout.Height(250));

        GUILayout.EndScrollView();
    }

    // -----------------------------
    // ANALYZE
    // -----------------------------
    void AnalyzeSelection()
    {
        List<string> paths = GetSelectedTexturePaths();

        Texture3DSAnalyzer.Report report =
            Texture3DSAnalyzer.Analyze(paths);

        lastReport =
            Texture3DSAnalyzer.BuildReportString(report);

        Debug.Log(lastReport);
    }

    // -----------------------------
    // OPTIMIZE
    // -----------------------------
    void OptimizeSelection()
    {
        List<string> paths = GetSelectedTexturePaths();

        int processed = 0;

        for (int i = 0; i < paths.Count; i++)
        {
            string path = paths[i];

            EditorUtility.DisplayProgressBar(
                "Optimizing Textures",
                path,
                (float)i / paths.Count
            );

            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
                continue;

            Texture3DSProcessor.Process(
                importer,
                path,
                this
            );

            processed++;
        }

        EditorUtility.ClearProgressBar();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Optimized textures: " + processed);
    }

    // -----------------------------
    // GET SELECTION
    // -----------------------------
    List<string> GetSelectedTexturePaths()
    {
        Object[] selection =
            Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);

        List<string> paths = new List<string>();

        foreach (Object obj in selection)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (!string.IsNullOrEmpty(path))
                paths.Add(path);
        }

        return paths;
    }

    // -----------------------------
    // PUBLIC SETTINGS ACCESS (for processor)
    // -----------------------------
    public bool GenerateMipmaps { get { return generateMipmaps; } }
    public bool OrganizeFolders { get { return organizeFolders; } }
    public bool SeparateAlpha { get { return separateAlpha; } }
    public bool ExtremeCompression { get { return extremeCompression; } }
    public bool AnalyzeOnly { get { return analyzeOnly; } }
    public bool ForcePointFilter { get { return forcePointFilter; } }
    public bool DisableReadWrite { get { return disableReadWrite; } }
    public bool ModifyMaxSize { get { return modifyMaxSize; } }
    public string RootFolder { get { return rootFolder; } }
}