using UnityEngine;

/// <summary>
/// Fly enemy type.
/// 
/// Role: Flying annoyance — patrols in the air, circles the player, dive-bombs.
/// 
/// Behaviour:
/// - Flying movement (uses FlyingEnemyMovement)
/// - Circles around player when in chase mode
/// - Dive attack: swoops down toward player
/// - Erratic, buzzing movement pattern
/// - Retreats upward between attacks
/// 
/// Animator Parameters:
/// - FlySpeed (float)   : flight speed
/// - IsAttacking (bool)  : currently attacking
/// - HitTrigger          : hit reaction
/// - IsDead (bool)       : death state
/// 
/// Inspector Defaults:
/// - Move Speed: 3.5
/// - Chase Speed: 5
/// - Fly Height: 2.5
/// - Attack Range: 1.5
/// - Health: 20 (very fragile)
/// </summary>
public class FlyEnemy : EnemyBase
{
    [Header("=== FLY SPECIFIC ===")]

    [Header("Flight Settings")]
    [SerializeField] private float flyHeight = 2.5f;
    [SerializeField] private float hoverAmplitude = 0.4f;
    [SerializeField] private float hoverFrequency = 2f;
    [SerializeField] private float circleRadius = 4f;
    [SerializeField] private float circleSpeed = 2f;

    [Header("Dive Attack")]
    [SerializeField] private float diveSpeed = 10f;
    [SerializeField] private float diveDuration = 0.6f;
    [SerializeField] private float retreatHeight = 4f;
    [SerializeField] private float minDiveCooldown = 2f;

    [Header("Erratic Movement")]
    [SerializeField] private float erraticStrength = 1f;
    [SerializeField] private float erraticChangeInterval = 0.3f;

    // References
    private FlyingEnemyMovement flyingMovement;

    // Runtime
    private float diveTimer = 0f;
    private bool isDiving = false;
    private float lastDiveTime = -Mathf.Infinity;
    private float circleAngle = 0f;
    private Vector3 erraticOffset = Vector3.zero;
    private float erraticTimer = 0f;

    bool canDive => Time.time >= lastDiveTime + minDiveCooldown;

    protected override void Awake()
    {
        // Set fly-specific defaults
        moveSpeed = 3.5f;
        chaseSpeed = 5f;
        idleDuration = 1f;

        base.Awake();

        // Get or add flying movement component
        flyingMovement = GetComponent<FlyingEnemyMovement>();
        if (flyingMovement == null)
            flyingMovement = gameObject.AddComponent<FlyingEnemyMovement>();
    }

    protected override void Start()
    {
        // Initialize flying movement before base Start
        if (flyingMovement != null)
        {
            flyingMovement.Initialize(this);
        }

        base.Start();

        // Start buzzing sound
        if (audioHandler != null)
            audioHandler.StartIdleLoop();
    }

    protected override void Update()
    {
        // Override standard Update to include flying movement
        if (!isInitialized) return;
        if (!stateMachine.IsAlive) return;

        // Update flying movement every frame
        if (flyingMovement != null)
        {
            flyingMovement.UpdateFlyingMovement();
        }

        UpdateCurrentState();

        // Detection with flying
        if (detection != null && detection.enabled)
            detection.CheckDetection(playerTransform);

        // Animation
        if (enemyAnimator != null)
        {
            enemyAnimator.UpdateAnimations(stateMachine.CurrentState, navMeshAgent != null ? navMeshAgent.velocity.magnitude : moveSpeed);
        }
    }

    protected override void HandleEnterPatrol()
    {
        base.HandleEnterPatrol();

        // Flying patrol: move between waypoints at flying height
        if (patrol != null && patrol.HasWaypoints)
        {
            Vector3 wp = patrol.GetNearestWaypoint(cachedTransform.position);
            flyingMovement?.SetDestination(wp);
        }
    }

    protected override void UpdatePatrol()
    {
        if (patrol != null)
        {
            patrol.UpdatePatrol();

            // Update flying destination to current patrol target
            if (flyingMovement != null && navMeshAgent != null && navMeshAgent.hasPath)
            {
                flyingMovement.SetDestination(navMeshAgent.destination);
            }
        }

        // Erratic movement during patrol
        UpdateErraticMovement();
    }

    protected override void HandleEnterChase()
    {
        base.HandleEnterChase();

        circleAngle = 0f;

        if (audioHandler != null)
        {
            audioHandler.StopLoopingSounds();
            audioHandler.StartChaseLoop();
        }
    }

