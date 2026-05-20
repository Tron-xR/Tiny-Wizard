using UnityEngine;

public abstract class SpellBase : MonoBehaviour
{
    [Header("Spell Settings")]
    [SerializeField] protected string spellName = "Spell";
    [SerializeField] protected float cooldownDuration = 1f;
    [SerializeField] protected float manaCost = 10f;
    [SerializeField] protected float castRange = 10f;
    [SerializeField] protected float castDelay = 0.2f;

    [Header("Animation")]
    [SerializeField] protected string castTrigger = "CastTrigger";
    [SerializeField] protected int spellTypeIndex = 0;

    [Header("VFX References")]
    [SerializeField] protected ParticleSystem castVFX;
    [SerializeField] protected ParticleSystem impactVFX;
    [SerializeField] protected AudioClip castSFX;
    [SerializeField] protected AudioClip impactSFX;

    [Header("Audio")]
    [SerializeField] protected AudioSource audioSource;

    // Runtime state
    protected SpellManager spellManager;
    protected float lastCastTime = -Mathf.Infinity;

    public string SpellName => spellName;
    public float CooldownDuration => cooldownDuration;
    public float ManaCost => manaCost;
    public int SpellTypeIndex => spellTypeIndex;
    public bool IsOnCooldown => Time.time < lastCastTime + cooldownDuration;
    public float RemainingCooldown => Mathf.Max(0f, (lastCastTime + cooldownDuration) - Time.time);
    public float CooldownProgress => cooldownDuration > 0f ? Mathf.Clamp01((Time.time - lastCastTime) / cooldownDuration) : 1f;

    public virtual void Initialize(SpellManager manager)
    {
        spellManager = manager;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public bool TryCast(Vector3 origin, Vector3 direction)
    {
        if (IsOnCooldown)
            return false;

        if (spellManager != null && spellManager.HasMana && spellManager.CurrentMana < manaCost)
            return false;

        StartCast(origin, direction);
        return true;
    }

    protected virtual void StartCast(Vector3 origin, Vector3 direction)
    {
        lastCastTime = Time.time;

        PlayCastVFX(origin);
        PlayCastSFX();

        if (spellManager != null)
        {
            spellManager.UseMana(manaCost);
            spellManager.TriggerCastAnimation(spellTypeIndex);
        }

        Invoke(nameof(ExecuteCast), castDelay);
    }

    protected abstract void ExecuteCast();

    protected virtual void PlayCastVFX(Vector3 position)
    {
        if (castVFX != null)
            Instantiate(castVFX, position, Quaternion.identity);
    }

    protected virtual void PlayImpactVFX(Vector3 position)
    {
        if (impactVFX != null)
            Instantiate(impactVFX, position, Quaternion.identity);
    }

    protected virtual void PlayCastSFX()
    {
        if (audioSource != null && castSFX != null)
            audioSource.PlayOneShot(castSFX);
    }

    protected virtual void PlayImpactSFX(Vector3 position)
    {
        if (audioSource != null && impactSFX != null)
            audioSource.PlayOneShot(impactSFX);
    }
}
