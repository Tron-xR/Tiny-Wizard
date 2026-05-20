using UnityEngine;

public class AttackSpell : SpellBase
{
    [Header("Damage")]
    [SerializeField] private float damageAmount = 30f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackRadius = 2f;
    [SerializeField] private float upwardModifier = 0.3f;

    [Header("Projectile")]
    [SerializeField] private SpellProjectile projectilePrefab;
    [SerializeField] private float projectileSpeed = 25f;

    [Header("Layer")]
    [SerializeField] private LayerMask targetLayers = -1;

    private Vector3 castOrigin;
    private Vector3 castDirection;

    protected override void StartCast(Vector3 origin, Vector3 direction)
    {
        castOrigin = origin;
        castDirection = direction;
        base.StartCast(origin, direction);
    }

    protected override void ExecuteCast()
    {
        if (projectilePrefab == null) return;

        SpellProjectile proj = Instantiate(projectilePrefab, castOrigin, Quaternion.LookRotation(castDirection));
        proj.Launch(castDirection * projectileSpeed, 0, 0, 0, OnProjectileHit);
    }

    private void OnProjectileHit(Vector3 hitPoint)
    {
        PlayImpactVFX(hitPoint);
        PlayImpactSFX(hitPoint);
        ApplyHitAt(hitPoint);
    }

    private void ApplyHitAt(Vector3 center)
    {
        Collider[] hits = Physics.OverlapSphere(center, knockbackRadius, targetLayers);

        foreach (Collider hit in hits)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 forceDir = (hit.transform.position - center).normalized + Vector3.up * upwardModifier;
                rb.AddForce(forceDir * knockbackForce, ForceMode.Impulse);
            }

            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(damageAmount, center, Vector3.up);

            ISpellTarget target = hit.GetComponentInParent<ISpellTarget>();
            if (target != null)
            {
                Vector3 forceDir = (hit.transform.position - center).normalized + Vector3.up * upwardModifier;
                target.OnPushSpell(forceDir, knockbackForce);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, knockbackRadius);
    }
}
