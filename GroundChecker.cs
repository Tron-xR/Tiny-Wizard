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
    private bool isGrounded;
    private RaycastHit groundHit;

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
    }

    /// <summary>
    /// Checks if player is grounded using multiple raycasts.
    /// Should be called before physics calculations.
    /// </summary>
    public void UpdateGroundCheck()
    {
        isGrounded = false;
        Vector3 rayOrigin = transform.position + raycastOffset;

        // Perform multiple raycasts in a circle pattern for better accuracy
        for (int i = 0; i < raycastCount; i++)
        {
            float angle = (360f / raycastCount) * i;
            Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 rayStartPos = rayOrigin + rayDirection * raycastRadius;

            // Cast down from the offset position
            if (Physics.Raycast(rayStartPos, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
            {
                isGrounded = true;
                groundHit = hit;
                break;
            }
        }

        // Fallback: simple raycast from center if circular pattern misses
        if (!isGrounded)
        {
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit centerHit, raycastDistance, groundLayer))
            {
                isGrounded = true;
                groundHit = centerHit;
            }
        }
    }

    /// <summary>
    /// Visualize raycast in editor for debugging.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Vector3 rayOrigin = transform.position + raycastOffset;
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
