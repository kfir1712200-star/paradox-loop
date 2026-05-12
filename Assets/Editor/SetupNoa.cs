using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public static class SetupNoa
{
    [MenuItem("Mimi/Setup Noa NPC")]
    public static void Run() => Execute();

    public static void Execute()
    {
        // Remove existing Noa if present
        var existing = GameObject.Find("Noa");
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
            Debug.Log("[SetupNoa] Removed existing Noa");
        }

        // ── Create Noa GameObject ───────────────────────────
        var noaGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        noaGO.name = "Noa";

        // Position near center of scene, away from Mimi
        noaGO.transform.position = new Vector3(-8f, 0f, -8f);
        noaGO.transform.localScale = new Vector3(0.5f, 0.85f, 0.5f); // slim capsule

        // Color the capsule pink/purple so it's recognizable
        var renderer = noaGO.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.9f, 0.6f, 0.8f, 1f);
            mat.name = "Noa_Placeholder";
            renderer.sharedMaterial = mat;
        }

        // ── Add NavMeshAgent ────────────────────────────────
        var agent = noaGO.AddComponent<NavMeshAgent>();
        agent.speed = 2f;
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 0.5f;
        agent.radius = 0.25f;
        agent.height = 1.7f;
        agent.baseOffset = 0f;
        agent.autoTraverseOffMeshLink = true;

        // ── Add Animator (required by NPCScheduler) ─────────
        noaGO.AddComponent<Animator>();

        // ── Create ScheduleData ScriptableObject ─────────────
        var schedule = ScriptableObject.CreateInstance<ScheduleData>();
        schedule.npcName = "Noa";
        schedule.npcDescription = "The central character. Sensitive, creative, quietly struggling.";

        schedule.entries = new System.Collections.Generic.List<ScheduleEntry>
        {
            // Every day 07:00-09:00 — arrives, waits near entrance
            new ScheduleEntry { day = -1, startHour = 7, endHour = 9,  action = NPCAction.WaitAtLocation, waypointName = "" },
            // Every day 09:00-12:00 — studying / classroom
            new ScheduleEntry { day = -1, startHour = 9, endHour = 12, action = NPCAction.Study,           waypointName = "" },
            // Every day 12:00-13:00 — lunch / eating
            new ScheduleEntry { day = -1, startHour = 12, endHour = 13, action = NPCAction.Eat,            waypointName = "" },
            // Every day 13:00-14:00 — back to waiting / end of day
            new ScheduleEntry { day = -1, startHour = 13, endHour = 14, action = NPCAction.WaitAtLocation, waypointName = "" },
        };

        // Save to Assets/ScriptableObjects/
        string savePath = "Assets/ScriptableObjects/NoaSchedule.asset";
        AssetDatabase.CreateAsset(schedule, savePath);
        AssetDatabase.SaveAssets();
        Debug.Log($"[SetupNoa] ScheduleData saved to {savePath}");

        // ── Add NoaController ────────────────────────────────
        var noa = noaGO.AddComponent<NoaController>();
        var so = new SerializedObject(noa);
        so.FindProperty("npcName").stringValue = "Noa";
        so.FindProperty("schedule").objectReferenceValue = schedule;
        // Set walk/run speeds to match agent
        so.FindProperty("walkSpeed").floatValue = 2f;
        so.FindProperty("runSpeed").floatValue  = 4f;
        so.ApplyModifiedProperties();

        // ── Add name label (for greybox clarity) ─────────────
        // Use a child text mesh for identification in scene view
        var labelGO = new GameObject("_Label");
        labelGO.transform.SetParent(noaGO.transform, false);
        labelGO.transform.localPosition = new Vector3(0, 1.4f, 0);

        EditorUtility.SetDirty(noaGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[SetupNoa] Noa NPC created at (-8, 0, -8) with NoaController + ScheduleData");
        Selection.activeGameObject = noaGO;
    }
}
