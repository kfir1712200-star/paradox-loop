using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the time loop mechanic for Project Paradox Loop.
/// Tracks loop count, persistent knowledge, and handles loop resets.
/// </summary>
public class LoopManager : Singleton<LoopManager>
{
    [Header("Loop Settings")]
    [SerializeField] private float loopTransitionDuration = 3f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    // Loop state
    private int currentLoop = 1;
    private bool isTransitioning = false;

    // Persistent knowledge — survives between loops
    private HashSet<string> knowledgeKeys = new HashSet<string>();
    
    // Persistent items — items that carry over between loops
    private HashSet<string> persistentItems = new HashSet<string>();

    // Loop history — what happened in each loop
    private List<LoopRecord> loopHistory = new List<LoopRecord>();

    // Properties
    public int CurrentLoop => currentLoop;
    public bool IsTransitioning => isTransitioning;
    public int KnowledgeCount => knowledgeKeys.Count;

    protected override void OnSingletonAwake()
    {
        // Listen for week end to trigger loop
        GameEvents.OnWeekEnd += OnWeekEnd;
    }

    protected override void OnDestroy()
    {
        GameEvents.OnWeekEnd -= OnWeekEnd;
        base.OnDestroy();
    }

    private void OnWeekEnd()
    {
        if (!isTransitioning)
        {
            StartCoroutine(LoopTransitionRoutine());
        }
    }

    private IEnumerator LoopTransitionRoutine()
    {
        isTransitioning = true;

        // Save what happened this loop
        RecordLoop();

        // Fire pre-reset event
        GameEvents.InvokeLoopReset();

        // Wait for transition (fade to black, glitch effect, etc.)
        // UI systems will listen to OnLoopReset and show transition screen
        yield return new WaitForSecondsRealtime(loopTransitionDuration);

        // Increment loop counter
        currentLoop++;

        // Reset time to Sunday morning
        TimeManager.Instance.ResetToWeekStart();

        // Fire new loop event
        GameEvents.InvokeLoopStart(currentLoop);

        isTransitioning = false;
    }

    // ═══════════════════════════════════════════
    // KNOWLEDGE SYSTEM — persists between loops
    // ═══════════════════════════════════════════

    /// <summary>Add a piece of knowledge that persists between loops</summary>
    public void AddKnowledge(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        
        if (knowledgeKeys.Add(key))
        {
            Debug.Log($"[LoopManager] New knowledge gained: {key}");
            GameEvents.InvokeKnowledgeGained(key);
        }
    }

    /// <summary>Check if the player has specific knowledge</summary>
    public bool HasKnowledge(string key)
    {
        return knowledgeKeys.Contains(key);
    }

    /// <summary>Get all knowledge keys</summary>
    public IReadOnlyCollection<string> GetAllKnowledge()
    {
        return knowledgeKeys;
    }

    // ═══════════════════════════════════════════
    // PERSISTENT ITEMS — carry over between loops
    // ═══════════════════════════════════════════

    /// <summary>Mark an item as persistent (survives loop reset)</summary>
    public void AddPersistentItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;
        
        if (persistentItems.Add(itemId))
        {
            Debug.Log($"[LoopManager] Persistent item collected: {itemId}");
            GameEvents.InvokePersistentItemCollected(itemId);
        }
    }

    /// <summary>Check if player has a persistent item</summary>
    public bool HasPersistentItem(string itemId)
    {
        return persistentItems.Contains(itemId);
    }

    // ═══════════════════════════════════════════
    // LOOP HISTORY
    // ═══════════════════════════════════════════

    private void RecordLoop()
    {
        var record = new LoopRecord
        {
            loopNumber = currentLoop,
            knowledgeGainedThisLoop = new List<string>(knowledgeKeys),
            // Add more data as needed (NPC states, choices made, etc.)
        };
        loopHistory.Add(record);
    }

    /// <summary>Force trigger a loop reset (for debug or special events)</summary>
    public void ForceLoopReset()
    {
        if (!isTransitioning)
        {
            TimeManager.Instance.StopTime();
            StartCoroutine(LoopTransitionRoutine());
        }
    }

    // ═══════════════════════════════════════════
    // SAVE/LOAD
    // ═══════════════════════════════════════════

    [Serializable]
    public class LoopData
    {
        public int currentLoop;
        public List<string> knowledgeKeys;
        public List<string> persistentItems;
    }

    public LoopData GetSaveData()
    {
        return new LoopData
        {
            currentLoop = currentLoop,
            knowledgeKeys = new List<string>(knowledgeKeys),
            persistentItems = new List<string>(persistentItems)
        };
    }

    public void LoadSaveData(LoopData data)
    {
        currentLoop = data.currentLoop;
        knowledgeKeys = new HashSet<string>(data.knowledgeKeys);
        persistentItems = new HashSet<string>(data.persistentItems);
    }

    // ═══════════════════════════════════════════
    // DATA CLASSES
    // ═══════════════════════════════════════════

    [Serializable]
    public class LoopRecord
    {
        public int loopNumber;
        public List<string> knowledgeGainedThisLoop;
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = Color.cyan;

        string info = $"Loop #{currentLoop} | Knowledge: {knowledgeKeys.Count} | Items: {persistentItems.Count}";
        GUI.Label(new Rect(10, 35, 500, 25), info, style);
    }
#endif
}
