using UnityEngine;

/// <summary>
/// Noa — the central character of Project Paradox Loop.
/// Inherits from NPCScheduler and adds the Stress System.
/// The player must prevent her tragedy by reducing her stress below the threshold.
/// </summary>
public class NoaController : NPCScheduler
{
    [Header("Stress System")]
    [SerializeField] private float maxStress = 100f;
    [SerializeField] private float initialStress = 30f;
    [SerializeField] private float stressThreshold = 80f; // Above this = tragedy on Friday
    [SerializeField] private float passiveStressPerHour = 2f; // Stress builds naturally

    [Header("Stress Modifiers")]
    [SerializeField] private float bullyingStressIncrease = 15f;
    [SerializeField] private float friendlyTalkStressDecrease = 10f;
    [SerializeField] private float helpActionStressDecrease = 8f;
    [SerializeField] private float ignoredStressIncrease = 5f;

    [Header("Visual Stress Cues")]
    [SerializeField] private float lowStressThreshold = 30f;
    [SerializeField] private float medStressThreshold = 50f;
    [SerializeField] private float highStressThreshold = 70f;

    // Stress state
    private float currentStress;
    private bool tragedyTriggered = false;

    // Animation hashes for stress
    private static readonly int StressLevelHash = Animator.StringToHash("StressLevel");
    private static readonly int IsDistressedHash = Animator.StringToHash("IsDistressed");

    // Properties
    public float CurrentStress => currentStress;
    public float StressNormalized => currentStress / maxStress;
    public bool IsHighStress => currentStress >= highStressThreshold;
    public bool TragedyTriggered => tragedyTriggered;

    public override string InteractionPrompt
    {
        get
        {
            if (isTalkingToPlayer) return "";
            if (currentStress > highStressThreshold)
                return $"Talk to {npcName} (she looks distressed)";
            return $"Talk to {npcName}";
        }
    }

    protected override void Awake()
    {
        base.Awake();
        npcName = "Noa";
        currentStress = initialStress;
    }

    protected override void Start()
    {
        base.Start();

        // Subscribe to week end for tragedy check
        GameEvents.OnWeekEnd += OnWeekEndCheck;
        GameEvents.OnHourChanged += OnHourStressUpdate;
    }

    protected override void OnDestroy()
    {
        GameEvents.OnWeekEnd -= OnWeekEndCheck;
        GameEvents.OnHourChanged -= OnHourStressUpdate;
        base.OnDestroy();
    }

    // ═══════════════════════════════════════════
    // STRESS SYSTEM
    // ═══════════════════════════════════════════

    /// <summary>Modify stress by a delta amount (positive = more stress, negative = less)</summary>
    public void ModifyStress(float delta, string reason = "")
    {
        float oldStress = currentStress;
        currentStress = Mathf.Clamp(currentStress + delta, 0f, maxStress);

        if (!string.IsNullOrEmpty(reason))
        {
            Debug.Log($"[Noa] Stress {(delta > 0 ? "+" : "")}{delta:F1} ({reason}): {oldStress:F1} -> {currentStress:F1}");
        }

        GameEvents.InvokeNPCStressChanged(npcName, currentStress);
    }

    /// <summary>Called when bullying event occurs</summary>
    public void OnBullyingWitnessed()
    {
        ModifyStress(bullyingStressIncrease, "bullying");

        // Add knowledge that player witnessed bullying
        if (LoopManager.Instance != null)
        {
            LoopManager.Instance.AddKnowledge("witnessed_noa_bullying");
        }
    }

    /// <summary>Called when player has a friendly conversation</summary>
    public void OnFriendlyTalk()
    {
        ModifyStress(-friendlyTalkStressDecrease, "friendly talk");
    }

    /// <summary>Called when player helps Noa</summary>
    public void OnPlayerHelped()
    {
        ModifyStress(-helpActionStressDecrease, "player helped");

        if (LoopManager.Instance != null)
        {
            LoopManager.Instance.AddKnowledge("helped_noa");
        }
    }

    /// <summary>Called when Noa is ignored during a critical moment</summary>
    public void OnIgnored()
    {
        ModifyStress(ignoredStressIncrease, "ignored");
    }

    private void OnHourStressUpdate(int hour)
    {
        // Passive stress increase each hour
        ModifyStress(passiveStressPerHour, "passive");
    }

    // ═══════════════════════════════════════════
    // TRAGEDY CHECK (Friday 14:00)
    // ═══════════════════════════════════════════

    private void OnWeekEndCheck()
    {
        if (currentStress >= stressThreshold)
        {
            TriggerTragedy();
        }
        else
        {
            TriggerGoodEnding();
        }
    }

