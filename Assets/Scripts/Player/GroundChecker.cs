using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [Header("Ground Detection")]
    [SerializeField] private float raycastDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ground Settings")]
    [SerializeField] private int raycastCount = 3;
    [SerializeField] private float raycastRadius = 0.3f;

    private Rigidbody rb;
    private Collider playerCollider;
    private bool isGrounded;
    private RaycastHit groundHit;
    private Vector3 bottomOffset = Vector3.zero;
    private const float SKIN_OFFSET = 0.05f;

    public bool IsGrounded => isGrounded;
    public RaycastHit GroundHit => groundHit;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();

        if (playerCollider is CapsuleCollider capsule)
            bottomOffset = Vector3.down * (capsule.height * 0.5f - capsule.center.y - SKIN_OFFSET);
        else if (playerCollider is BoxCollider box)
            bottomOffset = Vector3.down * (box.size.y * 0.5f - box.center.y - SKIN_OFFSET);
        else if (playerCollider is SphereCollider sphere)
            bottomOffset = Vector3.down * (sphere.radius - SKIN_OFFSET);

        if (groundLayer == 0)
            groundLayer = ~0;
    }

    public void UpdateGroundCheck()
    {
        Vector3 rayOrigin = rb.position + bottomOffset;
        bool foundGround = false;

        for (int i = 0; i < raycastCount; i++)
        {
            float angle = (360f / raycastCount) * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 start = rayOrigin + dir * raycastRadius;

            if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
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

        if (!foundGround && playerCollider != null)
        {
            Collider[] hits = Physics.OverlapSphere(rayOrigin + Vector3.down * 0.1f, 0.15f, groundLayer);
            if (hits.Length > 0)
            {
                foundGround = true;
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i] != playerCollider)
                    {
                        groundHit = new RaycastHit();
                        break;
                    }
                }
            }
        }

        isGrounded = foundGround;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + bottomOffset;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + Vector3.down * raycastDistance);

        Gizmos.color = Color.yellow;
        for (int i = 0; i < raycastCount; i++)
        {
            float angle = (360f / raycastCount) * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 start = origin + dir * raycastRadius;
            Gizmos.DrawLine(start, start + Vector3.down * raycastDistance);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin + Vector3.down * 0.1f, 0.15f);
    }
}
