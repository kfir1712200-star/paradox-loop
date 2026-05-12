using UnityEngine;
using System;
using System.IO;

/// <summary>
/// Main Game Manager for Project Paradox Loop.
/// Handles game state, save/load, and pause functionality.
/// </summary>
public class GameManager : Singleton<GameManager>
{
    [Header("Game State")]
    [SerializeField] private bool startTimeOnAwake = true;

    private bool isGamePaused = false;
    
    public bool IsGamePaused => isGamePaused;

    protected override void OnSingletonAwake()
    {
        Application.targetFrameRate = 60;
        
        // Subscribe to events
        GameEvents.OnWeekEnd += OnWeekEnd;
    }

    void Start()
    {
        if (startTimeOnAwake)
        {
            // Ensure TimeManager and LoopManager are initialized
            var _ = TimeManager.Instance;
            var __ = LoopManager.Instance;
            
            GameEvents.InvokeLoopStart(1);
        }
    }

    protected override void OnDestroy()
    {
        GameEvents.OnWeekEnd -= OnWeekEnd;
        base.OnDestroy();
    }

    void Update()
    {
        // Pause toggle
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // Debug controls (Editor only)
#if UNITY_EDITOR
        HandleDebugInput();
#endif
    }

    // ═══════════════════════════════════════════
    // PAUSE SYSTEM
    // ═══════════════════════════════════════════

    public void TogglePause()
    {
        SetPaused(!isGamePaused);
    }

    public void SetPaused(bool paused)
    {
        isGamePaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        
        // Also pause game time
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetTimePaused(paused);
        }

        // Show/hide cursor
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;

        GameEvents.InvokeGamePaused(paused);
    }

    // ═══════════════════════════════════════════
    // SAVE / LOAD (JSON)
    // ═══════════════════════════════════════════

    [Serializable]
    private class SaveData
    {
        public TimeManager.TimeData timeData;
        public LoopManager.LoopData loopData;
        public string saveDate;
    }

    private string GetSavePath(int slot = 0)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");
    }

    public void SaveGame(int slot = 0)
    {
        var saveData = new SaveData
        {
            timeData = TimeManager.Instance.GetSaveData(),
            loopData = LoopManager.Instance.GetSaveData(),
            saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        string json = JsonUtility.ToJson(saveData, true);
        string path = GetSavePath(slot);
        
        File.WriteAllText(path, json);
        Debug.Log($"[GameManager] Game saved to slot {slot}: {path}");
        
        GameEvents.InvokeGameSaved();
    }

    public bool LoadGame(int slot = 0)
    {
        string path = GetSavePath(slot);
        
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[GameManager] No save file found at slot {slot}");
            return false;
        }

        string json = File.ReadAllText(path);
        var saveData = JsonUtility.FromJson<SaveData>(json);

        TimeManager.Instance.LoadSaveData(saveData.timeData);
        LoopManager.Instance.LoadSaveData(saveData.loopData);

        Debug.Log($"[GameManager] Game loaded from slot {slot} (saved: {saveData.saveDate})");
        
        GameEvents.InvokeGameLoaded();
        return true;
    }

    public bool HasSaveFile(int slot = 0)
    {
        return File.Exists(GetSavePath(slot));
    }

    // ═══════════════════════════════════════════
    // EVENT HANDLERS
    // ═══════════════════════════════════════════

    private void OnWeekEnd()
    {
        Debug.Log("[GameManager] Week ended — Loop will reset!");
    }

    // ═══════════════════════════════════════════
    // DEBUG CONTROLS
    // ═══════════════════════════════════════════

#if UNITY_EDITOR
    private void HandleDebugInput()
    {
        // F1 = Skip to next hour
        if (Input.GetKeyDown(KeyCode.F1))
        {
            var tm = TimeManager.Instance;
            tm.SkipTo(tm.CurrentDay, tm.CurrentHour + 1);
            Debug.Log($"[Debug] Skipped to {tm.GetTimeString()}");
        }

        // F2 = Skip to next day
        if (Input.GetKeyDown(KeyCode.F2))
        {
            TimeManager.Instance.SkipToNextDay();
            Debug.Log($"[Debug] Skipped to next day");
        }

        // F3 = Force loop reset
        if (Input.GetKeyDown(KeyCode.F3))
        {
            LoopManager.Instance.ForceLoopReset();
            Debug.Log("[Debug] Forced loop reset");
        }

        // F4 = Toggle time speed (1x -> 2x -> 4x -> 1x)
        if (Input.GetKeyDown(KeyCode.F4))
        {
            var tm = TimeManager.Instance;
            float newSpeed = tm.TimeMultiplier switch
            {
                1f => 2f,
                2f => 4f,
                _ => 1f
            };
            tm.SetTimeMultiplier(newSpeed);
            Debug.Log($"[Debug] Time speed: x{newSpeed}");
        }

        // F5 = Quick Save
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveGame(0);
        }

        // F9 = Quick Load
        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadGame(0);
        }

        // F6 = Add test knowledge
        if (Input.GetKeyDown(KeyCode.F6))
        {
            LoopManager.Instance.AddKnowledge($"test_knowledge_{Time.time:F0}");
        }
    }
#endif
}
