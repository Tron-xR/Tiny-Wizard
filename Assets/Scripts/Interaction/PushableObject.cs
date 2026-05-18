using UnityEngine;

/// <summary>
/// An object that can be pushed by the player using the Interact button.
/// Extends InteractableObject with toggle-based push mechanics.
/// 
/// How it works:
/// - Look at the object and press Interact to start pushing
/// - While pushing, the object receives force based on the camera's forward direction
/// - Press Interact again (or walk away) to stop pushing
/// - Uses Rigidbody forces for physics-based pushing
/// 
/// Inspector setup:
/// - Requires a Rigidbody (mass affects push feel)
/// - Requires a Collider (non-trigger recommended for realistic physics)
/// - Set Push Force to control how easily the object moves
/// - Assign to an "Interactable" layer
/// 
/// Prefab structure:
///   Spoon (PushableObject + Rigidbody + Collider + Model visuals)
///   └── Model_Visuals (child with MeshRenderer)
/// 
/// Collider recommendation:
/// - BoxCollider or CapsuleCollider for stability
/// - Avoid SphereCollider for grounded objects (can roll away)
/// 
/// Rigidbody recommendations:
/// - Mass: 2-10 (lighter = easier to push)
/// - Drag: 1-3 (higher = less sliding after stop)
/// - Angular Drag: 5+ (prevents spinning)
/// - Freeze Rotation: XZ (prevents toppling for box-shaped objects)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PushableObject : InteractableObject
{
    [Header("Push Settings")]
    [SerializeField] private float pushForce = 8f;
    [SerializeField] private float maxPushSpeed = 5f;
    [SerializeField] private float pushStoppingDistance = 3f;
    [SerializeField] private bool requirePlayerMovement = true;

    [Header("Camera-Relative Pushing")]
    [SerializeField] private bool pushInLookDirection = true;

    // Reference components
    private Rigidbody rb;
    private InteractionController activeController;

    // State
    private bool isBeingPushed = false;

    // ===== INITIALISATION =====

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
    }

    // ===== INTERFACE METHODS =====

    /// <summary>
    /// Toggle push mode on/off when the player interacts.
    /// </summary>
    public override void OnInteract(InteractionController controller)
    {
        if (isBeingPushed)
        {
            StopPushing();
        }
        else
        {
            StartPushing(controller);
        }
    }

    /// <summary>Visual feedback when the player looks at this object.</summary>
    public override void OnFocusEnter(InteractionController controller)
    {
        // Override to add push-highlight effect
    }

    /// <summary>Cleanup when the player looks away.</summary>
    public override void OnFocusExit(InteractionController controller)
    {
        // Optionally stop pushing if the player looks away
    }

    // ===== CORE PUSH LOGIC =====

    /// <summary>
    /// Start pushing this object. The object will receive force in the look direction.
    /// </summary>
    private void StartPushing(InteractionController controller)
    {
        if (controller == null || rb == null) return;

        activeController = controller;
        isBeingPushed = true;

        SetPromptText("Release");
    }

    /// <summary>
    /// Stop pushing the object and let it settle naturally.
    /// </summary>
    public void StopPushing()
    {
        if (!isBeingPushed) return;

        isBeingPushed = false;
        activeController = null;

        SetPromptText("Push");
    }

    // ===== FIXED UPDATE (physics-based pushing) =====

    private void FixedUpdate()
    {
        if (!isBeingPushed || rb == null || activeController == null)
            return;

        // Check if the player is too far away — auto-stop
        float dist = Vector3.Distance(transform.position, activeController.transform.position);
        if (dist > pushStoppingDistance)
        {
            StopPushing();
            return;
        }

        if (requirePlayerMovement && activeController.TryGetComponent(out PlayerInputHandler inputHandler))
        {
            // Only push when the player is actually moving
            if (inputHandler.MoveInput.magnitude < 0.05f)
                return;
        }

        // Calculate push direction
        Vector3 pushDir;

        if (pushInLookDirection && Camera.main != null)
        {
            // Push in the camera's forward direction (projected to XZ plane)
            pushDir = Camera.main.transform.forward;
            pushDir.y = 0f;
            pushDir.Normalize();
        }
        else
        {
            // Push away from the player's position (simple radial push)
            pushDir = transform.position - activeController.transform.position;
            pushDir.y = 0f;
            pushDir.Normalize();
        }

        // Apply force with velocity cap
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 desiredVelocity = pushDir * maxPushSpeed;

        // Only push in the desired direction, don't pull
        Vector3 velocityChange = desiredVelocity - currentVelocity;
        float dotProduct = Vector3.Dot(velocityChange, pushDir);
        if (dotProduct > 0f)
        {
            rb.AddForce(pushDir * pushForce, ForceMode.Acceleration);
        }

        // Counteract lateral movement (keep object stable)
        Vector3 lateralVelocity = currentVelocity - Vector3.Project(currentVelocity, pushDir);
        rb.AddForce(-lateralVelocity * 2f, ForceMode.Acceleration);
    }

    // ===== PUBLIC ACCESSORS =====

    /// <summary>Whether this object is currently being pushed.</summary>
    public bool IsBeingPushed => isBeingPushed;

    /// <summary>Force-stop pushing from external scripts (e.g. if the player dies).</summary>
    public void ForceStop()
    {
        StopPushing();
    }
}
