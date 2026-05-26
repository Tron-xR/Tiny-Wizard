using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Base class for all enemy types.
/// 
/// Provides:
/// - Common references (NavMeshAgent, Animator, Collider, etc.)
/// - State machine integration
/// - Inspector-editable movement speeds
/// - Auto-discovery of child components
/// 
/// To create a new enemy type:
/// 1. Extend this class (e.g. public class CockroachEnemy : EnemyBase)
/// 2. Override Awake() to set type-specific defaults
/// 3. The sub-components (patrol, detection, attack, etc.) are auto-discovered
/// 
/// Hierarchy:
///   EnemyRoot (this script)
///   ├── Visuals (MeshRenderer, Animator)
///   ├── Collider (CapsuleCollider / BoxCollider / SphereCollider)
///   ├── NavMeshAgent
///   ├── DetectionPoint (empty Transform at eye level)
///   ├── AttackPoint (empty Transform at attack origin)
///   ├── VFXRoot (empty Transform for particle systems)
///   └── AudioRoot (empty Transform for AudioSources)
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStateMachine))]
public abstract class EnemyBase : MonoBehaviour
{
    [Header("=== ENEMY BASE SETTINGS ===")]

    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 3.5f;
    [SerializeField] protected float chaseSpeed = 6f;
    [SerializeField] protected float rotationSpeed = 120f; // degrees per second
    [SerializeField] protected float acceleration = 8f;

    [Header("State Timings")]
    [SerializeField] protected float idleDuration = 2f;
    [SerializeField] protected float returnToPatrolSpeed = 3f;

    [Header("References (auto-found if empty)")]
    [SerializeField] protected NavMeshAgent navMeshAgent;
    [SerializeField] protected Animator animator;
    [SerializeField] protected Collider enemyCollider;

    // Sub-component references — auto-discovered in Awake
    protected EnemyStateMachine stateMachine;
    protected EnemyPatrol patrol;
    protected EnemyDetection detection;
    protected EnemyAttack attack;
    protected EnemyHealth health;
    protected EnemyAnimator enemyAnimator;
    protected EnemyAudioHandler audioHandler;
    protected EnemyVFXHandler vfxHandler;

    // Cached transform for performance
    protected Transform cachedTransform;
    protected Transform playerTransform;

    // Runtime state
    protected float stateTimer = 0f;
    protected bool isInitialized = false;

    // ===== PROPERTIES =====

    public NavMeshAgent Agent => navMeshAgent;
    public EnemyStateMachine StateMachine => stateMachine;
    public EnemyPatrol Patrol => patrol;
    public EnemyDetection Detection => detection;
    public EnemyAttack Attack => attack;
    public EnemyHealth Health => health;
    public Transform PlayerTransform => playerTransform;
    public float MoveSpeed => moveSpeed;
    public float ChaseSpeed => chaseSpeed;

    // ===== UNITY LIFECYCLE =====

    protected virtual void Awake()
    {
        cachedTransform = transform;

        // Auto-find NavMeshAgent
        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();

        // Auto-find state machine component
        stateMachine = GetComponent<EnemyStateMachine>();

        // Auto-discover sub-components on this GameObject
        patrol = GetComponent<EnemyPatrol>();
        detection = GetComponent<EnemyDetection>();
        attack = GetComponent<EnemyAttack>();
        health = GetComponent<EnemyHealth>();
        enemyAnimator = GetComponent<EnemyAnimator>();
        audioHandler = GetComponent<EnemyAudioHandler>();
        vfxHandler = GetComponent<EnemyVFXHandler>();

        // Auto-find collider
        if (enemyCollider == null)
            enemyCollider = GetComponent<Collider>();

        // Auto-find animator in children
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    protected virtual void Start()
    {
        // Find the player in the scene
        FindPlayer();

        // Configure NavMeshAgent
        SetupNavMeshAgent();

        // Initialize sub-components
        InitializeComponents();

        // Subscribe to state machine events
        stateMachine.OnStateEnter += HandleStateEnter;
        stateMachine.OnStateExit += HandleStateExit;

        // Start in Idle state
        stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Idle);

        isInitialized = true;
    }

    protected virtual void OnDestroy()
    {
        if (stateMachine != null)
        {
            stateMachine.OnStateEnter -= HandleStateEnter;
            stateMachine.OnStateExit -= HandleStateExit;
        }

        if (health != null)
            health.OnDeath -= HandleDeath;

        if (detection != null)
        {
            detection.OnPlayerDetected -= HandlePlayerDetected;
            detection.OnPlayerLost -= HandlePlayerLost;
        }

        if (attack != null)
        {
            attack.OnAttack -= HandleAttackPerformed;
            attack.OnAttackHitEvent -= HandleAttackHit;
        }
    }

