using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Movement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float crouchSpeed = 2f;
    public float rotationSmoothSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 7f;
    public float gravity = -20f;

    [Header("Crouch")]
    public float standHeight = 2f;
    public float crouchHeight = 1.2f;
    public float crouchTransitionSpeed = 8f;

    [Header("Ladder")]
    public float ladderClimbSpeed = 3f;
    public LayerMask ladderLayer;

    [Header("Camera Reference")]
    public Transform cameraTransform;

    CharacterController controller;
    Vector3 velocity;
    bool isCrouching;
    bool isOnLadder;
    float currentHeight;

    bool isRunning;

    // Public state for animations
    [HideInInspector] public bool IsGrounded;
    [HideInInspector] public bool IsCrouching => isCrouching;
    [HideInInspector] public bool IsRunning => isRunning;
    [HideInInspector] public bool IsOnLadder => isOnLadder;
    [HideInInspector] public bool IsJumping;
    [HideInInspector] public float MoveSpeed;
    [HideInInspector] public float VerticalSpeed;
    [HideInInspector] public float NormalizedSpeed; // 0=idle, 0.5=walk, 1=run

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentHeight = standHeight;

        if (cameraTransform == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
                cameraTransform = cam.transform;
        }
    }

    void Update()
    {
        IsGrounded = controller.isGrounded;

        if (isOnLadder)
        {
            HandleLadderMovement();
        }
        else
        {
            HandleMovement();
            HandleJump();
            ApplyGravity();
        }

        HandleCrouch();

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0f, v).normalized;

        // Sprint with Left Shift (can't sprint while crouching)
        isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching && input.magnitude > 0.1f;
        
        float speed = isCrouching ? crouchSpeed : (isRunning ? runSpeed : walkSpeed);
        MoveSpeed = input.magnitude * speed;
        
        // Normalized speed for blend tree: 0=idle, 0.5=walk, 1=run
        if (input.magnitude < 0.1f)
            NormalizedSpeed = Mathf.Lerp(NormalizedSpeed, 0f, Time.deltaTime * 10f);
        else if (isRunning)
            NormalizedSpeed = Mathf.Lerp(NormalizedSpeed, 1f, Time.deltaTime * 8f);
        else
            NormalizedSpeed = Mathf.Lerp(NormalizedSpeed, 0.5f, Time.deltaTime * 8f);

        if (input.magnitude >= 0.1f)
        {
            // Movement relative to camera direction
            float targetAngle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float smoothAngle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, Time.deltaTime * rotationSmoothSpeed);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            velocity.x = moveDir.x * speed;
            velocity.z = moveDir.z * speed;
        }
        else
        {
            velocity.x = 0f;
            velocity.z = 0f;
        }
    }

    void HandleJump()
    {
        if (IsGrounded && Input.GetKeyDown(KeyCode.Space) && !isCrouching)
        {
            velocity.y = jumpForce;
            IsJumping = true;
        }

        if (IsGrounded && velocity.y <= 0f)
        {
            IsJumping = false;
        }
    }

    void ApplyGravity()
    {
        if (IsGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        VerticalSpeed = velocity.y;
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isCrouching)
            {
                // Check if there's space to stand up
                if (!Physics.Raycast(transform.position, Vector3.up, standHeight, ~0, QueryTriggerInteraction.Ignore))
                {
                    isCrouching = false;
                }
            }
            else
            {
                isCrouching = true;
            }
        }

        float targetHeight = isCrouching ? crouchHeight : standHeight;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        controller.height = currentHeight;
        controller.center = new Vector3(0f, currentHeight / 2f, 0f);
    }

    void HandleLadderMovement()
    {
        float v = Input.GetAxisRaw("Vertical");
        velocity = new Vector3(0f, v * ladderClimbSpeed, 0f);
        VerticalSpeed = velocity.y;
        MoveSpeed = Mathf.Abs(v);

        // Jump off ladder
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isOnLadder = false;
            velocity.y = jumpForce * 0.5f;
            IsJumping = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & ladderLayer) != 0)
        {
            isOnLadder = true;
            velocity = Vector3.zero;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & ladderLayer) != 0)
        {
            isOnLadder = false;
        }
    }
}
