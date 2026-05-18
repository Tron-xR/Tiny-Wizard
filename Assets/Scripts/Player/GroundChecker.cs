using UnityEngine;

/// <summary>
/// Handles ground detection using raycasts.
/// Determines if the player is grounded and provides ground hit information.
/// </summary>
public class GroundChecker : MonoBehaviour
{
    [Header("Ground Detection")]
    [SerializeField] private float raycastDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector3 raycastOffset = Vector3.zero;
    
    [Header("Ground Settings")]
    [SerializeField] private int raycastCount = 3;
    [SerializeField] private float raycastRadius = 0.3f;
    
    private Rigidbody rb;
    private Collider playerCollider;
    private bool isGrounded;
    private bool wasGrounded;
    private RaycastHit groundHit;
    private const float GROUND_BUFFER_TIME = 0.05f;
    private float lastGroundedTime = -10f;
    private Vector3 bottomOffset = Vector3.zero;

    /// <summary>
    /// Returns true if the player is currently grounded.
    /// </summary>
    public bool IsGrounded => isGrounded;
    
    /// <summary>
    /// Returns the raycast hit information from the ground.
    /// </summary>
    public RaycastHit GroundHit => groundHit;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();

        if (playerCollider is CapsuleCollider capsule)
            bottomOffset = Vector3.down * (capsule.height * 0.5f - capsule.center.y);
        else if (playerCollider is BoxCollider box)
            bottomOffset = Vector3.down * (box.size.y * 0.5f - box.center.y);
        else if (playerCollider is SphereCollider sphere)
            bottomOffset = Vector3.down * sphere.radius;

        if (groundLayer == 0)
            groundLayer = ~0;
    }

    /// <summary>
    /// Checks if player is grounded using multiple raycasts.
    /// Should be called before physics calculations.
    /// </summary>
    public void UpdateGroundCheck()
    {
        bool foundGround = false;
        Vector3 rayOrigin = rb.position + bottomOffset + raycastOffset;

        for (int i = 0; i < raycastCount; i++)
        {
            float angle = (360f / raycastCount) * i;
            Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 rayStartPos = rayOrigin + rayDirection * raycastRadius;

            if (Physics.Raycast(rayStartPos, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
            {
                foundGround = true;
                groundHit = hit;
                break;
            }
        }

        if (!foundGround)
        {
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit centerHit, raycastDistance, groundLayer))
            {
                foundGround = true;
                groundHit = centerHit;
            }
        }

        if (foundGround)
            lastGroundedTime = Time.time;

        isGrounded = foundGround || (Time.time - lastGroundedTime < GROUND_BUFFER_TIME);
        wasGrounded = isGrounded;
    }

    /// <summary>
    /// Visualize raycast in editor for debugging.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Vector3 rayOrigin = transform.position + bottomOffset + raycastOffset;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * raycastDistance);

        // Draw raycast circle
        Gizmos.color = Color.yellow;
        for (int i = 0; i < raycastCount; i++)
        {
            float angle = (360f / raycastCount) * i;
            Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 rayStartPos = rayOrigin + rayDirection * raycastRadius;
            Gizmos.DrawLine(rayStartPos, rayStartPos + Vector3.down * raycastDistance);
        }
    }
}
