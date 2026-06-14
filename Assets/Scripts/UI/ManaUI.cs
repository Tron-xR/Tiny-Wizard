using UnityEngine;
using UnityEngine.UI;

public class ManaUI : MonoBehaviour
{
    [Header("UI References")]
    public Image manaFill;
    public Text manaText;
    [SerializeField] private Image spellIcon;
    [SerializeField] private Text spellNameText;

    [Header("Settings")]
    [SerializeField] private Color manaColor = Color.blue;
    [SerializeField] private float fillSmoothSpeed = 5f;

    private SpellManager spellManager;
    private float targetFill = 1f;

    private void OnEnable()
    {
        if (spellManager == null)
            spellManager = FindFirstObjectByType<SpellManager>();

        if (spellManager == null)
        {
            Debug.LogWarning("ManaUI: No SpellManager found in scene.");
            return;
        }

        if (manaFill != null)
            manaFill.color = manaColor;

        spellManager.OnManaChanged += UpdateMana;
        spellManager.OnSpellSwitched += UpdateSpellInfo;

        UpdateMana(spellManager.CurrentMana);
        UpdateSpellInfo(spellManager.ActiveSpellIndex);
    }

    private void OnDisable()
    {
        if (spellManager != null)
        {
            spellManager.OnManaChanged -= UpdateMana;
            spellManager.OnSpellSwitched -= UpdateSpellInfo;
        }
    }

    private void Update()
    {
        if (manaFill != null)
            manaFill.fillAmount = Mathf.Lerp(manaFill.fillAmount, targetFill, fillSmoothSpeed * Time.deltaTime);
    }

    private void UpdateMana(float current)
    {
        if (spellManager == null) return;
        float percent = spellManager.MaxMana > 0f ? current / spellManager.MaxMana : 0f;
        targetFill = percent;

        if (manaText != null)
            manaText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(spellManager.MaxMana)}";
    }

    private void UpdateSpellInfo(int index)
    {
        if (spellManager == null) return;
        SpellBase spell = spellManager.ActiveSpell;
        if (spell == null) return;

        if (spellNameText != null)
            spellNameText.text = spell.name;
    }
}
