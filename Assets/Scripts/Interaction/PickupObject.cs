using UnityEngine;

/// <summary>
/// An object that can be picked up, held, and dropped by the player.
/// Extends InteractableObject with pickup/drop logic.
/// 
/// How it works:
/// - When the player looks at this object and presses Interact, it's picked up
/// - While held, physics is disabled and the object smoothly follows the HoldPoint
/// - Press Interact again to drop (or throw with optional velocity)
/// - Dropped objects regain physics naturally
/// 
/// Inspector setup:
/// - Requires a Collider (non-trigger for physics, trigger for simple pickups)
/// - Requires a Rigidbody if using physics-based pickup
/// - Assign to an "Interactable" layer
/// - Set Prompt Text to something like "Pick up bread" or "Take ingredient"
/// 
/// Prefab structure:
///   Bread (PickupObject + Rigidbody + Collider)
///   ├── Model_Visuals (child with MeshRenderer)
///   └── Collider child (optional, for separate collision)
/// </summary>
[RequireComponent(typeof(Collider))]
public class PickupObject : InteractableObject
{
    [Header("Pickup Settings")]
    [SerializeField] private float followSpeed = 15f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private bool usePhysicsWhileHeld = false;
    [SerializeField] private float throwForceOnDrop = 2f;

    [Header("Drop Settings")]
    [SerializeField] private bool enableGravityOnDrop = true;
    [SerializeField] private bool resetVelocityOnDrop = false;

    // Reference components
    private Rigidbody rb;
    private Collider objectCollider;
    private InteractionController activeController;
    private Camera mainCamera;

    // State
    private bool isHeld = false;
    private string originalPromptText;

    // ===== INITIALISATION =====

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();
        mainCamera = Camera.main;
    }

    // ===== INTERFACE METHODS =====

    /// <summary>
    /// Toggle pickup/drop when the player interacts.
    /// If already held by someone else, ignore.
    /// </summary>
    public override void OnInteract(InteractionController controller)
    {
        if (isHeld)
        {
            Drop();
        }
        else
        {
            PickUp(controller);
        }
    }

    /// <summary>Visual feedback when the player looks at this object.</summary>
    public override void OnFocusEnter(InteractionController controller)
    {
        // Override to add glow effects, outline, or highlight
    }

    /// <summary>Cleanup when the player looks away.</summary>
    public override void OnFocusExit(InteractionController controller)
    {
        // Override to remove highlights
    }

    // ===== CORE PICKUP LOGIC =====

    /// <summary>
    /// Picks up the object, disabling physics and attaching it to the hold point.
    /// </summary>
    public void PickUp(InteractionController controller)
    {
        if (controller == null || controller.HoldPoint == null) return;

        activeController = controller;

        // Notify the controller
        activeController.SetHeldObject(this);

        // Disable physics while held
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Make the collider a trigger so it doesn't push other objects
        if (objectCollider != null)
            objectCollider.isTrigger = usePhysicsWhileHeld;

        // Set transform parent and snap precisely to the hold point
        transform.SetParent(activeController.HoldPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        isHeld = true;

        // Update prompt text to show drop action
        originalPromptText = promptText;
        SetPromptText("Drop");
    }

    /// <summary>
    /// Drops the object, re-enabling physics and letting it fall naturally.
    /// Optionally applies a throw force based on player velocity.
    /// </summary>
    public void Drop()
    {
        if (!isHeld || activeController == null) return;

        // Clear the held reference
        activeController.ClearHeldObject();

        // Detach from hold point
        transform.SetParent(null);

        // Re-enable physics
        if (rb != null)
        {
            rb.isKinematic = false;

            if (enableGravityOnDrop)
                rb.useGravity = true;

            if (resetVelocityOnDrop)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else
            {
                // Add a small throw force in the camera's forward direction
                if (activeController != null && mainCamera != null)
                {
                    Vector3 throwDir = mainCamera.transform.forward;
                    throwDir.y = 0.25f;
                    rb.AddForce(throwDir * throwForceOnDrop, ForceMode.Impulse);
                }
            }
        }

        // Restore collider state
        if (objectCollider != null)
            objectCollider.isTrigger = false;

        isHeld = false;
        activeController = null;

        // Reset prompt text
        SetPromptText(originalPromptText);
    }

    // ===== UPDATE LOOP (when held) =====

    private void Update()
    {
        if (!isHeld || activeController == null || activeController.HoldPoint == null) return;

        // Smoothly move to the hold position
        transform.position = Vector3.Lerp(transform.position, activeController.HoldPoint.position, followSpeed * Time.deltaTime);

        // Smoothly match the hold point's rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, activeController.HoldPoint.rotation, rotationSpeed * Time.deltaTime);
    }

    // ===== PUBLIC ACCESSORS =====

    /// <summary>Whether this object is currently held by the player.</summary>
    public bool IsHeld => isHeld;

    /// <summary>Reference to the active interaction controller.</summary>
    public InteractionController ActiveController => activeController;
}
