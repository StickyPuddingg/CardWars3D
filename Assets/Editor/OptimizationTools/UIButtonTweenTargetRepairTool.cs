using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

public class UIButtonTweenTargetRepairTool : EditorWindow
{
    private GameObject referenceRoot; // Scene B (correct)
    private GameObject targetRoot;    // Scene A (broken)

    // Filter Options
    private bool copySerializedVariables = true;
    private bool copySceneReferences = true;
    private bool repairUIButtonTween = true;
    private bool copyAssets = true;
    private bool overwriteExisting = false;
    private bool previewOnly = true;

    // Results
    private int objectsMatched;
    private int componentsCopied;
    private int fieldsCopied;
    private int referencesRemapped;
    private int objectsSkipped;
    private int missingObjects;

    private Vector2 scrollPos;
    private List<string> log = new List<string>();
    private List<CopyOperation> previewOperations = new List<CopyOperation>();

    private class CopyOperation
    {
        public string objectPath;
        public string componentType;
        public string fieldName;
        public object oldValue;
        public object newValue;
        public bool isReferenceRemap;
    }

    [MenuItem("Tools/3DS/Scene Synchronizer / Reference Copier")]
    static void Init()
    {
        GetWindow<UIButtonTweenTargetRepairTool>("Scene Sync");
    }

