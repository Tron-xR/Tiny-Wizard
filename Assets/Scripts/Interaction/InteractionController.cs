using UnityEngine;

/// <summary>
/// Handles raycast-based interaction detection and dispatches Interact events.
/// Attach to the Player GameObject (or wherever the interaction origin lives).
/// 
/// How it works:
/// 1. Every frame, casts a ray from the center of the screen (camera)
/// 2. If it hits an IInteractable within range, sets it as the focused object
/// 3. Shows/hides the interaction prompt via InteractionUI
/// 4. When the player presses Interact, calls OnInteract on the focused object
/// 
/// Inspector setup:
/// - Camera: drag your Main Camera (raycast origin)
/// - Interaction UI: drag the Canvas child with InteractionUI
/// - Hold Point: drag the HoldPoint child under Player (for pickups)
/// - Input Handler: auto-assigned if on the same GameObject
/// - Interaction Layer: create an "Interactable" layer for performance
/// 
/// Required on this GameObject:
/// - PlayerInputHandler (already exists on Player from scene setup)
/// </summary>
public class InteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera interactionCamera;
    [SerializeField] private InteractionUI interactionUI;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private PlayerInputHandler inputHandler;

    [Header("Raycast Settings")]
    [SerializeField] private float maxInteractionDistance = 5f;
    [SerializeField] private LayerMask interactionLayer = -1;

    // The object the player is currently looking at
    private IInteractable currentFocus = null;
    private IInteractable previousFocus = null;

    // The object currently being held (if any)
    private PickupObject heldObject = null;

    // ===== INITIALISATION =====

    private void OnEnable()
    {
        if (inputHandler == null)
            inputHandler = GetComponent<PlayerInputHandler>();

        if (interactionCamera == null)
            interactionCamera = Camera.main;

        if (inputHandler != null)
            inputHandler.InteractPressed += OnInteractPressed;

        // Create hold point if missing
        if (holdPoint == null)
        {
            GameObject hp = new GameObject("HoldPoint");
            hp.transform.SetParent(transform);
            hp.transform.localPosition = new Vector3(0, 1.2f, 1f);
            holdPoint = hp.transform;
        }
    }

    private void OnDisable()
    {
        if (inputHandler != null)
            inputHandler.InteractPressed -= OnInteractPressed;
    }

    // ===== UPDATE LOOP =====

    private void LateUpdate()
    {
        PerformInteractionRaycast();
        UpdateInteractionUI();
    }

    /// <summary>
    /// Raycasts from the camera center to find the nearest interactable object.
    /// Handles focus enter/exit transitions.
    /// </summary>
    private void PerformInteractionRaycast()
    {
        previousFocus = currentFocus;
        currentFocus = null;

        if (interactionCamera == null) return;

        Ray ray = new Ray(interactionCamera.transform.position, interactionCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxInteractionDistance, interactionLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null && interactable.CanInteract)
            {
                currentFocus = interactable;
            }
        }

        // Handle focus transitions
        if (currentFocus != previousFocus)
        {
            if (previousFocus != null)
                previousFocus.OnFocusExit(this);

            if (currentFocus != null)
                currentFocus.OnFocusEnter(this);
        }
    }

    /// <summary>
    /// Updates the interaction prompt UI based on what the player is looking at.
    /// </summary>
    private void UpdateInteractionUI()
    {
        if (interactionUI == null) return;

        if (currentFocus != null && currentFocus.CanInteract)
        {
            interactionUI.ShowPrompt(currentFocus.PromptText);
        }
        else
        {
            interactionUI.HidePrompt();
        }
    }

    // ===== INTERACT EVENT =====

    /// <summary>
    /// Called when the player presses the Interact button.
    /// If holding an item, drops it first.
    /// Otherwise, interacts with the focused object (if any).
    /// </summary>
    private void OnInteractPressed()
    {
        // Holding an item takes priority — drop it
        if (heldObject != null)
        {
            heldObject.Drop();
            heldObject = null;
            return;
        }

        // Otherwise, interact with the focused object
        if (currentFocus != null && currentFocus.CanInteract)
        {
            currentFocus.OnInteract(this);
        }
    }

    // ===== PICKUP MANAGEMENT =====

    /// <summary>
    /// Stores a reference to the currently held object.
    /// Called by PickupObject when it's picked up.
    /// </summary>
    public void SetHeldObject(PickupObject obj)
    {
        heldObject = obj;
    }

    /// <summary>
    /// Clears the held object reference.
    /// Called by PickupObject when it's dropped.
    /// </summary>
    public void ClearHeldObject()
    {
        heldObject = null;
    }

    /// <summary>
    /// Whether the player is currently holding an object.
    /// </summary>
    public bool IsHoldingObject => heldObject != null;

    /// <summary>
    /// Returns the hold position transform for pickups to follow.
    /// </summary>
    public Transform HoldPoint => holdPoint;

    /// <summary>
    /// Returns the current interactable the player is looking at.
    /// </summary>
    public IInteractable CurrentFocus => currentFocus;

    /// <summary>
    /// Returns the currently held PickupObject (null if nothing held).
    /// </summary>
    public PickupObject HeldObject => heldObject;

    // ===== EDITOR VISUALISATION =====

    private void OnDrawGizmosSelected()
    {
        if (interactionCamera == null) return;

        Gizmos.color = Color.cyan;
        Vector3 rayEnd = interactionCamera.transform.position + interactionCamera.transform.forward * maxInteractionDistance;
        Gizmos.DrawLine(interactionCamera.transform.position, rayEnd);

        if (currentFocus != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentFocus.InteractionTransform.position, 0.3f);
        }
    }
}
