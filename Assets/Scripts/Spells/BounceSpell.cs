using UnityEngine;

public class BounceSpell : SpellBase
{
    [Header("Bounce Settings")]
    [SerializeField] private float bounceHeight = 8f;
    [SerializeField] private float bounceForwardForce = 5f;

    [Header("Bounce Pad")]
    [SerializeField] private GameObject bouncePadPrefab;
    [SerializeField] private float padDuration = 5f;

    [Header("Layer")]
    [SerializeField] private LayerMask groundLayers = -1;

    private Vector3 castDirection;

    protected override void StartCast(Vector3 origin, Vector3 direction)
    {
        castDirection = direction;
        base.StartCast(origin, direction);
    }

    protected override void ExecuteCast()
    {
        Vector3 targetPoint = FindGroundTarget();
        if (targetPoint != Vector3.zero)
        {
            SpawnBouncePad(targetPoint, Vector3.up);
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

        if (Physics.Raycast(ray, out hit, castRange, groundLayers))
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

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 20f, groundLayers))
            return hit.point;

        return transform.position - Vector3.up * 2f;
    }

    private void SpawnBouncePad(Vector3 position, Vector3 normal)
    {
        if (bouncePadPrefab == null) return;

        float halfHeight = bouncePadPrefab.transform.localScale.y * 0.5f;
        Vector3 spawnPos = position + normal * (halfHeight + 0.01f);

        GameObject pad = Instantiate(bouncePadPrefab, spawnPos, Quaternion.FromToRotation(Vector3.up, normal));

        BouncePad bp = pad.GetComponent<BouncePad>();
        if (bp != null)
            bp.Initialize(new Vector3(castDirection.x, 0, castDirection.z).normalized, bounceHeight, bounceForwardForce);

        Destroy(pad, padDuration);
    }
}
