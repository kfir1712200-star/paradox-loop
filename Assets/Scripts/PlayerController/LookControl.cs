using UnityEngine;

public class LookControl : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The player transform to orbit around")]
    public Transform target;

    [Header("Distance")]
    public float distance = 5f;
    public float minDistance = 1.5f;
    public float maxDistance = 15f;
    public float scrollSpeed = 2f;

    [Header("Sensitivity")]
    public float mouseSensitivityX = 3f;
    public float mouseSensitivityY = 2f;

    [Header("Vertical Limits")]
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 75f;

    [Header("Height Offset")]
    public float targetHeightOffset = 1.5f;

    [Header("Zoom (Right Click)")]
    public float zoomDistance = 1.5f;
    public float zoomSpeed = 8f;

    [Header("Collision")]
    public float collisionBuffer = 0.2f;
    public LayerMask collisionLayers = ~0;

    float yaw;
    float pitch = 15f;
    float currentDistance;
    bool isZooming;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("LookControl: No target assigned! Trying to find a player...");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        currentDistance = distance;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // Unlock cursor with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Lock cursor on click
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Mouse input
        yaw += Input.GetAxis("Mouse X") * mouseSensitivityX;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivityY;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        // Scroll wheel zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * scrollSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Right click zoom
        isZooming = Input.GetMouseButton(1);
        float targetDistance = isZooming ? zoomDistance : distance;

        // Calculate target position with height offset
        Vector3 targetPos = target.position + Vector3.up * targetHeightOffset;

        // Calculate desired camera position
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = targetPos - rotation * Vector3.forward * targetDistance;

        // Camera collision - prevent clipping through walls
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
        if (Physics.SphereCast(targetPos, collisionBuffer, desiredPosition - targetPos,
            out RaycastHit hit, currentDistance, collisionLayers))
        {
            currentDistance = hit.distance - collisionBuffer;
            currentDistance = Mathf.Max(currentDistance, 0.1f);
        }

        Vector3 finalPosition = targetPos - rotation * Vector3.forward * currentDistance;

        transform.position = finalPosition;
        transform.LookAt(targetPos);
    }
}
