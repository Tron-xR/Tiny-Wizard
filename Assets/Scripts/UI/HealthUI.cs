using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image healthFill;
    [SerializeField] private Text healthText;
    [SerializeField] private GameObject deathOverlay;

    [Header("Settings")]
    [SerializeField] private Color highHealthColor = Color.green;
    [SerializeField] private Color midHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private float midHealthThreshold = 0.6f;

    [Header("Animation")]
    [SerializeField] private float fillSmoothSpeed = 5f;

    private PlayerHealth playerHealth;
    private float targetFill = 1f;

    private void OnEnable()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogWarning("HealthUI: No PlayerHealth found in scene.");
            return;
        }

        playerHealth.OnHealthChanged += UpdateHealthDisplay;
        playerHealth.OnPlayerDeath += ShowDeathOverlay;
        playerHealth.OnPlayerRespawn += HideDeathOverlay;

        UpdateHealthDisplay(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthDisplay;
            playerHealth.OnPlayerDeath -= ShowDeathOverlay;
            playerHealth.OnPlayerRespawn -= HideDeathOverlay;
        }
    }

    private void Update()
    {
        if (healthFill != null)
            healthFill.fillAmount = Mathf.Lerp(healthFill.fillAmount, targetFill, fillSmoothSpeed * Time.deltaTime);
    }

    private void UpdateHealthDisplay(float current, float max)
    {
        float percent = max > 0f ? current / max : 0f;
        targetFill = percent;

        if (healthFill != null)
        {
            if (percent <= lowHealthThreshold)
                healthFill.color = lowHealthColor;
            else if (percent <= midHealthThreshold)
                healthFill.color = midHealthColor;
            else
                healthFill.color = highHealthColor;
        }

        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    private void ShowDeathOverlay()
    {
        if (deathOverlay != null)
            deathOverlay.SetActive(true);
    }

    private void HideDeathOverlay()
    {
        if (deathOverlay != null)
            deathOverlay.SetActive(false);
    }
}
