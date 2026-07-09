using UnityEngine;
using UnityEditor;

public class MakeMeshReadable
{
    [MenuItem("Tools/3DS/Mesh/Make Mesh Readable")]
    public static void MakeReadable()
    {
        // Get all currently selected objects in the editor
        Object[] selectedObjects = Selection.objects;

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogError("Please select one or more Mesh .asset files in the Project window.");
            return;
        }

        int successCount = 0;

        // Loop through everything selected
        foreach (Object obj in selectedObjects)
        {
            Mesh mesh = obj as Mesh;

            if (mesh != null)
            {
                SerializedObject serializedMesh = new SerializedObject(mesh);
                SerializedProperty isReadableProp = serializedMesh.FindProperty("m_IsReadable");

                if (isReadableProp != null)
                {
                    serializedMesh.Update();
                    isReadableProp.boolValue = true;
                    serializedMesh.ApplyModifiedProperties();

                    Debug.Log("Success: " + mesh.name + " is now readable.");
                    successCount++;
                }
            }
        }

        if (successCount > 0)
        {
            // Save all modified assets to disk at once
            AssetDatabase.SaveAssets();
            Debug.Log("Finished processing! " + successCount + " meshes made readable.");
        }
        else
        {
            Debug.LogWarning("No valid Mesh components found in the current selection.");
        }
    }
}