using UnityEngine;

public class FreezeableObject : MonoBehaviour
{
    [Header("Freeze Behaviour")]
    [SerializeField] private bool disableMovement = true;
    [SerializeField] private bool disableRotation = true;
    [SerializeField] private Material frozenMaterial;
    [SerializeField] private GameObject iceCoverPrefab;

    private Rigidbody rb;
    private Material originalMaterial;
    private Renderer objectRenderer;
    private GameObject iceCoverInstance;
    private bool isFrozen = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
            originalMaterial = objectRenderer.material;
    }

    public void Freeze(float duration)
    {
        if (isFrozen) return;
        isFrozen = true;

        if (rb != null)
        {
            if (disableMovement)
                rb.constraints |= RigidbodyConstraints.FreezePosition;

            if (disableRotation)
                rb.constraints |= RigidbodyConstraints.FreezeRotation;
        }

        if (frozenMaterial != null && objectRenderer != null)
            objectRenderer.material = frozenMaterial;

        if (iceCoverPrefab != null)
        {
            iceCoverInstance = Instantiate(iceCoverPrefab, transform.position, transform.rotation, transform);
        }

        Invoke(nameof(Unfreeze), duration);
    }

    private void Unfreeze()
    {
        if (!isFrozen) return;
        isFrozen = false;

        if (rb != null)
        {
            if (disableMovement)
                rb.constraints &= ~RigidbodyConstraints.FreezePosition;

            if (disableRotation)
                rb.constraints &= ~RigidbodyConstraints.FreezeRotation;
        }

        if (originalMaterial != null && objectRenderer != null)
            objectRenderer.material = originalMaterial;

        if (iceCoverInstance != null)
            Destroy(iceCoverInstance);
    }

    public bool IsFrozen => isFrozen;
}
