using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CreateNoaSchedule
{
    [MenuItem("Tools/Create Noa Schedule")]
    public static void Execute()
    {
        var schedule = ScriptableObject.CreateInstance<ScheduleData>();
        schedule.npcName = "Noa";
        schedule.npcDescription = "A quiet student hiding a painful secret. The player must discover what's troubling her and prevent the tragedy.";

        schedule.entries = new List<ScheduleEntry>
        {
            // ═══ SUNDAY (Day 0) ═══
            new ScheduleEntry { day = 0, startHour = 7, endHour = 8, action = NPCAction.Walk, waypointName = "SchoolEntrance", dialogueKey = "noa_sun_morning" },
            new ScheduleEntry { day = 0, startHour = 8, endHour = 10, action = NPCAction.Sit, waypointName = "Classroom_A", dialogueKey = "noa_sun_class1" },
            new ScheduleEntry { day = 0, startHour = 10, endHour = 11, action = NPCAction.Eat, waypointName = "Cafeteria_Corner", dialogueKey = "noa_sun_break" },
            new ScheduleEntry { day = 0, startHour = 11, endHour = 13, action = NPCAction.Sit, waypointName = "Classroom_A", dialogueKey = "noa_sun_class2" },
            new ScheduleEntry { day = 0, startHour = 13, endHour = 14, action = NPCAction.Walk, waypointName = "SchoolEntrance", dialogueKey = "noa_sun_leaving" },

            // ═══ MONDAY (Day 1) ═══
            new ScheduleEntry { day = 1, startHour = 7, endHour = 8, action = NPCAction.Walk, waypointName = "SchoolEntrance", dialogueKey = "noa_mon_morning" },
            new ScheduleEntry { day = 1, startHour = 8, endHour = 10, action = NPCAction.Sit, waypointName = "Classroom_B", dialogueKey = "noa_mon_class1" },
            new ScheduleEntry { day = 1, startHour = 10, endHour = 11, action = NPCAction.WaitAtLocation, waypointName = "Library_Desk", dialogueKey = "noa_mon_library" },
            new ScheduleEntry { day = 1, startHour = 11, endHour = 13, action = NPCAction.Sit, waypointName = "Classroom_B", dialogueKey = "noa_mon_class2" },
            new ScheduleEntry { day = 1, startHour = 13, endHour = 14, action = NPCAction.Walk, waypointName = "Bathroom_Hallway", dialogueKey = "noa_mon_alone" },

            // ═══ TUESDAY (Day 2) ═══
            new ScheduleEntry { day = 2, startHour = 7, endHour = 8, action = NPCAction.Walk, waypointName = "SchoolEntrance", dialogueKey = "noa_tue_morning" },
            new ScheduleEntry { day = 2, startHour = 8, endHour = 10, action = NPCAction.Sit, waypointName = "Classroom_A", dialogueKey = "noa_tue_class1" },
            new ScheduleEntry { day = 2, startHour = 10, endHour = 11, action = NPCAction.SpecialEvent, waypointName = "Hallway_Lockers", dialogueKey = "noa_tue_bullying_event" },
            new ScheduleEntry { day = 2, startHour = 11, endHour = 13, action = NPCAction.Sit, waypointName = "Classroom_A", dialogueKey = "noa_tue_class2" },
            new ScheduleEntry { day = 2, startHour = 13, endHour = 14, action = NPCAction.WaitAtLocation, waypointName = "Rooftop", dialogueKey = "noa_tue_rooftop" },

            // ═══ WEDNESDAY (Day 3) ═══
            new ScheduleEntry { day = 3, startHour = 7, endHour = 8, action = NPCAction.Walk, waypointName = "SchoolEntrance", dialogueKey = "noa_wed_morning" },
            new ScheduleEntry { day = 3, startHour = 8, endHour = 10, action = NPCAction.Sit, waypointName = "Classroom_B", dialogueKey = "noa_wed_class1" },
            new ScheduleEntry { day = 3, startHour = 10, endHour = 11, action = NPCAction.Eat, waypointName = "Cafeteria_Corner", dialogueKey = "noa_wed_break" },
            new ScheduleEntry { day = 3, startHour = 11, endHour = 13, action = NPCAction.Sit, waypointName = "Classroom_B", dialogueKey = "noa_wed_class2" },
            new ScheduleEntry { day = 3, startHour = 13, endHour = 14, action = NPCAction.Walk, waypointName = "SchoolEntrance", dialogueKey = "noa_wed_leaving" },

            // ═══ THURSDAY (Day 4) ═══
            new ScheduleEntry { day = 4, startHour = 7, endHour = 8, action = NPCAction.Walk, waypointName = "SchoolEntrance", dialogueKey = "noa_thu_morning" },
            new ScheduleEntry { day = 4, startHour = 8, endHour = 10, action = NPCAction.Sit, waypointName = "Classroom_A", dialogueKey = "noa_thu_class1" },
            new ScheduleEntry { day = 4, startHour = 10, endHour = 11, action = NPCAction.WaitAtLocation, waypointName = "Counselor_Office", dialogueKey = "noa_thu_counselor" },
            new ScheduleEntry { day = 4, startHour = 11, endHour = 13, action = NPCAction.Sit, waypointName = "Classroom_A", dialogueKey = "noa_thu_class2" },
            new ScheduleEntry { day = 4, startHour = 13, endHour = 14, action = NPCAction.WaitAtLocation, waypointName = "Library_Desk", dialogueKey = "noa_thu_library" },

            // ═══ FRIDAY (Day 5) ═══
            new ScheduleEntry { day = 5, startHour = 7, endHour = 8, action = NPCAction.Walk, waypointName = "SchoolEntrance", dialogueKey = "noa_fri_morning" },
            new ScheduleEntry { day = 5, startHour = 8, endHour = 10, action = NPCAction.Sit, waypointName = "Classroom_A", dialogueKey = "noa_fri_class1" },
            new ScheduleEntry { day = 5, startHour = 10, endHour = 11, action = NPCAction.WaitAtLocation, waypointName = "Bathroom_Hallway", dialogueKey = "noa_fri_break" },
            new ScheduleEntry { day = 5, startHour = 11, endHour = 13, action = NPCAction.Sit, waypointName = "Classroom_A", dialogueKey = "noa_fri_class2" },
            new ScheduleEntry { day = 5, startHour = 13, endHour = 14, action = NPCAction.SpecialEvent, waypointName = "Rooftop", dialogueKey = "noa_fri_finale" },
        };

        string path = "Assets/ScriptableObjects/Schedules/NoaSchedule.asset";
        // Ensure directory exists
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(
            System.IO.Path.Combine(Application.dataPath, "..", path)));

        AssetDatabase.CreateAsset(schedule, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Created Noa's schedule at {path} with {schedule.entries.Count} entries");
        EditorGUIUtility.PingObject(schedule);
    }
}
