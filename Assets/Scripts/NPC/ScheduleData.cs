using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject defining an NPC's weekly schedule.
/// Each entry specifies what the NPC does at a given day/hour.
/// </summary>
[CreateAssetMenu(fileName = "NewSchedule", menuName = "Paradox Loop/NPC Schedule")]
public class ScheduleData : ScriptableObject
{
    [Header("NPC Info")]
    public string npcName;
    public string npcDescription;

    [Header("Schedule")]
    public List<ScheduleEntry> entries = new List<ScheduleEntry>();

    /// <summary>Get the schedule entry for a specific day and hour</summary>
    public ScheduleEntry GetEntry(int day, int hour)
    {
        // Find exact match first
        foreach (var entry in entries)
        {
            if (entry.day == day && entry.startHour <= hour && hour < entry.endHour)
            {
                return entry;
            }
        }

        // Fallback: find a default entry (day = -1 means "every day")
        foreach (var entry in entries)
        {
            if (entry.day == -1 && entry.startHour <= hour && hour < entry.endHour)
            {
                return entry;
            }
        }

        return null;
    }
}

[Serializable]
public class ScheduleEntry
{
    [Tooltip("-1 = every day, 0=Sunday, 1=Monday, ... 5=Friday")]
    public int day = -1;

    [Range(7, 14)]
    public int startHour = 7;

    [Range(8, 15)]
    public int endHour = 8;

    public NPCAction action = NPCAction.Idle;

    [Tooltip("Name of the waypoint Transform to go to")]
    public string waypointName;

    [Tooltip("Optional dialogue key for this time slot")]
    public string dialogueKey;

    [Tooltip("Optional: specific animation to play")]
    public string animationTrigger;
}

public enum NPCAction
{
    Idle,
    Walk,
    Sit,
    Talk,
    Eat,
    Study,
    Patrol,
    WaitAtLocation,
    SpecialEvent
}
