using Unity.Tutorials.Core.Editor;
using UnityEditor;
using UnityEngine; // It's good practice to include this


public class Asset_Replacement_Tool : EditorWindow
{
    // Variables to hold the objects the user will select
    private GameObject replacementPrefab;
    private Material newMaterial;

    public bool changeName = false;

    [MenuItem("Tools/Asset Replacement Tool")]
    public static void ShowWindow()
    {
        // Get a reference to our window, creating it if it doesn't exist.
        var window = GetWindow<Asset_Replacement_Tool>("Asset Replacer");

        // Explicitly show it as a floating utility window. This is a more robust
        // way to ensure it stays on top of the main Unity editor.
        window.ShowUtility();
    }
    
    private void OnGUI()
    {
        
        // --- Prefab Replacement Section ---
        GUILayout.Label("Replace Selected Scene Objects", EditorStyles.boldLabel);

        // Assign the return value of ObjectField to our variables to store the selection
        // We convert the integer from .Length to a string so the Label can display it.
        GUILayout.Label($"Selected Objects: {Selection.gameObjects.Length}");
        // The Toggle function returns the new state, so we must assign it back to our variable.
        changeName = GUILayout.Toggle(changeName, "Change Replacement Name to Prefab Name");
        replacementPrefab = (GameObject)EditorGUILayout.ObjectField("Replacement Prefab:", replacementPrefab, typeof(GameObject), false);

        if (GUILayout.Button("Replace Asset"))
        {
            // This is a local variable, so it doesn't need an access modifier like 'private'.
            GameObject[] selectedObjects = Selection.gameObjects;
            if (replacementPrefab == null)
            {
                Debug.LogWarning("Please select a replacement prefab.");
                return;
            }
            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("No objects selected for replacement.");
                return;
            }

            // Set a single Undo group name for the entire operation.
            Undo.SetCurrentGroupName("Replace Selected Objects");
            int group = Undo.GetCurrentGroup();

            foreach (GameObject obj in selectedObjects)
            {
                // Instantiate the replacement prefab at the original object's position and rotation
                GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(replacementPrefab);
                
                // Register the creation of the new object for undo *first*.
                Undo.RegisterCreatedObjectUndo(newObject, "Create replacement object");

                // Copy transform properties and then destroy the original object.
                newObject.transform.SetParent(obj.transform.parent, false);
                newObject.transform.SetPositionAndRotation(obj.transform.position, obj.transform.rotation);

                // Use an if/else block to choose which name to apply.
                if (changeName)
                {
                    // If the toggle is checked, use the prefab's name.
                    newObject.name = replacementPrefab.name;
                }
                else {
                    // Otherwise, preserve the original object's name.
                    newObject.name = obj.name;
                }

                // Destroy the original object
                Undo.DestroyObjectImmediate(obj);
            }
            // Collapse all operations in the group into a single Undo step.
            Undo.CollapseUndoOperations(group);
            Debug.Log($"Replaced {selectedObjects.Length} objects with '{replacementPrefab.name}'.");
        }

        // --- Material Replacement Section ---
        EditorGUILayout.Space();
        GUILayout.Label("Change Material on Selected Objects", EditorStyles.boldLabel);

        newMaterial = (Material)EditorGUILayout.ObjectField("New Material:", newMaterial, typeof(Material), false);

        if (GUILayout.Button("Change Material"))
        {
            if (newMaterial == null)
            {
                Debug.LogError("New Material not set. Please assign a material.");
                return;
            }

            // --- The material changing logic will go here! ---
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("No objects selected for material change.");
                return;
            }

            Undo.SetCurrentGroupName("Change Materials on Selected Objects");
            int group = Undo.GetCurrentGroup();

            foreach (GameObject obj in selectedObjects)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Register the material change for undo
                    Undo.RecordObject(renderer, "Change Material");

                    // Create a new materials array and fill it with the new material.
                    // This is necessary to support objects with multiple material slots.
                    // Using .sharedMaterial only changes the first material.
                    var newMaterials = new Material[renderer.sharedMaterials.Length];
                    for (int i = 0; i < newMaterials.Length; i++)
                    {
                        newMaterials[i] = newMaterial;
                    }

                    // Assign the new materials array back to the renderer.
                    renderer.sharedMaterials = newMaterials;
                }
            }
            Undo.CollapseUndoOperations(group);
            Debug.Log($"Changed material on {selectedObjects.Length} objects to '{newMaterial.name}'.");
        }
    }
}