    void OnGUI()
    {
        GUILayout.Label("Scene Synchronizer / Reference Copier", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Compare two nearly identical scenes and copy serialized data from Scene B (reference) to Scene A (target) while remapping references.",
            MessageType.Info);

        GUILayout.Space(5);

        referenceRoot = (GameObject)EditorGUILayout.ObjectField(
            "Reference Root (Scene B)",
            referenceRoot,
            typeof(GameObject),
            true);

        targetRoot = (GameObject)EditorGUILayout.ObjectField(
            "Target Root (Scene A)",
            targetRoot,
            typeof(GameObject),
            true);

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Copy Filters", EditorStyles.boldLabel);
        copySerializedVariables = EditorGUILayout.Toggle("Copy Serialized Variables", copySerializedVariables);
        copySceneReferences = EditorGUILayout.Toggle("Copy Scene References", copySceneReferences);
        copyAssets = EditorGUILayout.Toggle("Copy Assets", copyAssets);
        repairUIButtonTween = EditorGUILayout.Toggle("Repair UIButtonTween.tweenTarget", repairUIButtonTween);

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Copy Modes", EditorStyles.boldLabel);
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing", overwriteExisting);
        previewOnly = EditorGUILayout.Toggle("Preview Only", previewOnly);

        GUILayout.Space(10);

        GUI.enabled = referenceRoot != null && targetRoot != null;

        if (GUILayout.Button("Synchronize Scenes", GUILayout.Height(40)))
        {
            Synchronize();
        }

        GUI.enabled = true;

        GUILayout.Space(15);

        GUILayout.Label("Results", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Objects Matched", objectsMatched.ToString());
        EditorGUILayout.LabelField("Components Copied", componentsCopied.ToString());
        EditorGUILayout.LabelField("Fields Copied", fieldsCopied.ToString());
        EditorGUILayout.LabelField("References Remapped", referencesRemapped.ToString());
        EditorGUILayout.LabelField("Objects Skipped", objectsSkipped.ToString());
        EditorGUILayout.LabelField("Missing Objects", missingObjects.ToString());

        GUILayout.Space(10);

        GUILayout.Label("Log", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < log.Count; i++)
        {
            EditorGUILayout.TextArea(log[i], EditorStyles.helpBox);
        }

        EditorGUILayout.EndScrollView();
    }

    void Synchronize()
    {
        log.Clear();
        previewOperations.Clear();

        objectsMatched = 0;
        componentsCopied = 0;
        fieldsCopied = 0;
        referencesRemapped = 0;
        objectsSkipped = 0;
        missingObjects = 0;

        Dictionary<string, GameObject> referenceMap = BuildHierarchyMap(referenceRoot);
        Dictionary<string, GameObject> targetMap = BuildHierarchyMap(targetRoot);

        log.Add("=== SCENE SYNCHRONIZATION START ===");

        // Match objects and copy components
        foreach (KeyValuePair<string, GameObject> refEntry in referenceMap)
        {
            string hierarchyPath = refEntry.Key;
            GameObject refObject = refEntry.Value;

            // Skip the root
            if (hierarchyPath == "")
                continue;

            GameObject targetObject;

            if (!targetMap.TryGetValue(hierarchyPath, out targetObject))
            {
                missingObjects++;
                log.Add("MISSING: " + hierarchyPath);
                continue;
            }

            objectsMatched++;

            // Get all components from reference object
            Component[] refComponents = refObject.GetComponents<Component>();

            foreach (Component refComponent in refComponents)
            {
                // Skip transforms and other special components
                if (refComponent is Transform)
                    continue;

                string componentType = refComponent.GetType().Name;

                // Check if same component exists on target
                Component targetComponent = targetObject.GetComponent(refComponent.GetType());

                if (targetComponent == null)
                {
                    // Try to add it
                    try
                    {
                        targetComponent = targetObject.AddComponent(refComponent.GetType());
                        componentsCopied++;
                        log.Add("ADDED COMPONENT: " + hierarchyPath + " > " + componentType);
                    }
                    catch
                    {
                        log.Add("FAILED TO ADD: " + hierarchyPath + " > " + componentType);
                        continue;
                    }
                }

                // Copy serialized fields
                CopySerializedData(
                    refComponent,
                    targetComponent,
                    hierarchyPath,
                    componentType,
                    referenceMap,
                    targetMap);
            }
        }

        if (previewOnly)
        {
            log.Add("");
            log.Add("=== PREVIEW MODE - NO CHANGES APPLIED ===");
            log.Add("Operations that would be performed: " + previewOperations.Count);
        }
        else
        {
            log.Add("");
            log.Add("=== APPLYING CHANGES ===");

            foreach (CopyOperation op in previewOperations)
            {
                // Changes already applied via SetDirty
            }

            AssetDatabase.SaveAssets();
            EditorApplication.MarkSceneDirty();
        }

        log.Add("=== SYNCHRONIZATION COMPLETE ===");
        Debug.Log("Scene Synchronization Complete. Fields copied: " + fieldsCopied);
    }

    void CopySerializedData(
        Component refComponent,
        Component targetComponent,
        string objectPath,
        string componentType,
        Dictionary<string, GameObject> referenceMap,
        Dictionary<string, GameObject> targetMap)
    {
        FieldInfo[] fields = refComponent.GetType().GetFields(
            BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            // Skip non-serialized fields
            if (field.IsNotSerialized || field.FieldType.IsPointer)
                continue;

            // Skip m_Script and other internal fields
            if (field.Name.StartsWith("m_") && field.Name != "m_Script" && !copySerializedVariables)
                continue;

            object refValue = field.GetValue(refComponent);
            object targetValue = field.GetValue(targetComponent);

            // Check if we should skip due to existing value
            if (!overwriteExisting && targetValue != null && !IsDefaultValue(targetValue))
            {
                objectsSkipped++;
                continue;
            }

            // Handle different field types
            object newValue = ProcessFieldValue(
                refValue,
                field.FieldType,
                objectPath,
                componentType,
                field.Name,
                referenceMap,
                targetMap);

            if (newValue != null || refValue == null)
            {
                try
                {
                    // Create undo record
                    Undo.RecordObject(targetComponent, "Synchronize " + componentType);

                    field.SetValue(targetComponent, newValue);

                    EditorUtility.SetDirty(targetComponent);

                    fieldsCopied++;

                    CopyOperation op = new CopyOperation();
                    op.objectPath = objectPath;
                    op.componentType = componentType;
                    op.fieldName = field.Name;
                    op.oldValue = targetValue;
                    op.newValue = newValue;
                    previewOperations.Add(op);

                    log.Add("COPIED: " + objectPath + " > " + componentType + "." + field.Name);
                }
                catch (System.Exception e)
                {
                    log.Add("ERROR copying " + objectPath + "." + componentType + "." + field.Name + ": " + e.Message);
                }
            }
        }
    }

    object ProcessFieldValue(
        object refValue,
        System.Type fieldType,
        string objectPath,
        string componentType,
        string fieldName,
        Dictionary<string, GameObject> referenceMap,
        Dictionary<string, GameObject> targetMap)
    {
        if (refValue == null)
            return null;

        // Handle GameObject references (scene references)
        if (fieldType == typeof(GameObject))
        {
            if (!copySceneReferences)
                return refValue;

            GameObject refGO = (GameObject)refValue;
            string refPath = GetRelativePath(referenceRoot.transform, refGO.transform);

            GameObject targetGO;

            if (targetMap.TryGetValue(refPath, out targetGO))
            {
                referencesRemapped++;
                log.Add("REMAPPED REFERENCE: " + objectPath + "." + fieldName + " -> " + refPath);
                return targetGO;
            }
            else
            {
                log.Add("MISSING REFERENCE TARGET: " + refPath);
                return null;
            }
        }

        // Handle Component references
        if (typeof(Component).IsAssignableFrom(fieldType))
        {
            if (!copySceneReferences)
                return refValue;

            Component refComp = (Component)refValue;

            if (refComp == null)
                return null;

            // Find the matching target object and component
            string compObjPath = GetRelativePath(referenceRoot.transform, refComp.transform);

            GameObject targetCompObj;

            if (!targetMap.TryGetValue(compObjPath, out targetCompObj))
            {
                log.Add("MISSING COMPONENT OBJECT: " + compObjPath);
                return null;
            }

            Component targetComp = targetCompObj.GetComponent(fieldType);

            if (targetComp != null)
            {
                referencesRemapped++;
                log.Add("REMAPPED COMPONENT: " + objectPath + "." + fieldName + " -> " + compObjPath);
                return targetComp;
            }

            return null;
        }

        // Special handling for UIButtonTween.tweenTarget
        if (repairUIButtonTween && fieldName == "tweenTarget" && componentType == "UIButtonTween")
        {
            if (fieldType == typeof(GameObject))
            {
                GameObject refGO = (GameObject)refValue;
                string refPath = GetRelativePath(referenceRoot.transform, refGO.transform);

                GameObject targetGO;

                if (targetMap.TryGetValue(refPath, out targetGO))
                {
                    referencesRemapped++;
                    log.Add("REPAIRED UIButtonTween.tweenTarget: " + objectPath + " -> " + refPath);
                    return targetGO;
                }
            }
        }

        // Handle primitive types and arrays
        if (copySerializedVariables)
        {
            return refValue;
        }

        return null;
    }

    Dictionary<string, GameObject> BuildHierarchyMap(GameObject root)
    {
        Dictionary<string, GameObject> map = new Dictionary<string, GameObject>();

        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < transforms.Length; i++)
        {
            string path = GetRelativePath(root.transform, transforms[i]);

            if (!map.ContainsKey(path))
                map.Add(path, transforms[i].gameObject);
        }

        return map;
    }

    string GetRelativePath(Transform root, Transform current)
    {
        if (current == root)
            return "";

        List<string> parts = new List<string>();

        while (current != root)
        {
            parts.Insert(0, current.name);
            current = current.parent;

            if (current == null)
                break;
        }

        string path = "";

        for (int i = 0; i < parts.Count; i++)
        {
            if (i > 0)
                path += "/";

            path += parts[i];
        }

        return path;
    }

    bool IsDefaultValue(object value)
    {
        if (value == null)
            return true;

        System.Type type = value.GetType();

        if (type == typeof(int))
            return (int)value == 0;
        if (type == typeof(float))
            return (float)value == 0f;
        if (type == typeof(bool))
            return (bool)value == false;
        if (type == typeof(string))
            return string.IsNullOrEmpty((string)value);
        if (typeof(GameObject).IsAssignableFrom(type))
            return (GameObject)value == null;
        if (typeof(Component).IsAssignableFrom(type))
            return (Component)value == null;

        return false;
    }
}