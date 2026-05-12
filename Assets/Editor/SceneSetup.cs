using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SceneSetup
{
    [MenuItem("Tools/Setup Paradox Loop Scene")]
    public static void Execute()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Cannot setup scene while in Play Mode! Please exit Play Mode first.");
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();

        // ═══════════════════════════════════════════
        // Create Manager GameObjects
        // ═══════════════════════════════════════════

        // GameManager
        var gmGO = CreateOrFind("[GameManager]");
        AddComponentIfMissing<GameManager>(gmGO);

        // TimeManager
        var tmGO = CreateOrFind("[TimeManager]");
        AddComponentIfMissing<TimeManager>(tmGO);

        // LoopManager
        var lmGO = CreateOrFind("[LoopManager]");
        AddComponentIfMissing<LoopManager>(lmGO);

        // ═══════════════════════════════════════════
        // DayNightCycle on Directional Light
        // ═══════════════════════════════════════════
        var dirLight = GameObject.Find("Directional Light");
        if (dirLight != null)
        {
            AddComponentIfMissing<DayNightCycle>(dirLight);
            Debug.Log("Added DayNightCycle to Directional Light");
        }

        // ═══════════════════════════════════════════
        // Player Setup — add InteractionSystem to npc_casual_set_00
        // ═══════════════════════════════════════════
        var playerGO = GameObject.Find("npc_casual_set_00");
        if (playerGO != null)
        {
            AddComponentIfMissing<InteractionSystem>(playerGO);
            
            // Ensure it has a CharacterController
            var cc = AddComponentIfMissing<CharacterController>(playerGO);
            if (cc != null)
            {
                cc.height = 1.8f;
                cc.center = new Vector3(0f, 0.9f, 0f);
                cc.radius = 0.3f;
            }

            // Ensure it has Movement
            AddComponentIfMissing<Movement>(playerGO);

            // Set tag
            playerGO.tag = "Player";
            playerGO.layer = LayerMask.NameToLayer("Player");
            
            Debug.Log("Player setup complete on npc_casual_set_00");
        }
        else
        {
            Debug.LogWarning("npc_casual_set_00 not found in scene!");
        }

        // ═══════════════════════════════════════════
        // Save Scene
        // ═══════════════════════════════════════════
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        Debug.Log("Scene setup complete and saved!");
    }

    static GameObject CreateOrFind(string name)
    {
        var go = GameObject.Find(name);
        if (go == null)
        {
            go = new GameObject(name);
            Debug.Log($"Created: {name}");
        }
        else
        {
            Debug.Log($"Found existing: {name}");
        }
        return go;
    }

    static T AddComponentIfMissing<T>(GameObject go) where T : Component
    {
        var comp = go.GetComponent<T>();
        if (comp == null)
        {
            comp = go.AddComponent<T>();
            Debug.Log($"Added {typeof(T).Name} to {go.name}");
        }
        return comp;
    }
}
