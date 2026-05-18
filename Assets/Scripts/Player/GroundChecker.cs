using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [Header("Ground Detection")]
    [SerializeField] private float checkDistance = 1f;
    [SerializeField] private float checkRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector3 offset = Vector3.zero;

    private Rigidbody rb;
    private Collider playerCollider;
    private bool isGrounded;

    public bool IsGrounded => isGrounded;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();

        if (groundLayer == 0)
            groundLayer = ~0;
    }

    public void UpdateGroundCheck()
    {
        Vector3 origin = rb.position + Vector3.down * checkDistance + offset;

        Collider[] hits = Physics.OverlapSphere(origin, checkRadius, groundLayer);

        isGrounded = false;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] != playerCollider)
            {
                isGrounded = true;
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.down * checkDistance + offset;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(origin, checkRadius);
    }
}
