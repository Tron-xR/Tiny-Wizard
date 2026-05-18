using UnityEngine;

/// <summary>
/// Main player controller for third-person movement.
/// Handles movement, jumping, sprinting, and animation integration.
/// Uses Rigidbody physics and the NEW Input System through PlayerInputHandler.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    [SerializeField] private GroundChecker groundChecker;
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float airAcceleration = 8f;
    [SerializeField] private float airDeceleration = 3f;

    [Header("Sprint")]
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float sprintAcceleration = 25f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown = 0.1f;
    [SerializeField] private float airDrag = 0.1f;
    [SerializeField] private float groundDrag = 5f;

    [Header("Rotation")]
    [SerializeField] private float rotationSmoothTime = 0.1f;
    [SerializeField] private bool useCharacterRotation = true;

    [Header("Animation")]
    [SerializeField] private float speedAnimationSmoothTime = 0.1f;

    // Movement state
    private Vector3 velocity = Vector3.zero;
    private Vector3 desiredVelocity = Vector3.zero;
    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private float animationSpeed = 0f;
    private float lastJumpTime = -10f;
    private float rotationVelocity = 0f;

    private void OnEnable()
    {
        // Auto-cache references if not assigned
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (groundChecker == null) groundChecker = GetComponent<GroundChecker>();
        if (inputHandler == null) inputHandler = GetComponent<PlayerInputHandler>();
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
    }

    private void FixedUpdate()
    {
        // Update ground state
        groundChecker.UpdateGroundCheck();

        // Handle input and calculate desired velocity
        HandleMovement();

        // Handle jump
        HandleJump();

        // Apply velocity
        ApplyVelocity();

        // Update animations
        UpdateAnimations();
    }

    /// <summary>
    /// Handles movement input and calculates desired velocity.
    /// Movement is relative to camera direction.
    /// </summary>
    private void HandleMovement()
    {
        Vector2 input = inputHandler.MoveInput;
        bool isSprinting = inputHandler.SprintPressed;

        // Convert input to world-space direction based on camera
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Project forward and right onto ground plane to avoid upward movement
        forward -= forward.y * Vector3.up;
        right -= right.y * Vector3.up;

        forward.Normalize();
        right.Normalize();

        // Calculate desired movement direction
        Vector3 desiredDirection = (forward * input.y + right * input.x).normalized;
        
        // Determine target speed
        targetSpeed = isSprinting ? sprintSpeed : moveSpeed;
        if (input.magnitude < 0.01f) targetSpeed = 0f;

        // Get current horizontal velocity
        Vector3 currentVelocityXZ = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        currentSpeed = currentVelocityXZ.magnitude;

        // Calculate desired velocity
        if (input.magnitude > 0.01f)
        {
            float currentAcceleration = groundChecker.IsGrounded ? acceleration : airAcceleration;
            if (isSprinting && groundChecker.IsGrounded) currentAcceleration = sprintAcceleration;

            float accelerationSpeed = Mathf.Lerp(currentSpeed, targetSpeed, currentAcceleration * Time.fixedDeltaTime);
            desiredVelocity = desiredDirection * accelerationSpeed;
        }
        else
        {
            // Decelerate when no input
            float currentDeceleration = groundChecker.IsGrounded ? deceleration : airDeceleration;
            float decelerationSpeed = Mathf.Lerp(currentSpeed, 0f, currentDeceleration * Time.fixedDeltaTime);
            desiredVelocity = currentVelocityXZ.normalized * decelerationSpeed;
        }

        // Rotate character to face movement direction if moving
        if (useCharacterRotation && input.magnitude > 0.01f)
        {
            RotateCharacterToFaceDirection(desiredDirection);
        }

        // Apply drag based on ground state
        rb.drag = groundChecker.IsGrounded ? groundDrag : airDrag;
    }

    /// <summary>
    /// Rotates the character to face the movement direction smoothly.
    /// </summary>
    private void RotateCharacterToFaceDirection(Vector3 direction)
    {
        if (direction.magnitude < 0.01f) return;

        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);
        transform.rotation = Quaternion.Euler(0, angle, 0);
    }

    /// <summary>
    /// Handles jump input and applies jump force.
    /// </summary>
    private void HandleJump()
    {
        if (!inputHandler.JumpPressed) return;
        if (!groundChecker.IsGrounded) return;
        if (Time.time - lastJumpTime < jumpCooldown) return;

        // Apply jump force
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        lastJumpTime = Time.time;

        // Trigger jump animation
        animator.SetTrigger("JumpTrigger");
    }

    /// <summary>
    /// Applies the calculated velocity to the Rigidbody.
    /// Preserves vertical velocity for gravity.
    /// </summary>
    private void ApplyVelocity()
    {
        // Preserve vertical velocity (gravity)
        float verticalVelocity = rb.velocity.y;

        // Apply desired horizontal velocity
        rb.velocity = new Vector3(desiredVelocity.x, verticalVelocity, desiredVelocity.z);
    }

    /// <summary>
    /// Updates animator parameters for animation blending.
    /// </summary>
    private void UpdateAnimations()
    {
        // Smooth animation speed
        float targetAnimationSpeed = inputHandler.MoveInput.magnitude * (inputHandler.SprintPressed ? sprintSpeed : moveSpeed);
        animationSpeed = Mathf.SmoothDamp(animationSpeed, targetAnimationSpeed / moveSpeed, ref rotationVelocity, speedAnimationSmoothTime);

        // Update animator parameters
        animator.SetFloat("Speed", animationSpeed);
        animator.SetBool("IsGrounded", groundChecker.IsGrounded);
    }

    /// <summary>
    /// Gets the current horizontal velocity magnitude.
    /// </summary>
    public float GetCurrentSpeed()
    {
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        return horizontalVelocity.magnitude;
    }

    /// <summary>
    /// Gets whether the player is currently grounded.
    /// </summary>
    public bool IsGrounded => groundChecker.IsGrounded;

    /// <summary>
    /// Gets the current velocity.
    /// </summary>
    public Vector3 GetVelocity() => rb.velocity;
}
