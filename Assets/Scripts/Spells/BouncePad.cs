using UnityEngine;
using UnityEngine.AI;

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
        Vector3 velocity = Vector3.up * bounceHeight + launchDirection * bounceForwardForce;

        // Handle PlayerController (kinematic)
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null)
        {
            player.Launch(velocity);
            return;
        }

        // Handle NavMeshAgent enemies (kinematic Rigidbodies)
        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
        NavMeshAgent agent = other.GetComponentInParent<NavMeshAgent>();

        if (enemy != null || agent != null)
        {
            // Disable agent, apply launch via transform, re-enable after bounce
            NavMeshAgent navAgent = agent;
            if (navAgent == null) navAgent = other.GetComponentInParent<NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.enabled = false;
            }

            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.linearVelocity = velocity;
            }

            // Re-enable agent after a short delay
            if (navAgent != null && rb != null)
                StartCoroutine(ReenableAgent(navAgent, rb));

            return;
        }

        // Standard Rigidbody (non-kinematic)
        Rigidbody standardRb = other.attachedRigidbody;
        if (standardRb != null && !standardRb.isKinematic)
        {
            standardRb.linearVelocity = velocity;
            return;
        }

        // ISpellTarget interface
        ISpellTarget target = other.GetComponentInParent<ISpellTarget>();
        if (target != null)
            target.OnBounceSpell(bounceHeight);
    }

    private System.Collections.IEnumerator ReenableAgent(NavMeshAgent agent, Rigidbody rb)
    {
        yield return new WaitForSeconds(0.5f);
        if (agent != null)
        {
            agent.enabled = true;
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
            }
        }
    }
}
