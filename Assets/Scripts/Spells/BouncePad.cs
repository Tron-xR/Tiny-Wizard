using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [Header("Bounce")]
    [SerializeField] private float bounceHeight = 8f;
    [SerializeField] private float bounceForwardForce = 5f;

    private Vector3 launchDirection;

    public void Initialize(Vector3 forwardDir, float height, float forwardForce)
    {
        launchDirection = forwardDir;
        bounceHeight = height;
        bounceForwardForce = forwardForce;
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            Vector3 velocity = Vector3.up * bounceHeight + launchDirection * bounceForwardForce;
            rb.linearVelocity = velocity;
        }

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null)
            player.Launch(Vector3.up * bounceHeight + launchDirection * bounceForwardForce);

        ISpellTarget target = other.GetComponentInParent<ISpellTarget>();
        if (target != null)
            target.OnBounceSpell(bounceHeight);
    }
}
