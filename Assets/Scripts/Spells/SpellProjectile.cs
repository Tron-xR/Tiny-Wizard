using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private bool destroyOnHit = true;

    [Header("VFX")]
    [SerializeField] private ParticleSystem trailVFX;
    [SerializeField] private GameObject impactPrefab;

    private Vector3 velocity;
    private float pushForce;
    private float pushRadius;
    private float upwardModifier;
    private System.Action<Vector3> onHit;

    public void Launch(Vector3 launchVelocity, float force, float radius, float upward, System.Action<Vector3> hitCallback)
    {
        velocity = launchVelocity;
        pushForce = force;
        pushRadius = radius;
        upwardModifier = upward;
        onHit = hitCallback;

        if (trailVFX != null)
            trailVFX.Play();

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += velocity * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (onHit != null)
            onHit.Invoke(transform.position);

        if (impactPrefab != null)
            Instantiate(impactPrefab, transform.position, Quaternion.identity);

        if (destroyOnHit)
            Destroy(gameObject);
    }
}
