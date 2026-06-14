using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float invincibilityTime = 1f;

    [Header("Hit Flash")]
    [SerializeField] private bool enableHitFlash = true;
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;

    [Header("Death")]
    [SerializeField] private float respawnDelay = 3f;

    public System.Action<float, float> OnHealthChanged;
    public System.Action OnPlayerDeath;
    public System.Action OnPlayerRespawn;

    private float currentHealth;
    private bool isDead = false;
    private float invincibleTimer = 0f;

    private Color originalColor;
    private Material materialInstance;
    private float flashTimer = 0f;
    private bool isFlashing = false;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercent => maxHealth > 0f ? currentHealth / maxHealth : 0f;
    public bool IsDead => isDead;
    public bool IsInvincible => invincibleTimer > 0f;

    private void OnEnable()
    {
        currentHealth = maxHealth;

        if (playerRenderer == null)
            playerRenderer = GetComponentInChildren<Renderer>();

        if (enableHitFlash && playerRenderer != null)
        {
            materialInstance = playerRenderer.material;
            originalColor = materialInstance.color;
        }
    }

    private void Update()
    {
        if (invincibleTimer > 0f)
            invincibleTimer -= Time.deltaTime;

        if (isFlashing)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
                EndHitFlash();
        }
    }

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (isDead || invincibleTimer > 0f || amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);

        invincibleTimer = invincibilityTime;

        if (enableHitFlash && materialInstance != null)
            StartHitFlash();

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        invincibleTimer = invincibilityTime;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnPlayerRespawn?.Invoke();
    }

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

        if (isFlashing)
            EndHitFlash();

        OnPlayerDeath?.Invoke();
        Invoke(nameof(Respawn), respawnDelay);
    }
}
