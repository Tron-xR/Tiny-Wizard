using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum CollectibleType { HealthOrb, ManaOrb, Key }

    [Header("Type")]
    [SerializeField] private CollectibleType type = CollectibleType.HealthOrb;
    [SerializeField] private int value = 20;

    [Header("VFX")]
    [SerializeField] private GameObject pickupVFXPrefab;
    [SerializeField] private AudioClip pickupSFX;

    [Header("Rotation")]
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    private Vector3 basePosition;
    private bool collected = false;

    private void OnEnable()
    {
        basePosition = transform.position;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = basePosition + Vector3.up * yOffset;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        switch (type)
        {
            case CollectibleType.HealthOrb:
                PlayerHealth health = other.GetComponent<PlayerHealth>();
                if (health != null)
                    health.Heal(value);
                break;

            case CollectibleType.ManaOrb:
                SpellManager spells = FindFirstObjectByType<SpellManager>();
                if (spells != null)
                    spells.UseMana(-value);
                break;

            case CollectibleType.Key:
                Debug.Log("Key collected!");
                break;
        }

        if (pickupVFXPrefab != null)
            Instantiate(pickupVFXPrefab, transform.position, Quaternion.identity);

        if (pickupSFX != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(pickupSFX, Camera.main.transform.position);

        Destroy(gameObject);
    }
}
