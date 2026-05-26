using UnityEngine;

/// <summary>
/// Handles melee attack logic for enemies.
///
/// Uses OverlapSphere at the AttackPoint to detect targets.
/// Supports attack cooldown, animation event hooks, and damage dealing.
///
/// Inspector Setup:
/// 1. Add this component to the enemy GameObject
/// 2. Set attack range, damage, and cooldown
/// 3. (Optional) Create an AttackPoint child Transform where the attack originates
/// 4. Assign the target layers (should include the Player layer)
///
/// Animation Events:
/// - Call "OnAttackHit()" from your attack animation to trigger hit detection
/// - Call "OnAttackFinished()" from your attack animation to end the attack state
/// </summary>
public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackRadius = 0.5f; // OverlapSphere radius
    [SerializeField] private LayerMask targetLayers = -1;

    [Header("Attack Point")]
    [SerializeField] private Transform attackPoint; // where to center the hit detection
    [SerializeField] private Vector3 attackPointOffset = Vector3.zero;

    [Header("Knockback")]
    [SerializeField] private bool applyKnockback = true;
    [SerializeField] private float knockbackForce = 5f;

    [Header("Debug")]
    [SerializeField] private bool showAttackGizmo = true;

    // Events
    public System.Action OnAttack; // called when attack starts
    public System.Action<GameObject> OnAttackHitEvent; // called when attack connects

    // References
    private EnemyBase enemyBase;
    private Transform cachedTransform;

    // Runtime state
    private float lastAttackTime = -Mathf.Infinity;
    private bool isAttacking = false;
    private bool hasHitThisAttack = false;

    public float AttackRange => attackRange;
    public float AttackDamage => attackDamage;
    public bool CanAttack => Time.time >= lastAttackTime + attackCooldown;
    public bool IsAttacking => isAttacking;

    // ===== INITIALIZATION =====

    public void Initialize(EnemyBase enemy)
    {
        enemyBase = enemy;
        cachedTransform = transform;
    }

    // ===== PUBLIC API =====

    /// <summary>
    /// Attempt to attack the target. Returns true if attack was initiated.
    /// Call every frame from EnemyBase's UpdateAttack().
    /// </summary>
    public bool TryAttack(Transform target, Vector3 origin)
    {
        if (target == null) return false;
        if (!CanAttack) return false;
        if (isAttacking) return false;

        // Check distance
        float dist = Vector3.Distance(origin, target.position);
        if (dist > attackRange) return false;

        StartAttack();
        return true;
    }

    /// <summary>
    /// Called by animation event at the moment of impact.
    /// </summary>
    public void OnAttackHit()
    {
        if (!isAttacking) return;
        if (hasHitThisAttack) return; // only hit once per attack

        hasHitThisAttack = true;

        // Get the attack position
        Vector3 attackPos = GetAttackPosition();

        // Detect targets in range
        Collider[] hits = Physics.OverlapSphere(attackPos, attackRadius, targetLayers);

        foreach (Collider hit in hits)
        {
            // Don't hit self
            if (hit.transform.root == cachedTransform.root) continue;

            // Try to damage via IDamageable
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage, hit.ClosestPoint(attackPos), cachedTransform.forward);
            }

            // Apply knockback to Rigidbody
            if (applyKnockback)
            {
                Rigidbody rb = hit.GetComponentInParent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 knockDir = (hit.transform.position - attackPos).normalized;
                    knockDir.y = 0.3f; // slight upward
                    rb.AddForce(knockDir * knockbackForce, ForceMode.Impulse);
                }
            }

            // Fire hit event
            OnAttackHitEvent?.Invoke(hit.gameObject);
        }
    }

    /// <summary>
    /// Called by animation event when the attack animation finishes.
    /// </summary>
    public void OnAttackFinished()
    {
        EndAttack();
    }

    // ===== PRIVATE METHODS =====

    private void StartAttack()
    {
        isAttacking = true;
        hasHitThisAttack = false;
        lastAttackTime = Time.time;

        OnAttack?.Invoke();
    }

    private void EndAttack()
    {
        isAttacking = false;
        hasHitThisAttack = false;
    }

    private Vector3 GetAttackPosition()
    {
        if (attackPoint != null)
            return attackPoint.position;

        return cachedTransform.position + cachedTransform.forward * attackRange * 0.5f + attackPointOffset;
    }

    // ===== GIZMOS =====

    private void OnDrawGizmosSelected()
    {
        if (!showAttackGizmo) return;

        // Attack range circle
        Gizmos.color = Color.red;
        Vector3 attackPos = Application.isPlaying ? GetAttackPosition() : 
            (attackPoint != null ? attackPoint.position : transform.position + transform.forward * attackRange * 0.5f + attackPointOffset);

        Gizmos.DrawWireSphere(attackPos, attackRadius);

        // Attack range line
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawRay(transform.position, transform.forward * attackRange);
    }
}
