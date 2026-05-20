using UnityEngine;

public class FreezeSpell : SpellBase
{
    [Header("Freeze Settings")]
    [SerializeField] private float freezeDuration = 5f;
    [SerializeField] private float freezeRadius = 4f;
    [SerializeField] private bool createIcePlatform = true;
    [SerializeField] private GameObject icePlatformPrefab;
    [SerializeField] private float platformDuration = 8f;

    [Header("Layer")]
    [SerializeField] private LayerMask targetLayers = -1;

    protected override void ExecuteCast()
    {
        Vector3 targetPoint = FindGroundTarget();
        if (targetPoint != Vector3.zero)
        {
            ApplyFreezeAt(targetPoint);
            CreateIcePlatform(targetPoint);
        }

        PlayImpactVFX(targetPoint);
        PlayImpactSFX(targetPoint);
    }

    private Vector3 FindGroundTarget()
    {
        Camera cam = Camera.main;
        if (cam == null) return transform.position - Vector3.up * 2f;

        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, castRange, targetLayers))
        {
            if (hit.collider.transform.root != transform.root)
                return hit.point;
        }

        if (ray.direction.y < 0f)
        {
            float t = -ray.origin.y / ray.direction.y;
            if (t > 0f)
                return ray.GetPoint(t);
        }

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 20f, targetLayers))
            return hit.point;

        return transform.position - Vector3.up * 2f;
    }

    private void ApplyFreezeAt(Vector3 center)
    {
        Collider[] hits = Physics.OverlapSphere(center, freezeRadius, targetLayers);

        foreach (Collider hit in hits)
        {
            ISpellTarget target = hit.GetComponentInParent<ISpellTarget>();
            if (target != null)
                target.OnFreezeSpell(freezeDuration);

            FreezeableObject freezeable = hit.GetComponentInParent<FreezeableObject>();
            if (freezeable != null)
                freezeable.Freeze(freezeDuration);
        }
    }

    private void CreateIcePlatform(Vector3 position)
    {
        if (!createIcePlatform || icePlatformPrefab == null)
            return;

        float halfHeight = icePlatformPrefab.transform.localScale.y * 0.5f;
        Vector3 spawnPos = position + Vector3.up * (halfHeight + 0.01f);

        GameObject platform = Instantiate(icePlatformPrefab, spawnPos, Quaternion.identity);

        Destroy(platform, platformDuration);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, freezeRadius);
    }
}
