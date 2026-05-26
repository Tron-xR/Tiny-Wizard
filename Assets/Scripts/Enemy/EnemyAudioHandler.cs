using UnityEngine;

/// <summary>
/// Manages enemy audio — footsteps, attacks, detection, death, and type-specific sounds.
/// 
/// Designed to be modular: each enemy type can override or extend sounds.
/// Place this on the enemy root, and assign AudioClips in the Inspector.
/// 
/// Inspector Setup:
/// 1. Add this component to the enemy GameObject
/// 2. (Optional) Create an AudioRoot child for organization
/// 3. Assign AudioClips for each sound type
/// 4. Each enemy type (Cockroach, Spider, Fly) can use different clips
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class EnemyAudioHandler : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;       // one-shot sounds (attacks, hits, death)
    [SerializeField] private AudioSource loopSource;      // looping sounds (buzzing, skittering)

    [Header("Sound Clips")]
    [SerializeField] private AudioClip detectionSound;    // played when player is detected
    [SerializeField] private AudioClip attackSound;       // played when attacking
    [SerializeField] private AudioClip hitSound;          // played when taking damage
    [SerializeField] private AudioClip deathSound;        // played on death
    [SerializeField] private AudioClip footstepSound;     // played during movement

    [Header("Looping Sounds")]
    [SerializeField] private AudioClip idleLoopSound;     // looping ambient sound (buzz, hiss, skitter)
    [SerializeField] private AudioClip chaseLoopSound;    // looping sound while chasing

    [Header("Volume Settings")]
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.8f;
    [SerializeField] [Range(0f, 1f)] private float loopVolume = 0.5f;
    [SerializeField] [Range(0f, 3f)] private float pitchVariation = 0.1f; // slight randomization

    // References
    private EnemyBase enemyBase;

    // ===== INITIALIZATION =====

    public void Initialize(EnemyBase enemy)
    {
        enemyBase = enemy;

        // Auto-find audio sources if not assigned
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
                sfxSource = gameObject.AddComponent<AudioSource>();
        }

        if (loopSource == null)
        {
            // Create a second AudioSource for looping sounds
            loopSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure SFX source
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 1f; // 3D sound
        sfxSource.volume = sfxVolume;

        // Configure loop source
        loopSource.playOnAwake = false;
        loopSource.spatialBlend = 1f;
        loopSource.volume = loopVolume;
        loopSource.loop = true;
    }

    // ===== PUBLIC API =====

    /// <summary>
    /// Play the detection sound (player spotted).
    /// </summary>
    public void PlayDetectionSound()
    {
        PlayOneShot(detectionSound);
    }

    /// <summary>
    /// Play the attack sound.
    /// </summary>
    public void PlayAttackSound()
    {
        PlayOneShot(attackSound);
    }

    /// <summary>
    /// Play the hit sound (took damage).
    /// </summary>
    public void PlayHitSound()
    {
        PlayOneShot(hitSound);
    }

    /// <summary>
    /// Play the death sound.
    /// </summary>
    public void PlayDeathSound()
    {
        PlayOneShot(deathSound);
        StopLoopingSounds();
    }

    /// <summary>
    /// Play a footstep sound (call from animation event).
    /// </summary>
    public void PlayFootstep()
    {
        PlayOneShot(footstepSound);
    }

    /// <summary>
    /// Start the idle looping sound (e.g. fly buzzing, spider hiss).
    /// </summary>
    public void StartIdleLoop()
    {
        StartLoop(idleLoopSound);
    }

    /// <summary>
    /// Start the chase looping sound.
    /// </summary>
    public void StartChaseLoop()
    {
        if (loopSource.isPlaying && loopSource.clip == chaseLoopSound) return;

        StartLoop(chaseLoopSound);
    }

    /// <summary>
    /// Stop all looping sounds.
    /// </summary>
    public void StopLoopingSounds()
    {
        if (loopSource != null && loopSource.isPlaying)
        {
            loopSource.Stop();
        }
    }

    /// <summary>
    /// Play a custom sound clip with optional pitch variation.
    /// </summary>
    public void PlayCustomSound(AudioClip clip, float volume = 1f)
    {
        PlayOneShot(clip, volume);
    }

    /// <summary>
    /// Play a sound at a specific world position.
    /// </summary>
    public void PlaySoundAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, position, volume * sfxVolume);
    }

    // ===== PRIVATE HELPERS =====

    private void PlayOneShot(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null || sfxSource == null) return;

        // Add slight pitch variation for natural feel
        sfxSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
    }

    private void StartLoop(AudioClip clip)
    {
        if (clip == null || loopSource == null) return;

        if (loopSource.isPlaying)
            loopSource.Stop();

        loopSource.clip = clip;
        loopSource.Play();
    }
}
