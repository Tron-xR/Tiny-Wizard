using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Flying movement behaviour for airborne enemies (Fly).
/// 
/// Overrides standard NavMesh ground movement with:
/// - Altitude control (fly at a fixed height above ground/navmesh)
/// - Hover oscillation (gentle bobbing up and down)
/// - No ground-based navigation constraints
/// - Flies directly toward targets without pathfinding
/// 
/// How it works:
/// The NavMeshAgent's position is updated every frame to maintain altitude.
/// Movement is calculated toward the destination, ignoring navmesh Y differences.
/// A sine wave adds hovering oscillation.
/// 
/// Inspector Setup:
/// 1. Add this component to the enemy alongside EnemyBase
/// 2. Set fly height and hover parameters
/// 3. The NavMeshAgent should have very low or zero Radius/Height/Step Height
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class FlyingEnemyMovement : MonoBehaviour
{
    [Header("Flying Settings")]
    [SerializeField] private float flyHeight = 3f;          // height above ground to maintain
    [SerializeField] private float hoverAmplitude = 0.3f;   // bob up/down amount
    [SerializeField] private float hoverFrequency = 1.5f;   // bob speed
    [SerializeField] private float flySpeed = 4f;
    [SerializeField] private float maxFlySpeed = 6f;
    [SerializeField] private float rotationSpeed = 180f;    // degrees per second

    [Header("Navigation")]
    [SerializeField] private bool useDirectMovement = true;  // true: bypass NavMesh pathfinding
    [SerializeField] private LayerMask groundLayer = -1;

    // References
    private NavMeshAgent navMeshAgent;
    private Transform cachedTransform;
    private EnemyBase enemyBase;

    // Runtime state
    private Vector3 targetPosition;
    private bool hasTarget = false;
    private float hoverOffset = 0f;
    private float groundY = 0f;

    // ===== INITIALIZATION =====

    public void Initialize(EnemyBase enemy)
    {
        enemyBase = enemy;
        cachedTransform = transform;
        navMeshAgent = enemy.Agent;

        // Configure agent for flying
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = flySpeed;
            navMeshAgent.baseOffset = flyHeight;
            navMeshAgent.radius = 0.1f;
            navMeshAgent.height = 0.1f;
            navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }

        // Randomize hover start so enemies don't bob in sync
        hoverOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    // ===== PUBLIC API =====

    /// <summary>
    /// Set the target position for the flying enemy to move toward.
    /// </summary>
    public void SetDestination(Vector3 destination)
    {
        targetPosition = destination;
        hasTarget = true;

        if (navMeshAgent != null && navMeshAgent.isOnNavMesh && !useDirectMovement)
        {
            navMeshAgent.SetDestination(destination);
        }
    }

    /// <summary>
    /// Clear the current destination.
    /// </summary>
    public void ClearDestination()
    {
        hasTarget = false;

        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.ResetPath();
        }
    }

    /// <summary>
    /// Update flying movement. Call from EnemyBase's Update().
    /// </summary>
    public void UpdateFlyingMovement()
    {
        if (cachedTransform == null) return;

        // Find ground height below the enemy
        FindGroundHeight();

        // Calculate hover oscillation (sine wave bob)
        float hover = Mathf.Sin((Time.time + hoverOffset) * hoverFrequency) * hoverAmplitude;

        // Calculate target altitude
        float targetY = groundY + flyHeight + hover;

        if (useDirectMovement && hasTarget)
        {
            // Direct movement toward target (no NavMesh pathfinding)
            Vector3 currentPos = cachedTransform.position;
            Vector3 direction = (targetPosition - currentPos).normalized;
            direction.y = 0f;

            float dist = Vector3.Distance(currentPos, targetPosition);

            if (dist > 0.5f && direction.magnitude > 0.01f)
            {
                // Move toward target
                float moveSpeed = Mathf.Min(flySpeed, dist * 2f); // slow down near target
                Vector3 move = direction * moveSpeed * Time.deltaTime;
                move.y = 0f;

                Vector3 newPos = currentPos + move;
                newPos.y = targetY;

                // Apply using NavMeshAgent for proper physics interaction
                if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
                {
                    navMeshAgent.Move(move);
                    // Adjust Y after Move (which only affects XZ)
                    Vector3 agentPos = navMeshAgent.nextPosition;
                    agentPos.y = targetY;
                    navMeshAgent.nextPosition = agentPos;
                }
                else
                {
                    cachedTransform.position = new Vector3(newPos.x, targetY, newPos.z);
                }

                // Rotate toward movement direction
                RotateToward(direction);
            }
            else
            {
                // Maintain hover position
                Vector3 pos = cachedTransform.position;
                pos.y = targetY;
                cachedTransform.position = pos;
            }
        }
        else
        {
            // Use NavMeshAgent for movement, but override Y
            if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
            {
                Vector3 agentPos = navMeshAgent.nextPosition;
                agentPos.y = targetY;
                navMeshAgent.nextPosition = agentPos;
            }
            else
            {
                Vector3 pos = cachedTransform.position;
                pos.y = targetY;
                cachedTransform.position = pos;
            }
        }
    }

    /// <summary>
    /// Check if the flying enemy has reached its destination.
    /// </summary>
    public bool HasReachedDestination(float threshold = 1f)
    {
        if (!hasTarget) return true;

        float dist = Vector3.Distance(
            new Vector3(cachedTransform.position.x, 0f, cachedTransform.position.z),
            new Vector3(targetPosition.x, 0f, targetPosition.z)
        );

        return dist <= threshold;
    }

    // ===== PRIVATE METHODS =====

    private void FindGroundHeight()
    {
        RaycastHit hit;
        Vector3 origin = cachedTransform.position + Vector3.up * 10f;

        if (Physics.Raycast(origin, Vector3.down, out hit, 30f, groundLayer))
        {
            groundY = hit.point.y;
        }
        else
        {
            groundY = cachedTransform.position.y - flyHeight;
        }
    }

    private void RotateToward(Vector3 direction)
    {
        if (direction.magnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        cachedTransform.rotation = Quaternion.RotateTowards(
            cachedTransform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    // ===== GIZMOS =====

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Vector3 pos = transform.position;
        pos.y += flyHeight;
        Gizmos.DrawWireSphere(pos, 0.3f);

        Gizmos.color = new Color(1f, 0f, 1f, 0.1f);
        Gizmos.DrawLine(transform.position, pos);
    }
}
