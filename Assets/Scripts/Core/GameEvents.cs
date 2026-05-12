using System;
using UnityEngine;

/// <summary>
/// Central event system for Project Paradox Loop.
/// All game-wide events are defined here as static C# events.
/// </summary>
public static class GameEvents
{
    // ═══════════════════════════════════════════
    // TIME EVENTS
    // ═══════════════════════════════════════════
    
    /// <summary>Fired every in-game minute. Args: hour, minute</summary>
    public static event Action<int, int> OnTimeChanged;
    
    /// <summary>Fired when the hour changes. Args: newHour</summary>
    public static event Action<int> OnHourChanged;
    
    /// <summary>Fired when the day changes. Args: dayIndex (0=Sunday, 5=Friday)</summary>
    public static event Action<int> OnDayChanged;
    
    /// <summary>Fired when the week ends (Friday 14:00)</summary>
    public static event Action OnWeekEnd;
    
    /// <summary>Fired when time is paused or resumed. Args: isPaused</summary>
    public static event Action<bool> OnTimePaused;
    
    /// <summary>Fired when time speed changes. Args: multiplier</summary>
    public static event Action<float> OnTimeSpeedChanged;

    // ═══════════════════════════════════════════
    // LOOP EVENTS
    // ═══════════════════════════════════════════
    
    /// <summary>Fired when a new loop begins. Args: loopNumber</summary>
    public static event Action<int> OnLoopStart;
    
    /// <summary>Fired just before the loop resets</summary>
    public static event Action OnLoopReset;
    
    /// <summary>Fired when new knowledge is discovered. Args: knowledgeKey</summary>
    public static event Action<string> OnKnowledgeGained;
    
    /// <summary>Fired when a persistent item is collected. Args: itemId</summary>
    public static event Action<string> OnPersistentItemCollected;

    // ═══════════════════════════════════════════
    // PLAYER EVENTS
    // ═══════════════════════════════════════════
    
    /// <summary>Fired when player interacts with something. Args: interactable</summary>
    public static event Action<IInteractable> OnPlayerInteract;
    
    /// <summary>Fired when player enters/exits a zone. Args: zoneName, entered</summary>
    public static event Action<string, bool> OnPlayerZoneChange;

    // ═══════════════════════════════════════════
    // NPC EVENTS
    // ═══════════════════════════════════════════
    
    /// <summary>Fired when an NPC's stress level changes. Args: npcName, stressLevel</summary>
    public static event Action<string, float> OnNPCStressChanged;
    
    /// <summary>Fired when dialogue starts. Args: npcName</summary>
    public static event Action<string> OnDialogueStart;
    
    /// <summary>Fired when dialogue ends. Args: npcName</summary>
    public static event Action<string> OnDialogueEnd;

    // ═══════════════════════════════════════════
    // GAME STATE EVENTS
    // ═══════════════════════════════════════════
    
    /// <summary>Fired when game is saved</summary>
    public static event Action OnGameSaved;
    
    /// <summary>Fired when game is loaded</summary>
    public static event Action OnGameLoaded;
    
    /// <summary>Fired when game is paused/unpaused. Args: isPaused</summary>
    public static event Action<bool> OnGamePaused;

    // ═══════════════════════════════════════════
    // INVOKE METHODS (called by managers)
    // ═══════════════════════════════════════════
    
    public static void InvokeTimeChanged(int hour, int minute) => OnTimeChanged?.Invoke(hour, minute);
    public static void InvokeHourChanged(int hour) => OnHourChanged?.Invoke(hour);
    public static void InvokeDayChanged(int dayIndex) => OnDayChanged?.Invoke(dayIndex);
    public static void InvokeWeekEnd() => OnWeekEnd?.Invoke();
    public static void InvokeTimePaused(bool isPaused) => OnTimePaused?.Invoke(isPaused);
    public static void InvokeTimeSpeedChanged(float multiplier) => OnTimeSpeedChanged?.Invoke(multiplier);
    
    public static void InvokeLoopStart(int loopNumber) => OnLoopStart?.Invoke(loopNumber);
    public static void InvokeLoopReset() => OnLoopReset?.Invoke();
    public static void InvokeKnowledgeGained(string key) => OnKnowledgeGained?.Invoke(key);
    public static void InvokePersistentItemCollected(string itemId) => OnPersistentItemCollected?.Invoke(itemId);
    
    public static void InvokePlayerInteract(IInteractable interactable) => OnPlayerInteract?.Invoke(interactable);
    public static void InvokePlayerZoneChange(string zone, bool entered) => OnPlayerZoneChange?.Invoke(zone, entered);
    
    public static void InvokeNPCStressChanged(string npcName, float stress) => OnNPCStressChanged?.Invoke(npcName, stress);
    public static void InvokeDialogueStart(string npcName) => OnDialogueStart?.Invoke(npcName);
    public static void InvokeDialogueEnd(string npcName) => OnDialogueEnd?.Invoke(npcName);
    
    public static void InvokeGameSaved() => OnGameSaved?.Invoke();
    public static void InvokeGameLoaded() => OnGameLoaded?.Invoke();
    public static void InvokeGamePaused(bool isPaused) => OnGamePaused?.Invoke(isPaused);
}
