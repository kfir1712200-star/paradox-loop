using UnityEngine;
using UnityEditor;

public class ProjectSetup
{
    [MenuItem("Tools/Setup Project Tags and Layers")]
    public static void Execute()
    {
        // Setup Tags
        string[] tags = new string[]
        {
            "Player",
            "NPC",
            "Interactable",
            "Evidence",
            "Door",
            "Clue",
            "Zone"
        };

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        foreach (string tag in tags)
        {
            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
                Debug.Log($"Added tag: {tag}");
            }
        }

        // Setup Layers (use layers 6-15 which are user-definable)
        SerializedProperty layersProp = tagManager.FindProperty("layers");
        
        // Layer 6 = Player
        SetLayer(layersProp, 6, "Player");
        // Layer 7 = NPC
        SetLayer(layersProp, 7, "NPC");
        // Layer 8 = Interactable
        SetLayer(layersProp, 8, "Interactable");
        // Layer 9 = Ground
        SetLayer(layersProp, 9, "Ground");
        // Layer 10 = Ladder
        SetLayer(layersProp, 10, "Ladder");
        // Layer 11 = Zone
        SetLayer(layersProp, 11, "Zone");

        tagManager.ApplyModifiedProperties();
        Debug.Log("Project tags and layers setup complete!");
    }

    static void SetLayer(SerializedProperty layersProp, int index, string name)
    {
        SerializedProperty sp = layersProp.GetArrayElementAtIndex(index);
        if (string.IsNullOrEmpty(sp.stringValue))
        {
            sp.stringValue = name;
            Debug.Log($"Set layer {index} = {name}");
        }
        else if (sp.stringValue != name)
        {
            Debug.LogWarning($"Layer {index} already set to '{sp.stringValue}', skipping '{name}'");
        }
    }
}
