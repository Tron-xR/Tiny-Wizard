using UnityEngine;
using UnityEngine.InputSystem;

public class AttackSpell : SpellBase
{
    [Header("Damage")]
    [SerializeField] private float damageAmount = 30f;
    [SerializeField] private float hitRadius = 2f;

    [Header("Projectile")]
    [SerializeField] private SpellProjectile projectilePrefab;
    [SerializeField] private float projectileSpeed = 25f;

    [Header("Layer")]
    [SerializeField] private LayerMask targetLayers = -1;

    protected override void ExecuteCast()
    {
        if (projectilePrefab == null) return;
        if (spellManager == null) return;

        // Recalculate origin and direction at execution time (not cached from StartCast)
        // so the projectile fires where the player/camera is aiming right now.
        Transform originTransform = spellManager.CastOrigin;
        Vector3 origin = originTransform != null ? originTransform.position : spellManager.transform.position;
        Vector3 direction = GetCastDirection();

        SpellProjectile proj = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(direction));
        proj.Launch(direction * projectileSpeed, 0, 0, 0, OnProjectileHit);
    }

    private Vector3 GetCastDirection()
    {
        Camera cam = Camera.main;
        if (cam == null) return transform.forward;

        // Cast a ray from the camera through the mouse cursor position
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);
        Vector3 origin = spellManager.CastOrigin != null
            ? spellManager.CastOrigin.position
            : spellManager.transform.position;

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, targetLayers))
        {
            // Fire toward the world point under the cursor
            return (hit.point - origin).normalized;
        }

        // If nothing under the cursor, fire along the screen-center direction
        return cam.transform.forward;
    }

    private void OnProjectileHit(Vector3 hitPoint)
    {
        PlayImpactVFX(hitPoint);
        PlayImpactSFX(hitPoint);
        ApplyHitAt(hitPoint);
    }

    private void ApplyHitAt(Vector3 center)
    {
        Collider[] hits = Physics.OverlapSphere(center, hitRadius, targetLayers);

        foreach (Collider hit in hits)
        {
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(damageAmount, center, Vector3.up);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
