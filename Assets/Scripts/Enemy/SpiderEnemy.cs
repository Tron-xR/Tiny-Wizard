using UnityEngine;

/// <summary>
/// Spider enemy type.
/// 
/// Role: Ambush enemy — waits in place, then lunges at nearby players.
/// 
/// Behaviour:
/// - Waits in idle (ambush posture)
/// - Detects player at medium range
/// - Lunges toward player (fast burst movement)
/// - Creepy idle animation
/// - Optional wall/ceiling movement illusion
/// 
/// Animator Parameters:
/// - IdleType (float) : different idle variations
/// - IsAggro (bool)   : became aggressive
/// - AttackTrigger     : lunge attack
/// - CrawlSpeed (float): movement speed for blending
/// - IsDead (bool)     : death state
/// </summary>
public class SpiderEnemy : EnemyBase
{
    [Header("=== SPIDER SPECIFIC ===")]

    [Header("Lunge Attack")]
    [SerializeField] private float lungeDistance = 4f;
    [SerializeField] private float lungeSpeed = 12f;
    [SerializeField] private float lungeDuration = 0.4f;

    [Header("Ambush")]
    [SerializeField] private float ambushDetectionRadius = 5f; // shorter than standard
    [SerializeField] private float aggroRange = 8f;
    [SerializeField] private float idleVariationMin = 0.5f;
    [SerializeField] private float idleVariationMax = 3f;

    [Header("Web Attack (Optional)")]
    [SerializeField] private bool hasWebAttack = false;
    [SerializeField] private GameObject webProjectilePrefab;
    [SerializeField] private float webCooldown = 4f;
    [SerializeField] private float webRange = 6f;

    // Runtime
    private bool hasLunged = false;
    private float lungeTimer = 0f;
    private Vector3 lungeTarget;
    private float nextWebTime = 0f;
    private float currentIdleTimer = 0f;

    protected override void Awake()
    {
        // Set spider-specific defaults
        moveSpeed = 2.5f;
        chaseSpeed = 5f;
        idleDuration = 4f; // longer idle (ambush predator)

        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        // Override detection radius for ambush style
        if (detection != null)
        {
            detection.CheckDetection(playerTransform); // initial check
        }

        currentIdleTimer = Random.Range(idleVariationMin, idleVariationMax);
    }

    protected override void HandleEnterIdle()
    {
        base.HandleEnterIdle();

        // Randomize idle duration for variety
        currentIdleTimer = Random.Range(idleVariationMin, idleVariationMax);

        // Play idle sound (hissing)
        if (audioHandler != null)
            audioHandler.StartIdleLoop();
    }

    protected override void UpdateIdle()
    {
        stateTimer += Time.deltaTime;

        // Spider waits in ambush — extra sensitive to player proximity
        if (playerTransform != null)
        {
            float dist = Vector3.Distance(cachedTransform.position, playerTransform.position);
            if (dist <= ambushDetectionRadius)
            {
                stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Chase);
                return;
            }
        }

        // After idle timer, patrol briefly
        if (stateTimer >= currentIdleTimer)
        {
            if (patrol != null && patrol.HasWaypoints)
                stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Patrol);
            else
            {
                // No waypoints — loop idle
                stateTimer = 0f;
                currentIdleTimer = Random.Range(idleVariationMin, idleVariationMax);
            }
        }
    }

    protected override void UpdateChase()
    {
        if (playerTransform == null)
        {
            stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Idle);
            return;
        }

        // Spider moves toward player but stops at lunge distance
        float distToPlayer = Vector3.Distance(cachedTransform.position, playerTransform.position);

        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            if (distToPlayer > lungeDistance)
            {
                // Move toward player
                navMeshAgent.speed = chaseSpeed;
                navMeshAgent.SetDestination(playerTransform.position);
            }
            else
            {
                // In lunge range — stop and prepare
                StopAgent();
            }
        }

        RotateTowards(playerTransform.position);

        // Lunge when close enough
        if (distToPlayer <= lungeDistance && attack != null && attack.CanAttack)
        {
            if (!hasLunged)
            {
                PerformLunge();
            }
        }

        // Check if player escaped aggro range
        if (detection != null && distToPlayer > aggroRange)
        {
            hasLunged = false;
            stateMachine.TransitionTo(EnemyStateMachine.EnemyState.ReturnToPatrol);
        }
    }

    protected override void UpdateAttack()
    {
        if (hasLunged)
        {
            lungeTimer += Time.deltaTime;

            // Move toward lunge target
            if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.speed = lungeSpeed;
                navMeshAgent.SetDestination(lungeTarget);
            }

            if (lungeTimer >= lungeDuration)
            {
                // Lunge finished
                hasLunged = false;
                lungeTimer = 0f;

                if (attack != null)
                {
                    attack.OnAttackHit(); // apply damage at end of lunge
                    attack.OnAttackFinished();
                }

                stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Chase);
            }
            return;
        }

        // Web attack
        if (hasWebAttack && webProjectilePrefab != null && Time.time >= nextWebTime)
        {
            if (playerTransform != null)
            {
                float dist = Vector3.Distance(cachedTransform.position, playerTransform.position);
                if (dist <= webRange)
                {
                    FireWeb();
                    nextWebTime = Time.time + webCooldown;
                }
            }
        }

        base.UpdateAttack();
    }

    private void PerformLunge()
    {
        hasLunged = true;
        lungeTimer = 0f;

        // Set lunge target slightly past the player
        if (playerTransform != null)
        {
            Vector3 dirToPlayer = (playerTransform.position - cachedTransform.position).normalized;
            lungeTarget = playerTransform.position + dirToPlayer * 0.5f;
        }

        if (audioHandler != null)
            audioHandler.PlayAttackSound();

        if (enemyAnimator != null)
            enemyAnimator.TriggerAttack();
    }

    private void FireWeb()
    {
        if (webProjectilePrefab == null || playerTransform == null) return;

        Vector3 dir = (playerTransform.position - cachedTransform.position).normalized;

        GameObject web = Instantiate(webProjectilePrefab, cachedTransform.position + dir * 1.5f, Quaternion.LookRotation(dir));

        // Destroy web projectile after some time
        Destroy(web, 5f);
    }

    protected override void HandleEnterDead()
    {
        base.HandleEnterDead();

        if (audioHandler != null)
            audioHandler.StopLoopingSounds();
    }
}
