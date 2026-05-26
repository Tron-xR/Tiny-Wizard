using UnityEngine;

/// <summary>
/// Manages enemy visual effects — hit sparks, death particles, attack effects, etc.
/// 
/// Each effect is a ParticleSystem reference that can be assigned in the Inspector.
/// Effects are instantiated at runtime (not pooled, for game-jam scope).
/// 
/// Inspector Setup:
/// 1. Add this component to the enemy GameObject
/// 2. Assign ParticleSystem prefabs for each effect
/// 3. (Optional) Create a VFXRoot child Transform for organization
/// </summary>
public class EnemyVFXHandler : MonoBehaviour
{
    [Header("VFX Prefabs")]
    [SerializeField] private ParticleSystem hitVFXPrefab;        // sparks / impact on hit
    [SerializeField] private ParticleSystem deathVFXPrefab;      // explosion / dissolve on death
    [SerializeField] private ParticleSystem attackVFXPrefab;     // attack impact effect
    [SerializeField] private ParticleSystem detectionVFXPrefab;  // ! effect on detection

    [Header("VFX Settings")]
    [SerializeField] private float effectLifetime = 2f; // auto-destroy after this time
    [SerializeField] private Transform vfxRoot;          // optional parent for spawned effects

    // ===== INITIALIZATION =====

    public void Initialize(EnemyBase enemy)
    {
        // If no VFX root is assigned, create one
        if (vfxRoot == null)
        {
            GameObject root = new GameObject("VFXRoot");
            root.transform.SetParent(transform);
            root.transform.localPosition = Vector3.zero;
            vfxRoot = root.transform;
        }
    }

    // ===== PUBLIC API =====

    /// <summary>
    /// Play hit visual effects at the given position and normal.
    /// </summary>
    public void PlayHitVFX(Vector3 position, Vector3 normal)
    {
        if (hitVFXPrefab != null)
        {
            Quaternion rotation = Quaternion.LookRotation(normal);
            ParticleSystem vfx = Instantiate(hitVFXPrefab, position, rotation, vfxRoot);
            Destroy(vfx.gameObject, effectLifetime);
        }
    }

    /// <summary>
    /// Play death visual effects at the given position.
    /// </summary>
    public void PlayDeathVFX(Vector3 position)
    {
        if (deathVFXPrefab != null)
        {
            ParticleSystem vfx = Instantiate(deathVFXPrefab, position, Quaternion.identity, vfxRoot);
            Destroy(vfx.gameObject, effectLifetime);
        }
    }

    /// <summary>
    /// Play attack visual effects at the given position.
    /// </summary>
    public void PlayAttackVFX(Vector3 position)
    {
        if (attackVFXPrefab != null)
        {
            ParticleSystem vfx = Instantiate(attackVFXPrefab, position, Quaternion.identity, vfxRoot);
            Destroy(vfx.gameObject, effectLifetime);
        }
    }

    /// <summary>
    /// Play detection visual effect (e.g. exclamation mark).
    /// </summary>
    public void PlayDetectionVFX(Vector3 position)
    {
        if (detectionVFXPrefab != null)
        {
            ParticleSystem vfx = Instantiate(detectionVFXPrefab, position, Quaternion.identity, vfxRoot);
            Destroy(vfx.gameObject, effectLifetime);
        }
    }

    /// <summary>
    /// Play a custom VFX at a given position with optional parent.
    /// </summary>
    public void PlayCustomVFX(ParticleSystem prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return;

        ParticleSystem vfx = Instantiate(prefab, position, rotation, vfxRoot);
        Destroy(vfx.gameObject, effectLifetime);
    }
}