    protected virtual void Update()
    {
        if (!isInitialized) return;
        if (!stateMachine.IsAlive) return;

        // Update the current state
        UpdateCurrentState();

        // Update sub-components
        if (detection != null && detection.enabled)
            detection.CheckDetection(playerTransform);

        // Update animation parameters
        if (enemyAnimator != null)
            enemyAnimator.UpdateAnimations(stateMachine.CurrentState, navMeshAgent.velocity.magnitude);
    }

    // ===== STATE MACHINE =====

    /// <summary>
    /// Main state update — delegates to the appropriate handler method.
    /// </summary>
    protected virtual void UpdateCurrentState()
    {
        switch (stateMachine.CurrentState)
        {
            case EnemyStateMachine.EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyStateMachine.EnemyState.Patrol:
                UpdatePatrol();
                break;
            case EnemyStateMachine.EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyStateMachine.EnemyState.Attack:
                UpdateAttack();
                break;
            case EnemyStateMachine.EnemyState.ReturnToPatrol:
                UpdateReturnToPatrol();
                break;
        }
    }

    /// <summary>
    /// Called when entering any state. Override to add type-specific behaviour.
    /// </summary>
    protected virtual void OnEnterState(EnemyStateMachine.EnemyState state)
    {
        stateTimer = 0f;

        switch (state)
        {
            case EnemyStateMachine.EnemyState.Idle:
                HandleEnterIdle();
                break;
            case EnemyStateMachine.EnemyState.Patrol:
                HandleEnterPatrol();
                break;
            case EnemyStateMachine.EnemyState.Chase:
                HandleEnterChase();
                break;
            case EnemyStateMachine.EnemyState.Attack:
                HandleEnterAttack();
                break;
            case EnemyStateMachine.EnemyState.ReturnToPatrol:
                HandleEnterReturnToPatrol();
                break;
            case EnemyStateMachine.EnemyState.Dead:
                HandleEnterDead();
                break;
        }
    }

    /// <summary>
    /// Called when exiting any state. Override to clean up.
    /// </summary>
    protected virtual void OnExitState(EnemyStateMachine.EnemyState state) { }

    // ===== STATE HANDLERS (override in derived classes) =====

    protected virtual void HandleEnterIdle()
    {
        StopAgent();
        if (enemyAnimator != null)
            enemyAnimator.SetIdle();
    }

    protected virtual void HandleEnterPatrol()
    {
        if (patrol != null)
            patrol.StartPatrol();
    }

