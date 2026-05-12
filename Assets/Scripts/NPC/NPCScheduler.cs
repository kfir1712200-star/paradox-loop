using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// Base class for all scheduled NPCs in Project Paradox Loop.
/// Manages schedule following, navigation, state machine, and player awareness.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class NPCScheduler : MonoBehaviour, IInteractable
{
    [Header("NPC Identity")]
    [SerializeField] protected string npcName = "NPC";
    [SerializeField] protected ScheduleData schedule;

    [Header("Navigation")]
    [SerializeField] protected float walkSpeed = 2f;
    [SerializeField] protected float runSpeed = 4f;
    [SerializeField] protected float stoppingDistance = 0.5f;
    [SerializeField] protected float rotationSpeed = 5f;

    [Header("Player Awareness")]
    [SerializeField] protected float detectionRange = 8f;
    [SerializeField] protected float talkRange = 3f;
    [SerializeField] protected float followDetectionAngle = 120f;
    [SerializeField] protected float suspicionBuildRate = 0.1f;
    [SerializeField] protected float suspicionDecayRate = 0.2f;

    [Header("Waypoints")]
    [SerializeField] protected Transform waypointParent;
    protected Dictionary<string, Transform> waypoints = new Dictionary<string, Transform>();

    // Components
    protected NavMeshAgent agent;
    protected Animator animator;

    // State
    protected NPCState currentState = NPCState.Idle;
    protected NPCState previousState;
    protected ScheduleEntry currentScheduleEntry;
    protected Transform currentTarget;

    // Player awareness
    protected Transform playerTransform;
    protected float suspicionLevel = 0f;
    protected bool isPlayerNearby = false;
    protected bool isPlayerFollowing = false;
    protected bool isTalkingToPlayer = false;

    // Animation hashes
    protected static readonly int SpeedHash = Animator.StringToHash("Speed");
    protected static readonly int IsSittingHash = Animator.StringToHash("IsSitting");
    protected static readonly int IsTalkingHash = Animator.StringToHash("IsTalking");

    // IInteractable
    public virtual string InteractionPrompt => isTalkingToPlayer ? "" : $"Talk to {npcName}";
    public virtual bool CanInteract => !isTalkingToPlayer && IsPlayerInRange(talkRange);

    // Properties
    public string NPCName => npcName;
    public NPCState CurrentState => currentState;
    public float SuspicionLevel => suspicionLevel;
    public bool IsTalkingToPlayer => isTalkingToPlayer;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = walkSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.angularSpeed = rotationSpeed * 100f;

        // Cache waypoints
        if (waypointParent != null)
        {
            foreach (Transform child in waypointParent)
            {
                waypoints[child.name] = child;
            }
        }
    }

    protected virtual void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Subscribe to events
        GameEvents.OnHourChanged += OnHourChanged;
        GameEvents.OnDayChanged += OnDayChanged;
        GameEvents.OnLoopReset += OnLoopReset;

        // Initial schedule check
        if (TimeManager.Instance != null)
        {
            UpdateSchedule(TimeManager.Instance.CurrentDay, TimeManager.Instance.CurrentHour);
        }
    }

    protected virtual void OnDestroy()
    {
        GameEvents.OnHourChanged -= OnHourChanged;
        GameEvents.OnDayChanged -= OnDayChanged;
        GameEvents.OnLoopReset -= OnLoopReset;
    }

    protected virtual void Update()
    {
        UpdatePlayerAwareness();
        UpdateStateMachine();
        UpdateAnimations();
    }

    // ═══════════════════════════════════════════
    // SCHEDULE
    // ═══════════════════════════════════════════

    protected virtual void OnHourChanged(int hour)
    {
        if (TimeManager.Instance != null)
        {
            UpdateSchedule(TimeManager.Instance.CurrentDay, hour);
        }
    }

    protected virtual void OnDayChanged(int day)
    {
        if (TimeManager.Instance != null)
        {
            UpdateSchedule(day, TimeManager.Instance.CurrentHour);
        }
    }

    protected virtual void UpdateSchedule(int day, int hour)
    {
        if (schedule == null) return;

        var entry = schedule.GetEntry(day, hour);
        if (entry == null || entry == currentScheduleEntry) return;

        currentScheduleEntry = entry;

        // Navigate to waypoint if specified
        if (!string.IsNullOrEmpty(entry.waypointName) && waypoints.TryGetValue(entry.waypointName, out Transform wp))
        {
            currentTarget = wp;
            SetState(NPCState.Walking);
            NavigateTo(wp.position);
        }
        else
        {
            // Perform action at current location
            SetStateFromAction(entry.action);
        }
    }

    protected void SetStateFromAction(NPCAction action)
    {
        switch (action)
        {
            case NPCAction.Idle:
            case NPCAction.WaitAtLocation:
                SetState(NPCState.Waiting);
                break;
            case NPCAction.Sit:
            case NPCAction.Study:
                SetState(NPCState.Sitting);
                break;
            case NPCAction.Talk:
                SetState(NPCState.Talking);
                break;
            case NPCAction.Eat:
                SetState(NPCState.Eating);
                break;
            case NPCAction.Patrol:
                SetState(NPCState.Patrolling);
                break;
            case NPCAction.SpecialEvent:
                SetState(NPCState.SpecialEvent);
                break;
            default:
                SetState(NPCState.Idle);
                break;
        }
    }

    // ═══════════════════════════════════════════
    // STATE MACHINE
    // ═══════════════════════════════════════════

    protected virtual void SetState(NPCState newState)
    {
        if (currentState == newState) return;

        previousState = currentState;
        OnExitState(currentState);
        currentState = newState;
        OnEnterState(newState);
    }

    protected virtual void OnEnterState(NPCState state)
    {
        switch (state)
        {
            case NPCState.Idle:
            case NPCState.Waiting:
                StopNavigation();
                break;
            case NPCState.Sitting:
                StopNavigation();
                break;
            case NPCState.Talking:
                StopNavigation();
                if (playerTransform != null)
                    LookAtTarget(playerTransform);
                break;
        }
    }

    protected virtual void OnExitState(NPCState state)
    {
        // Clean up previous state if needed
    }

    protected virtual void UpdateStateMachine()
    {
        switch (currentState)
        {
            case NPCState.Walking:
                UpdateWalking();
                break;
            case NPCState.Talking:
                UpdateTalking();
                break;
            case NPCState.Patrolling:
                UpdatePatrolling();
                break;
        }
    }

    protected virtual void UpdateWalking()
    {
        if (agent.enabled && !agent.pathPending && agent.remainingDistance <= stoppingDistance)
        {
            // Arrived at destination
            if (currentScheduleEntry != null)
            {
                SetStateFromAction(currentScheduleEntry.action);
            }
            else
            {
                SetState(NPCState.Idle);
            }
        }
    }

    protected virtual void UpdateTalking()
    {
        if (isTalkingToPlayer && playerTransform != null)
        {
            LookAtTarget(playerTransform);
        }
    }

    protected virtual void UpdatePatrolling()
    {
        // Override in subclasses for patrol behavior
    }

    // ═══════════════════════════════════════════
    // NAVIGATION
    // ═══════════════════════════════════════════

    protected void NavigateTo(Vector3 position)
    {
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(position);
        }
    }

    protected void StopNavigation()
    {
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    protected void LookAtTarget(Transform target)
    {
        if (target == null) return;
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    // ═══════════════════════════════════════════
    // PLAYER AWARENESS
    // ═══════════════════════════════════════════

    protected virtual void UpdatePlayerAwareness()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        isPlayerNearby = distance <= detectionRange;

        if (isPlayerNearby)
        {
            // Check if player is behind the NPC (following)
            Vector3 toPlayer = (playerTransform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, toPlayer);

            // Player is behind and close = suspicious
            if (angle > followDetectionAngle * 0.5f && distance < detectionRange * 0.5f)
            {
                isPlayerFollowing = true;
                suspicionLevel = Mathf.Min(suspicionLevel + suspicionBuildRate * Time.deltaTime, 1f);
            }
            else
            {
                isPlayerFollowing = false;
                suspicionLevel = Mathf.Max(suspicionLevel - suspicionDecayRate * Time.deltaTime, 0f);
            }
        }
        else
        {
            isPlayerFollowing = false;
            suspicionLevel = Mathf.Max(suspicionLevel - suspicionDecayRate * Time.deltaTime, 0f);
        }
    }

    protected bool IsPlayerInRange(float range)
    {
        if (playerTransform == null) return false;
        return Vector3.Distance(transform.position, playerTransform.position) <= range;
    }

    // ═══════════════════════════════════════════
    // INTERACTION
    // ═══════════════════════════════════════════

    public virtual void Interact()
    {
        if (isTalkingToPlayer) return;

        StartDialogue();
    }

    protected virtual void StartDialogue()
    {
        isTalkingToPlayer = true;
        SetState(NPCState.Talking);
        GameEvents.InvokeDialogueStart(npcName);

        // Pause time during dialogue
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetTimePaused(true);
        }

        Debug.Log($"[{npcName}] Started dialogue");

        // TODO: Connect to Dialogue System
        // For now, end dialogue after a moment
        Invoke(nameof(EndDialogue), 2f);
    }

    protected virtual void EndDialogue()
    {
        isTalkingToPlayer = false;
        GameEvents.InvokeDialogueEnd(npcName);

        // Resume time
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetTimePaused(false);
        }

        // Return to previous state
        if (currentScheduleEntry != null)
        {
            SetStateFromAction(currentScheduleEntry.action);
        }
        else
        {
            SetState(NPCState.Idle);
        }

        Debug.Log($"[{npcName}] Ended dialogue");
    }

    // ═══════════════════════════════════════════
    // ANIMATION
    // ═══════════════════════════════════════════

    protected virtual void UpdateAnimations()
    {
        if (animator == null) return;

        float speed = agent.enabled ? agent.velocity.magnitude / walkSpeed : 0f;
        animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
        animator.SetBool(IsSittingHash, currentState == NPCState.Sitting);
        animator.SetBool(IsTalkingHash, currentState == NPCState.Talking);
    }

    // ═══════════════════════════════════════════
    // LOOP RESET
    // ═══════════════════════════════════════════

    protected virtual void OnLoopReset()
    {
        // Reset NPC to initial state
        isTalkingToPlayer = false;
        suspicionLevel = 0f;
        currentScheduleEntry = null;
        SetState(NPCState.Idle);

        Debug.Log($"[{npcName}] Loop reset");
    }

    // ═══════════════════════════════════════════
    // DEBUG
    // ═══════════════════════════════════════════

    protected virtual void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Talk range
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, talkRange);

        // Current target
        if (currentTarget != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, currentTarget.position);
            Gizmos.DrawSphere(currentTarget.position, 0.2f);
        }
    }
}
