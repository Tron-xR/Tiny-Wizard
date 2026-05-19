using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [Header("Ground Detection")]
    [SerializeField] private float checkDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private bool isGrounded;
    private float lastGroundDistance;

    public bool IsGrounded => isGrounded;
    public float GroundDistance => lastGroundDistance;
    public LayerMask GroundLayer => groundLayer;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        if (groundLayer == 0)
            groundLayer = ~0;
    }

    public void UpdateGroundCheck()
    {
        Vector3 origin = GetFeetPosition() + Vector3.up * 0.1f;
        int mask = groundLayer & ~(1 << gameObject.layer);
        RaycastHit hitInfo;
        isGrounded = Physics.Raycast(origin, Vector3.down, out hitInfo, checkDistance, mask);
        lastGroundDistance = isGrounded ? hitInfo.distance : checkDistance;
    }

    public Vector3 GetFeetPosition()
    {
        float bottomY = rb.position.y + capsuleCollider.center.y - capsuleCollider.height * 0.5f;
        return new Vector3(rb.position.x, bottomY, rb.position.z);
    }

    public float GetGroundY()
    {
        return isGrounded ? (GetFeetPosition().y + 0.1f - lastGroundDistance) : float.MinValue;
    }

    private void OnDrawGizmosSelected()
    {
        if (rb == null || capsuleCollider == null) return;
        float bottomY = rb.position.y + capsuleCollider.center.y - capsuleCollider.height * 0.5f;
        Vector3 feetPos = new Vector3(rb.position.x, bottomY + 0.1f, rb.position.z);
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(feetPos, Vector3.down * checkDistance);
        Gizmos.DrawWireSphere(feetPos + Vector3.down * checkDistance, 0.05f);
    }
}