    protected override void UpdateChase()
    {
        if (playerTransform == null)
        {
            stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Idle);
            return;
        }

        float distToPlayer = Vector3.Distance(cachedTransform.position, playerTransform.position);

        if (canDive && distToPlayer <= (attack != null ? attack.AttackRange * 2f : 3f))
        {
            // Start dive attack
            stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Attack);
            return;
        }

        // Circle around player
        circleAngle += circleSpeed * Time.deltaTime;

        // Calculate circle position at fly height above player
        Vector3 circlePos = playerTransform.position + Vector3.up * flyHeight;
        circlePos += new Vector3(
            Mathf.Cos(circleAngle) * circleRadius,
            Mathf.Sin(circleAngle * 0.5f) * hoverAmplitude, // slight vertical oscillation
            Mathf.Sin(circleAngle) * circleRadius
        );

        // Add erratic movement
        circlePos += GetErraticOffset();

        // Set flying destination
        flyingMovement?.SetDestination(circlePos);

        // Rotate to face player
        RotateTowards(playerTransform.position);

        // Check lost player
        if (detection != null && !detection.IsPlayerInChaseRange(playerTransform))
        {
            stateMachine.TransitionTo(EnemyStateMachine.EnemyState.ReturnToPatrol);
        }
    }

    protected override void HandleEnterAttack()
    {
        base.HandleEnterAttack();

        isDiving = true;
        diveTimer = 0f;
        lastDiveTime = Time.time;

        if (enemyAnimator != null)
            enemyAnimator.TriggerAttack();

        if (audioHandler != null)
            audioHandler.PlayAttackSound();
    }

    protected override void UpdateAttack()
    {
        if (!isDiving)
        {
            base.UpdateAttack();
            return;
        }

        diveTimer += Time.deltaTime;

        if (playerTransform != null)
        {
            if (diveTimer <= diveDuration * 0.5f)
            {
                // Dive TOWARD player
                Vector3 diveTarget = playerTransform.position + Vector3.up * 0.5f;
                flyingMovement?.SetDestination(diveTarget);

                // Increase speed during dive
                if (navMeshAgent != null)
                    navMeshAgent.speed = diveSpeed;

                RotateTowards(diveTarget);
            }
            else
            {
                // Retreat upward
                Vector3 retreatTarget = playerTransform.position + Vector3.up * retreatHeight;
                retreatTarget += (cachedTransform.position - playerTransform.position).normalized * circleRadius * 0.5f;
                flyingMovement?.SetDestination(retreatTarget);

                if (navMeshAgent != null)
                    navMeshAgent.speed = chaseSpeed;
            }

            // Apply damage at the peak of dive
            if (diveTimer >= diveDuration * 0.4f && diveTimer <= diveDuration * 0.6f)
            {
                if (attack != null)
                {
                    attack.OnAttackHit();
                }
            }
        }

        // End dive
        if (diveTimer >= diveDuration)
        {
            isDiving = false;
            diveTimer = 0f;

            // Reset nav speed
            if (navMeshAgent != null)
                navMeshAgent.speed = chaseSpeed;

            stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Chase);
        }
    }

    protected override void HandleEnterReturnToPatrol()
    {
        base.HandleEnterReturnToPatrol();

        if (audioHandler != null)
        {
            audioHandler.StopLoopingSounds();
            audioHandler.StartIdleLoop();
        }

        // Return at flying height
        if (patrol != null && patrol.HasWaypoints)
        {
            Vector3 returnTarget = patrol.GetNearestWaypoint(cachedTransform.position);
            returnTarget.y += flyHeight;
            flyingMovement?.SetDestination(returnTarget);
        }
    }

    protected override void HandleEnterDead()
    {
        base.HandleEnterDead();

        // Fly falls to ground on death — disable flying movement
        if (flyingMovement != null)
            flyingMovement.enabled = false;

        if (audioHandler != null)
            audioHandler.StopLoopingSounds();
    }

    // ===== ERRATIC MOVEMENT =====

    private void UpdateErraticMovement()
    {
        erraticTimer += Time.deltaTime;
        if (erraticTimer >= erraticChangeInterval)
        {
            erraticTimer = 0f;
            erraticOffset = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-0.5f, 0.5f),
                Random.Range(-1f, 1f)
            ) * erraticStrength;
        }
    }

    private Vector3 GetErraticOffset()
    {
        UpdateErraticMovement();
        return erraticOffset;
    }
}
