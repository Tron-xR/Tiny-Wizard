using UnityEngine;

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
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float jumpCooldown = 0.1f;

    [Header("Rotation")]
    [SerializeField] private float rotationSmoothTime = 0.1f;
    [SerializeField] private bool rotateToFaceMovement = true;

    [Header("Animation")]
    [SerializeField] private float speedAnimationSmoothTime = 0.1f;

    private Vector3 desiredVelocity = Vector3.zero;
    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private float animationSpeed = 0f;
    private float lastJumpTime = -10f;
    private float rotationVelocity = 0f;
    private float animationSpeedVelocity = 0f;

    private void OnEnable()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (groundChecker == null) groundChecker = GetComponent<GroundChecker>();
        if (inputHandler == null) inputHandler = GetComponent<PlayerInputHandler>();
        if (cameraTransform == null) cameraTransform = Camera.main?.transform;

        if (inputHandler != null)
            inputHandler.JumpPressed += OnJumpRequested;

        rb.linearDamping = 0f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void OnDisable()
    {
        if (inputHandler != null)
            inputHandler.JumpPressed -= OnJumpRequested;
    }

    private void OnJumpRequested()
    {
        if (Time.time - lastJumpTime < jumpCooldown) return;

        if (!groundChecker.IsGrounded)
        {
            Debug.Log("Jump blocked: not grounded");
            return;
        }

        lastJumpTime = Time.time;

        Vector3 vel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(vel.x, jumpForce, vel.z);

        if (HasValidAnimator())
            animator.SetTrigger("JumpTrigger");
    }

    private void FixedUpdate()
    {
        groundChecker.UpdateGroundCheck();
        HandleMovement();
        ApplyVelocity();
        UpdateAnimations();
    }

    private void HandleMovement()
    {
        Vector2 input = inputHandler.MoveInput;
        bool isSprinting = inputHandler.IsSprinting;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward -= forward.y * Vector3.up;
        right -= right.y * Vector3.up;
        forward.Normalize();
        right.Normalize();

        Vector3 desiredDirection = (forward * input.y + right * input.x).normalized;

        targetSpeed = isSprinting ? sprintSpeed : moveSpeed;
        if (input.magnitude < 0.01f) targetSpeed = 0f;

        Vector3 currentVelocityXZ = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        currentSpeed = currentVelocityXZ.magnitude;

        if (input.magnitude > 0.01f)
        {
            float currentAcceleration = groundChecker.IsGrounded ? acceleration : airAcceleration;
            if (isSprinting && groundChecker.IsGrounded) currentAcceleration = sprintAcceleration;

            float speed = Mathf.Lerp(currentSpeed, targetSpeed, currentAcceleration * Time.fixedDeltaTime);
            desiredVelocity = desiredDirection * speed;
        }
        else
        {
            float currentDeceleration = groundChecker.IsGrounded ? deceleration : airDeceleration;
            float speed = Mathf.Lerp(currentSpeed, 0f, currentDeceleration * Time.fixedDeltaTime);
            desiredVelocity = currentVelocityXZ.normalized * speed;
        }

        if (rotateToFaceMovement && input.magnitude > 0.01f)
            RotateTowards(desiredDirection);
    }

    private void RotateTowards(Vector3 direction)
    {
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);
        rb.MoveRotation(Quaternion.Euler(0, angle, 0));
    }

    private void ApplyVelocity()
    {
        float verticalVelocity = rb.linearVelocity.y;
        rb.linearVelocity = new Vector3(desiredVelocity.x, verticalVelocity, desiredVelocity.z);
    }

    private void UpdateAnimations()
    {
        float targetNorm = inputHandler.MoveInput.magnitude * (inputHandler.IsSprinting ? sprintSpeed / moveSpeed : 1f);
        animationSpeed = Mathf.SmoothDamp(animationSpeed, targetNorm, ref animationSpeedVelocity, speedAnimationSmoothTime);

        if (HasValidAnimator())
        {
            animator.SetFloat("Speed", animationSpeed);
            animator.SetBool("IsGrounded", groundChecker.IsGrounded);
        }
    }

    public float GetCurrentSpeed()
    {
        Vector3 hVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        return hVel.magnitude;
    }

    public bool IsGrounded => groundChecker.IsGrounded;
    public Vector3 GetVelocity() => rb.linearVelocity;

    private bool HasValidAnimator()
    {
        return animator != null && animator.runtimeAnimatorController != null;
    }
}