    private void TriggerTragedy()
    {
        tragedyTriggered = true;
        SetState(NPCState.SpecialEvent);
        Debug.Log($"[Noa] TRAGEDY — Stress was {currentStress:F1} (threshold: {stressThreshold})");

        // TODO: Play tragedy cutscene via Timeline
        // The LoopManager will handle the loop reset after this
    }

    private void TriggerGoodEnding()
    {
        tragedyTriggered = false;
        Debug.Log($"[Noa] SAVED — Stress was {currentStress:F1} (threshold: {stressThreshold})");

        // TODO: Play good ending cutscene
        // Add knowledge
        if (LoopManager.Instance != null)
        {
            LoopManager.Instance.AddKnowledge("saved_noa_week_" + LoopManager.Instance.CurrentLoop);
        }
    }

    // ═══════════════════════════════════════════
    // DIALOGUE OVERRIDE
    // ═══════════════════════════════════════════

    protected override void StartDialogue()
    {
        base.StartDialogue();

        // Friendly talk reduces stress
        OnFriendlyTalk();

        // Different dialogue based on stress level and knowledge
        if (LoopManager.Instance != null)
        {
            if (LoopManager.Instance.HasKnowledge("witnessed_noa_bullying"))
            {
                Debug.Log("[Noa] Player knows about bullying — special dialogue available");
            }
        }
    }

    // ═══════════════════════════════════════════
    // ANIMATION OVERRIDE
    // ═══════════════════════════════════════════

    protected override void UpdateAnimations()
    {
        base.UpdateAnimations();

        if (animator == null) return;

        // Stress-based animations
        float stressNorm = StressNormalized;
        animator.SetFloat(StressLevelHash, stressNorm);
        animator.SetBool(IsDistressedHash, currentStress >= highStressThreshold);
    }

    // ═══════════════════════════════════════════
    // LOOP RESET OVERRIDE
    // ═══════════════════════════════════════════

    protected override void OnLoopReset()
    {
        base.OnLoopReset();

        // Reset stress to initial value
        currentStress = initialStress;
        tragedyTriggered = false;

        GameEvents.InvokeNPCStressChanged(npcName, currentStress);
        Debug.Log($"[Noa] Stress reset to {initialStress}");
    }

    // ═══════════════════════════════════════════
    // STRESS VISUAL INDICATOR
    // ═══════════════════════════════════════════

    /// <summary>Returns a description of Noa's current emotional state</summary>
    public string GetEmotionalState()
    {
        if (currentStress < lowStressThreshold)
            return "calm";
        if (currentStress < medStressThreshold)
            return "uneasy";
        if (currentStress < highStressThreshold)
            return "anxious";
        return "distressed";
    }

    // ═══════════════════════════════════════════
    // SAVE/LOAD
    // ═══════════════════════════════════════════

    [System.Serializable]
    public class NoaData
    {
        public float stress;
        public bool tragedyTriggered;
    }

    public NoaData GetSaveData()
    {
        return new NoaData
        {
            stress = currentStress,
            tragedyTriggered = tragedyTriggered
        };
    }

    public void LoadSaveData(NoaData data)
    {
        currentStress = data.stress;
        tragedyTriggered = data.tragedyTriggered;
        GameEvents.InvokeNPCStressChanged(npcName, currentStress);
    }

    // ═══════════════════════════════════════════
    // DEBUG
    // ═══════════════════════════════════════════

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Stress indicator above head
        if (Application.isPlaying)
        {
            Color stressColor = Color.Lerp(Color.green, Color.red, StressNormalized);
            Gizmos.color = stressColor;
            Vector3 headPos = transform.position + Vector3.up * 2f;
            Gizmos.DrawSphere(headPos, 0.15f);
        }
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        if (!Application.isPlaying) return;

        // Show stress bar above NPC in screen space
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPos = transform.position + Vector3.up * 2.2f;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        if (screenPos.z > 0)
        {
            float barWidth = 60f;
            float barHeight = 8f;
            float x = screenPos.x - barWidth / 2f;
            float y = Screen.height - screenPos.y - barHeight;

            // Background
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(x - 1, y - 1, barWidth + 2, barHeight + 2), Texture2D.whiteTexture);

            // Stress bar
            GUI.color = Color.Lerp(Color.green, Color.red, StressNormalized);
            GUI.DrawTexture(new Rect(x, y, barWidth * StressNormalized, barHeight), Texture2D.whiteTexture);

            // Name
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11
            };
            style.normal.textColor = Color.white;
            GUI.color = Color.white;
            GUI.Label(new Rect(x - 10, y - 18, barWidth + 20, 16), $"{npcName} ({GetEmotionalState()})", style);
        }
    }
#endif
}