    protected virtual void HandleEnterChase()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = chaseSpeed;
            navMeshAgent.stoppingDistance = attack != null ? attack.AttackRange * 0.8f : 1f;
        }
        if (audioHandler != null)
            audioHandler.PlayDetectionSound();
    }

    protected virtual void HandleEnterAttack()
    {
        StopAgent();
        if (enemyAnimator != null)
            if (enemyAnimator != null) enemyAnimator.TriggerAttack();
    }

    protected virtual void HandleEnterReturnToPatrol()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = returnToPatrolSpeed;
            navMeshAgent.stoppingDistance = 0.5f;
        }
    }

    protected virtual void HandleEnterDead()
    {
        StopAgent();
        if (enemyCollider != null)
            enemyCollider.enabled = false;

        if (navMeshAgent != null)
            navMeshAgent.enabled = false;

        if (enemyAnimator != null)
            enemyAnimator.SetDead();

        if (audioHandler != null)
            audioHandler.PlayDeathSound();

        if (vfxHandler != null)
            vfxHandler.PlayDeathVFX(cachedTransform.position);
    }

    // ===== STATE UPDATE METHODS =====

    protected virtual void UpdateIdle()
    {
        stateTimer += Time.deltaTime;

        if (stateTimer >= idleDuration)
        {
            // Transition to patrol if we have waypoints
            if (patrol != null && patrol.HasWaypoints)
                stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Patrol);
            else
                stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Idle); // loop idle
        }
    }

    protected virtual void UpdatePatrol()
    {
        if (patrol != null)
        {
            patrol.UpdatePatrol();
        }
    }

    protected virtual void UpdateChase()
    {
        if (playerTransform == null)
        {
            stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Idle);
            return;
        }

        // Move toward player
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(playerTransform.position);
        }

        // Rotate to face player while chasing
        RotateTowards(playerTransform.position);

        // Check if we're close enough to attack
        if (attack != null)
        {
            float distToPlayer = Vector3.Distance(cachedTransform.position, playerTransform.position);
            if (distToPlayer <= attack.AttackRange && attack.CanAttack)
            {
                stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Attack);
            }
        }

        // Check if we lost the player
        if (detection != null)
        {
            if (!detection.IsPlayerInChaseRange(playerTransform))
            {
                stateMachine.TransitionTo(EnemyStateMachine.EnemyState.ReturnToPatrol);
            }
        }
    }

    protected virtual void UpdateAttack()
    {
        if (attack == null) return;

        // Face the player during attack
        if (playerTransform != null)
            RotateTowards(playerTransform.position);

        attack.TryAttack(playerTransform, cachedTransform.position);

        // After attack cooldown, return to chase
        if (!attack.IsAttacking)
        {
            stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Chase);
        }
    }

    protected virtual void UpdateReturnToPatrol()
    {
        if (patrol == null || !patrol.HasWaypoints)
        {
            stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Idle);
            return;
        }

        // Check if player is detected again during return
        if (detection != null && detection.IsPlayerInDetectionRange(playerTransform))
        {
            stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Chase);
            return;
        }

        // Move to nearest waypoint
        Vector3 returnTarget = patrol.GetNearestWaypoint(cachedTransform.position);

        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(returnTarget);
        }

        // When close enough to waypoint, start patrolling again
        float dist = Vector3.Distance(cachedTransform.position, returnTarget);
        if (dist < 1.5f)
        {
            stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Patrol);
        }
    }

    // ===== EVENT HANDLERS =====

    private void HandleStateEnter(EnemyStateMachine.EnemyState state)
    {
        OnEnterState(state);
    }

    private void HandleStateExit(EnemyStateMachine.EnemyState state)
    {
        OnExitState(state);
    }

    private void HandlePlayerDetected()
    {
        if (stateMachine.IsAlive && stateMachine.CurrentState != EnemyStateMachine.EnemyState.Chase)
        {
            stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Chase);
        }
    }

    private void HandlePlayerLost()
    {
        if (stateMachine.CurrentState == EnemyStateMachine.EnemyState.Chase)
        {
            stateMachine.TransitionTo(EnemyStateMachine.EnemyState.ReturnToPatrol);
        }
    }

    private void HandleAttackPerformed()
    {
        if (enemyAnimator != null)
            enemyAnimator.TriggerAttack();
    }

    private void HandleAttackHit(GameObject target)
    {
        if (vfxHandler != null)
            vfxHandler.PlayAttackVFX(target.transform.position);
    }

    private void HandleDeath()
    {
        stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Dead);
    }

    // ===== PUBLIC API =====

    /// <summary>
    /// Called by external scripts to apply damage to this enemy.
    /// </summary>
    public virtual void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (!stateMachine.IsAlive) return;

        if (vfxHandler != null)
            vfxHandler.PlayHitVFX(hitPoint, hitNormal);

        if (audioHandler != null)
            audioHandler.PlayHitSound();

        if (enemyAnimator != null)
            enemyAnimator.TriggerHit();

        if (health != null)
            health.TakeDamage(amount, hitPoint, hitNormal);

        // When hit, become aware of the player
        if (detection != null && playerTransform != null)
        {
            detection.ForceDetectPlayer();
            if (stateMachine.CurrentState != EnemyStateMachine.EnemyState.Chase &&
                stateMachine.CurrentState != EnemyStateMachine.EnemyState.Attack)
            {
                stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Chase);
            }
        }
    }

    // ===== HELPER METHODS =====

    protected void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    protected void SetupNavMeshAgent()
    {
        if (navMeshAgent == null) return;

        navMeshAgent.speed = moveSpeed;
        navMeshAgent.acceleration = acceleration;
        navMeshAgent.angularSpeed = rotationSpeed;
        navMeshAgent.stoppingDistance = 0.5f;
        navMeshAgent.autoBraking = true;
        navMeshAgent.autoRepath = true;
        navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }

    protected void InitializeComponents()
    {
        // Initialize patrol
        if (patrol != null)
            patrol.Initialize(this);

        // Initialize detection
        if (detection != null)
        {
            detection.Initialize(this);
            detection.OnPlayerDetected += HandlePlayerDetected;
            detection.OnPlayerLost += HandlePlayerLost;
        }

        // Initialize attack
        if (attack != null)
        {
            attack.Initialize(this);
            attack.OnAttack += HandleAttackPerformed;
            attack.OnAttackHitEvent += HandleAttackHit;
        }

        // Initialize health
        if (health != null)
        {
            health.Initialize(this);
            health.OnDeath += HandleDeath;
        }

        // Initialize animator handler
        if (enemyAnimator != null)
            enemyAnimator.Initialize(animator);

        // Initialize audio
        if (audioHandler != null)
            audioHandler.Initialize(this);
    }

    protected void StopAgent()
    {
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.ResetPath();
            navMeshAgent.velocity = Vector3.zero;
        }
    }

    public void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - cachedTransform.position).normalized;
        direction.y = 0f;

        if (direction.magnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        cachedTransform.rotation = Quaternion.RotateTowards(
            cachedTransform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    // ===== GIZMOS =====

    protected virtual void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return;

        // Draw state machine info
        if (stateMachine != null)
        {
            Gizmos.color = Color.white;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, 
                $"State: {stateMachine.CurrentState}");
        }
    }
}
