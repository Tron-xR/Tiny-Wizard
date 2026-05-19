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

    [Header("Rotation")]
    [SerializeField] private float rotationSmoothTime = 0.1f;
    [SerializeField] private bool rotateToFaceMovement = true;

    [Header("Animation")]
    [SerializeField] private float speedAnimationSmoothTime = 0.1f;

    private CapsuleCollider capsuleCollider;
    private Vector3 moveVelocity = Vector3.zero;
    private float verticalVelocity = 0f;
    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private float animationSpeed = 0f;
    private float rotationVelocity = 0f;
    private float animationSpeedVelocity = 0f;

    private void OnEnable()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (groundChecker == null) groundChecker = GetComponent<GroundChecker>();
        if (inputHandler == null) inputHandler = GetComponent<PlayerInputHandler>();
        if (cameraTransform == null) cameraTransform = Camera.main?.transform;
        if (capsuleCollider == null) capsuleCollider = GetComponent<CapsuleCollider>();

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        verticalVelocity = 0f;
        moveVelocity = Vector3.zero;
    }

    private void FixedUpdate()
    {
        groundChecker.UpdateGroundCheck();
        HandleMovement();
        HandleJump();
        ApplyGravity();
        ApplyMovement();
        UpdateAnimations();
    }

    private void ApplyGravity()
    {
        if (groundChecker.IsGrounded && verticalVelocity <= 0f)
        {
            verticalVelocity = 0f;
            return;
        }
        verticalVelocity += Physics.gravity.y * Time.fixedDeltaTime;
    }

    private void HandleJump()
    {
        if (inputHandler == null) return;
        if (!inputHandler.ConsumeJump() || !groundChecker.IsGrounded) return;

        verticalVelocity = jumpForce;
        if (HasValidAnimator())
            animator.SetTrigger("JumpTrigger");
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

        Vector3 currentVelocityXZ = new Vector3(moveVelocity.x, 0, moveVelocity.z);
        currentSpeed = currentVelocityXZ.magnitude;

        if (input.magnitude > 0.01f)
        {
            float accel = groundChecker.IsGrounded ? acceleration : airAcceleration;
            if (isSprinting && groundChecker.IsGrounded) accel = sprintAcceleration;
            float speed = Mathf.Lerp(currentSpeed, targetSpeed, accel * Time.fixedDeltaTime);
            moveVelocity = desiredDirection * speed;
        }
        else
        {
            float decel = groundChecker.IsGrounded ? deceleration : airDeceleration;
            float speed = Mathf.Lerp(currentSpeed, 0f, decel * Time.fixedDeltaTime);
            moveVelocity = currentVelocityXZ.normalized * speed;
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

    private void ApplyMovement()
    {
        Vector3 hMove = moveVelocity * Time.fixedDeltaTime;
        float yMove = verticalVelocity * Time.fixedDeltaTime;

        Vector3 safeH = SlideMove(hMove);

        rb.MovePosition(rb.position + new Vector3(safeH.x, yMove, safeH.z));
    }

    private Vector3 SlideMove(Vector3 hMove)
    {
        if (hMove.magnitude < 0.0001f) return Vector3.zero;

        float radius = capsuleCollider.radius;
        Vector3 center = rb.position + capsuleCollider.center;
        float halfH = capsuleCollider.height * 0.5f - capsuleCollider.radius;
        Vector3 top = center + Vector3.up * halfH;
        Vector3 bottom = center - Vector3.up * halfH;
        Vector3 dir = hMove.normalized;
        float castDist = hMove.magnitude + 0.02f;

        RaycastHit hit;
        if (Physics.CapsuleCast(top, bottom, radius, dir, out hit, castDist, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.normal.y > 0.7f)
                return Vector3.zero;

            float safe = Mathf.Max(0f, hit.distance - 0.02f);
            safe = Mathf.Min(safe, hMove.magnitude);
            Vector3 slideDir = Vector3.ProjectOnPlane(dir, hit.normal).normalized;
            float remaining = hMove.magnitude - safe;
            return dir * safe + slideDir * remaining;
        }

        return hMove;
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
        return new Vector3(moveVelocity.x, 0, moveVelocity.z).magnitude;
    }

    public bool IsGrounded => groundChecker.IsGrounded;
    public Vector3 GetVelocity()
    {
        Vector3 vel = moveVelocity;
        vel.y = verticalVelocity;
        return vel;
    }

    private bool HasValidAnimator()
    {
        return animator != null && animator.runtimeAnimatorController != null;
    }
}
