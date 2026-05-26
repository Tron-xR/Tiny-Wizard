using UnityEngine;

/// <summary>
/// Handles enemy health, damage, and death.
/// 
/// Features:
/// - Configurable max health
/// - Damage with hit flash effect
/// - Death event with destroy delay
/// - Optional ragdoll hooks
/// - Implements IDamageable for spell/weapon compatibility
/// 
/// Inspector Setup:
/// 1. Add this component to the enemy GameObject
/// 2. Set max health
/// 3. (Optional) Assign Renderer for hit flash
/// 4. (Optional) Assign death VFX prefab
/// </summary>
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float destroyDelay = 5f; // time before GameObject is destroyed after death

    [Header("Hit Flash")]
    [SerializeField] private bool enableHitFlash = true;
    [SerializeField] private Renderer enemyRenderer; // the main renderer for hit flash
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;

    [Header("Death VFX")]
    [SerializeField] private GameObject deathVFXPrefab; // optional: particle system prefab

    // Events
    public System.Action OnDamageTaken;
    public System.Action OnDeath;

    // References
    private EnemyBase enemyBase;
    private Transform cachedTransform;

    // Runtime state
    private float currentHealth;
    private bool isDead = false;

    // Hit flash
    private Color originalColor;
    private Material materialInstance; // instance to avoid modifying shared material
    private float flashTimer = 0f;
    private bool isFlashing = false;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercent => maxHealth > 0f ? currentHealth / maxHealth : 0f;
    public bool IsDead => isDead;

    // ===== INITIALIZATION =====

    public void Initialize(EnemyBase enemy)
    {
        enemyBase = enemy;
        cachedTransform = transform;

        currentHealth = maxHealth;

        // Set up hit flash material
        if (enableHitFlash && enemyRenderer != null)
        {
            // Create a material instance so we don't modify the shared asset
            materialInstance = enemyRenderer.material;
            originalColor = materialInstance.color;
        }
    }

    // ===== PUBLIC API =====

    /// <summary>
    /// Apply damage to this enemy (hit point version — implements IDamageable).
    /// </summary>
    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (isDead) return;
        if (amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);

        // Trigger hit flash
        if (enableHitFlash && materialInstance != null)
        {
            StartHitFlash();
        }

        OnDamageTaken?.Invoke();

        // Check for death
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Heal the enemy by the given amount.
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    // ===== PRIVATE METHODS =====

    private void StartHitFlash()
    {
        isFlashing = true;
        flashTimer = hitFlashDuration;
        materialInstance.color = hitFlashColor;
    }

    private void EndHitFlash()
    {
        isFlashing = false;
        materialInstance.color = originalColor;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // End any hit flash
        if (isFlashing)
            EndHitFlash();

        // Spawn death VFX
        if (deathVFXPrefab != null)
        {
            Instantiate(deathVFXPrefab, cachedTransform.position, Quaternion.identity);
        }

        // Fire death event
        OnDeath?.Invoke();

        // Destroy after delay
        Destroy(gameObject, destroyDelay);
    }

    // ===== UPDATE =====

    private void Update()
    {
        // Handle hit flash timer
        if (isFlashing)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
            {
                EndHitFlash();
            }
        }
    }

    // ===== GIZMOS =====

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            // Show health bar info
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
                $"HP: {currentHealth}/{maxHealth}");
        }
    }
}
