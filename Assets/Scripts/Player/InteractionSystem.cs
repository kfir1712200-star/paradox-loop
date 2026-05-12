using UnityEngine;
using TMPro;

/// <summary>
/// Handles player interaction with objects in the world.
/// Uses raycasting to detect IInteractable objects.
/// </summary>
public class InteractionSystem : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactionLayers = ~0;
    [SerializeField] private Transform raycastOrigin; // Usually the camera
    
    [Header("UI")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;
    
    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private IInteractable currentTarget;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        
        if (raycastOrigin == null && mainCamera != null)
        {
            raycastOrigin = mainCamera.transform;
        }

        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }

    void Update()
    {
        CheckForInteractable();
        
        if (currentTarget != null && currentTarget.CanInteract && Input.GetKeyDown(interactKey))
        {
            currentTarget.Interact();
            GameEvents.InvokePlayerInteract(currentTarget);
        }
    }

    private void CheckForInteractable()
    {
        if (raycastOrigin == null) return;

        Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionLayers))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable == null)
            {
                // Check parent
                interactable = hit.collider.GetComponentInParent<IInteractable>();
            }

            if (interactable != null && interactable.CanInteract)
            {
                if (currentTarget != interactable)
                {
                    currentTarget = interactable;
                    ShowPrompt(interactable.InteractionPrompt);
                }
                return;
            }
        }

        // Nothing found
        if (currentTarget != null)
        {
            currentTarget = null;
            HidePrompt();
        }
    }

    private void ShowPrompt(string text)
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(true);
        }
        if (promptText != null)
        {
            promptText.text = $"[E] {text}";
        }
    }

    private void HidePrompt()
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (raycastOrigin != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(raycastOrigin.position, raycastOrigin.forward * interactionRange);
        }
    }
}
