using UnityEngine;
using System.Collections.Generic;

public class SpellManager : MonoBehaviour
{
    [Header("Spell List")]
    [SerializeField] private List<SpellBase> spells = new List<SpellBase>();

    [Header("Mana System")]
    [SerializeField] private bool useMana = false;
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float manaRegenRate = 10f;

    [Header("References")]
    [SerializeField] private Transform castOrigin;
    [SerializeField] private Animator playerAnimator;

    [Header("Input")]
    [SerializeField] private PlayerInputHandler inputHandler;

    [Header("Events")]
    public System.Action<int> OnSpellSwitched;
    public System.Action<float> OnManaChanged;
    public System.Action<float> OnCooldownUpdated;

    private int activeSpellIndex = 0;
    private float currentMana;
    private float lastScrollValue = 0f;

    public SpellBase ActiveSpell => spells.Count > 0 ? spells[activeSpellIndex] : null;
    public int ActiveSpellIndex => activeSpellIndex;
    public int SpellCount => spells.Count;
    public float CurrentMana => currentMana;
    public float MaxMana => maxMana;
    public bool HasMana => currentMana > 0f;

    private void OnEnable()
    {
        if (inputHandler == null)
            inputHandler = GetComponent<PlayerInputHandler>();

        if (inputHandler != null)
        {
            inputHandler.CastSpellPressed += OnCastSpellPressed;
            inputHandler.SpellSlotPressed += OnSpellSlotPressed;
        }

        currentMana = maxMana;

        InitializeSpells();
    }

    private void OnDisable()
    {
        if (inputHandler != null)
        {
            inputHandler.CastSpellPressed -= OnCastSpellPressed;
            inputHandler.SpellSlotPressed -= OnSpellSlotPressed;
        }
    }

    private void InitializeSpells()
    {
        foreach (SpellBase spell in spells)
        {
            if (spell != null)
                spell.Initialize(this);
        }
    }

    private void Update()
    {
        HandleSpellSwitching();
        RegenMana();
    }

    private void HandleSpellSwitching()
    {
        if (spells.Count <= 1) return;

        if (inputHandler != null)
        {
            float scroll = inputHandler.ZoomInput;
            if (Mathf.Abs(scroll - lastScrollValue) > 0.01f)
            {
                if (scroll > lastScrollValue)
                    SwitchToNext();
                else if (scroll < lastScrollValue)
                    SwitchToPrevious();
            }
            lastScrollValue = scroll;
        }
    }

    private void SwitchToNext()
    {
        activeSpellIndex = (activeSpellIndex + 1) % spells.Count;
        OnSpellSwitched?.Invoke(activeSpellIndex);
    }

    private void SwitchToPrevious()
    {
        activeSpellIndex = (activeSpellIndex - 1 + spells.Count) % spells.Count;
        OnSpellSwitched?.Invoke(activeSpellIndex);
    }

    public void SwitchToSpell(int index)
    {
        if (index < 0 || index >= spells.Count) return;
        activeSpellIndex = index;
        OnSpellSwitched?.Invoke(activeSpellIndex);
    }

    private void OnSpellSlotPressed(int slotIndex)
    {
        SwitchToSpell(slotIndex);
    }

    private void OnCastSpellPressed()
    {
        if (ActiveSpell == null) return;

        Vector3 origin = castOrigin != null ? castOrigin.position : transform.position;
        Vector3 direction = GetCastDirection();

        ActiveSpell.TryCast(origin, direction);
    }

    private Vector3 GetCastDirection()
    {
        Camera cam = Camera.main;
        if (cam != null)
            return cam.transform.forward;

        return transform.forward;
    }

    public void TriggerCastAnimation(int spellTypeIndex)
    {
        if (playerAnimator == null) return;

        playerAnimator.SetInteger("SpellType", spellTypeIndex);
        playerAnimator.SetTrigger("CastTrigger");
    }

    private void RegenMana()
    {
        if (!useMana) return;

        currentMana = Mathf.Min(maxMana, currentMana + manaRegenRate * Time.deltaTime);
    }

    public void UseMana(float amount)
    {
        if (!useMana) return;

        currentMana = Mathf.Max(0f, currentMana - amount);
        OnManaChanged?.Invoke(currentMana);
    }
}
