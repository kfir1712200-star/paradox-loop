using UnityEngine;
using UnityEditor;
using UnityEditor.AI;
using System.Reflection;

public static class BakeNavMesh
{
    [MenuItem("Mimi/Bake NavMesh")]
    public static void Run() => Execute();

    public static void Execute()
    {
        // Mark Terrain as Navigation Static
        var terrain = GameObject.Find("Terrain");
        if (terrain != null)
        {
            GameObjectUtility.SetStaticEditorFlags(terrain,
                StaticEditorFlags.NavigationStatic | StaticEditorFlags.ContributeGI);
            Debug.Log("[BakeNavMesh] Terrain marked as NavigationStatic");
        }
        else
        {
            // Try finding any Terrain object
            var terrains = Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None);
            foreach (var t in terrains)
            {
                GameObjectUtility.SetStaticEditorFlags(t.gameObject,
                    StaticEditorFlags.NavigationStatic | StaticEditorFlags.ContributeGI);
                Debug.Log($"[BakeNavMesh] {t.gameObject.name} marked as NavigationStatic");
            }
        }

        // Bake NavMesh via NavMeshBuilder
        NavMeshBuilder.BuildNavMesh();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[BakeNavMesh] NavMesh baked successfully!");
    }
}
