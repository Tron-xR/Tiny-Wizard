using UnityEngine;

/// <summary>
/// Cockroach enemy type.
/// 
/// Role: Basic ground enemy — fast, aggressive, swarming.
/// 
/// Behaviour:
/// - Fast movement speed
/// - Short attack range (bite/charge)
/// - Aggressive chase
/// - Chaotic movement patterns (quick turns)
/// 
/// Animator Parameters:
/// - Speed (float)
/// - IsChasing (bool)
/// - AttackTrigger 
/// - IsDead (bool)
/// 
/// Inspector Defaults:
/// - Move Speed: 4.5 (fast for a scurrying insect)
/// - Chase Speed: 7 (very fast when chasing)
/// - Attack Range: 1.0
/// - Attack Damage: 8 (low damage, makes up with numbers)
/// - Health: 30 (fragile, dies fast)
/// - Detection Radius: 10 (good awareness)
/// </summary>
public class CockroachEnemy : EnemyBase
{
    [Header("=== COCKROACH SPECIFIC ===")]

    [Header("Charge Attack")]
    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private float chargeDuration = 0.5f;
    [SerializeField] private bool useChargeAttack = true;

    [Header("Chaotic Movement")]
    [SerializeField] private float directionChangeInterval = 0.8f;
    [SerializeField] private float chaosAngle = 30f;

    // Runtime
    private float directionChangeTimer = 0f;
    private bool isCharging = false;
    private float chargeTimer = 0f;

    protected override void Awake()
    {
        // Set cockroach-specific defaults before base Awake()
        moveSpeed = 4.5f;
        chaseSpeed = 7f;
        idleDuration = 1.5f;

        base.Awake();
    }

    protected override void UpdateChase()
    {
        if (playerTransform == null)
        {
            stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Idle);
            return;
        }

        // Chaotic movement: randomly change direction
        directionChangeTimer += Time.deltaTime;
        if (directionChangeTimer >= directionChangeInterval)
        {
            directionChangeTimer = 0f;
            chaosAngle = Random.Range(-45f, 45f);
        }

        // Move toward player with slight chaos offset (simulates insect pathfinding)
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.speed = chaseSpeed;

            // Add slight chaos to destination
            Vector3 offset = Random.insideUnitSphere * 0.3f;
            Vector3 dest = playerTransform.position + offset;
            navMeshAgent.SetDestination(dest);
        }

        RotateTowards(playerTransform.position);

        // Check attack range
        if (attack != null)
        {
            float dist = Vector3.Distance(cachedTransform.position, playerTransform.position);
            if (dist <= attack.AttackRange && attack.CanAttack)
            {
                if (useChargeAttack)
                {
                    StartCharge();
                }
                stateMachine.TransitionTo(EnemyStateMachine.EnemyState.Attack);
            }
        }

        // Check if lost player
        if (detection != null && !detection.IsPlayerInChaseRange(playerTransform))
        {
            stateMachine.TransitionTo(EnemyStateMachine.EnemyState.ReturnToPatrol);
        }
    }

    protected override void UpdateAttack()
    {
        if (isCharging)
        {
            // Charge toward player
            chargeTimer += Time.deltaTime;

            if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.speed = chargeSpeed;
                navMeshAgent.SetDestination(playerTransform != null ? playerTransform.position : cachedTransform.position);

                // Rotate toward player during charge
                if (playerTransform != null)
                    RotateTowards(playerTransform.position);
            }

            if (chargeTimer >= chargeDuration)
            {
                isCharging = false;
                chargeTimer = 0f;

                if (navMeshAgent != null)
                    navMeshAgent.speed = chaseSpeed;
            }

            return;
        }

        base.UpdateAttack();
    }

    private void StartCharge()
    {
        isCharging = true;
        chargeTimer = 0f;
    }

    protected override void HandleEnterAttack()
    {
        base.HandleEnterAttack();

        if (audioHandler != null)
            audioHandler.PlayAttackSound();

        if (useChargeAttack)
        {
            StartCharge();
        }
    }

    public override void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        base.TakeDamage(amount, hitPoint, hitNormal);

        // Cockroaches are startled by damage — speed up briefly
        if (stateMachine.IsAlive && navMeshAgent != null)
        {
            navMeshAgent.speed = chaseSpeed * 1.3f;
        }
    }
}
