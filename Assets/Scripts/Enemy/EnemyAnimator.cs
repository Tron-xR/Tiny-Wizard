using UnityEngine;

/// <summary>
/// Manages enemy Animator parameters.
/// 
/// Provides clean methods to set animation states without directly
/// accessing Animator in the enemy scripts.
/// 
/// Supported parameters:
/// - Speed (float)      : movement speed for blend trees
/// - IsChasing (bool)   : is the enemy chasing the player
/// - AttackTrigger      : triggers attack animation
/// - HitTrigger         : triggers hit reaction
/// - IsDead (bool)      : death state
/// 
/// Inspector Setup:
/// 1. Add this component to the enemy GameObject
/// 2. The Animator is auto-found from children during Initialize()
/// </summary>
public class EnemyAnimator : MonoBehaviour
{
    [Header("Animator Parameters")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string isChasingParam = "IsChasing";
    [SerializeField] private string attackTriggerParam = "AttackTrigger";
    [SerializeField] private string hitTriggerParam = "HitTrigger";
    [SerializeField] private string isDeadParam = "IsDead";

    [Header("Animation Smoothing")]
    [SerializeField] private float speedSmoothTime = 0.1f;

    // References
    private Animator animator;

    // Runtime state
    private float currentSpeed = 0f;
    private float speedVelocity = 0f;

    // ===== INITIALIZATION =====

    public void Initialize(Animator enemyAnimator)
    {
        animator = enemyAnimator;
    }

    // ===== PUBLIC API =====

    /// <summary>
    /// Update animation parameters. Call every frame from EnemyBase.
    /// </summary>
    public void UpdateAnimations(EnemyStateMachine.EnemyState state, float moveSpeed)
    {
        if (animator == null) return;

        // Smooth the speed parameter for blend trees
        currentSpeed = Mathf.SmoothDamp(currentSpeed, moveSpeed, ref speedVelocity, speedSmoothTime);

        animator.SetFloat(speedParam, currentSpeed);

        // Update state-based bools
        bool isChasing = state == EnemyStateMachine.EnemyState.Chase;
        animator.SetBool(isChasingParam, isChasing);
    }

    /// <summary>
    /// Set idle animation state.
    /// </summary>
    public void SetIdle()
    {
        if (animator == null) return;

        animator.SetFloat(speedParam, 0f);
        animator.SetBool(isChasingParam, false);
    }

    /// <summary>
    /// Set movement speed directly (useful for spider crawling).
    /// </summary>
    public void SetSpeed(float speed)
    {
        if (animator == null) return;

        currentSpeed = speed;
        animator.SetFloat(speedParam, speed);
    }

    /// <summary>
    /// Set the chasing bool on the animator.
    /// </summary>
    public void SetChasing(bool chasing)
    {
        if (animator == null) return;

        animator.SetBool(isChasingParam, chasing);
    }

    /// <summary>
    /// Trigger the attack animation.
    /// </summary>
    public void TriggerAttack()
    {
        if (animator == null) return;

        animator.SetTrigger(attackTriggerParam);
    }

    /// <summary>
    /// Trigger the hit reaction animation.
    /// </summary>
    public void TriggerHit()
    {
        if (animator == null) return;

        animator.SetTrigger(hitTriggerParam);
    }

    /// <summary>
    /// Set the dead animation state.
    /// </summary>
    public void SetDead()
    {
        if (animator == null) return;

        animator.SetBool(isDeadParam, true);
        animator.SetFloat(speedParam, 0f);
    }

    /// <summary>
    /// Set a custom float parameter on the animator.
    /// Useful for enemy-specific parameters.
    /// </summary>
    public void SetFloat(string paramName, float value)
    {
        if (animator == null) return;

        animator.SetFloat(paramName, value);
    }

    /// <summary>
    /// Set a custom bool parameter on the animator.
    /// </summary>
    public void SetBool(string paramName, bool value)
    {
        if (animator == null) return;

        animator.SetBool(paramName, value);
    }

    /// <summary>
    /// Set a custom trigger parameter on the animator.
    /// </summary>
    public void SetTrigger(string paramName)
    {
        if (animator == null) return;

        animator.SetTrigger(paramName);
    }
}
