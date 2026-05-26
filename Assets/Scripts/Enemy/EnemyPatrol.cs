using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Handles waypoint-based patrol for enemies.
/// 
/// Supports:
/// - Ordered waypoint patrol (cycle through waypoints in sequence)
/// - Random waypoint patrol (pick random waypoints)
/// - Wait time at each waypoint
/// - Editor gizmo visualization
/// 
/// Inspector Setup:
/// 1. Add this component to the enemy GameObject
/// 2. Create empty child GameObjects as waypoints
/// 3. Assign them to the waypoints array
/// 4. Set patrol speed and wait time
/// 
/// Waypoints should be placed on the NavMesh for proper pathfinding.
/// </summary>
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private bool randomPatrol = false;
    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float waitTimeAtWaypoint = 2f;
    [SerializeField] private float waypointReachedDistance = 0.8f;
    [SerializeField] private bool loopPatrol = true;

    [Header("Gizmos")]
    [SerializeField] private Color waypointColor = Color.cyan;
    [SerializeField] private Color connectionColor = Color.cyan;

    // References
    private EnemyBase enemyBase;
    private NavMeshAgent navMeshAgent;

    // Runtime state
    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool isPatrolling = false;

    public bool HasWaypoints => waypoints != null && waypoints.Length > 0;
    public int WaypointCount => waypoints != null ? waypoints.Length : 0;
    public bool IsWaiting => isWaiting;

    // ===== INITIALIZATION =====

    public void Initialize(EnemyBase enemy)
    {
        enemyBase = enemy;
        navMeshAgent = enemy.Agent;
    }

    // ===== PUBLIC API =====

    /// <summary>
    /// Start or restart patrol from the beginning.
    /// </summary>
    public void StartPatrol()
    {
        if (!HasWaypoints) return;

        isPatrolling = true;
        isWaiting = false;
        waitTimer = 0f;

        if (navMeshAgent != null)
        {
            navMeshAgent.speed = patrolSpeed;
            navMeshAgent.stoppingDistance = 0.3f;
        }

        // Move to first waypoint
        SetDestinationToWaypoint(currentWaypointIndex);
    }

    /// <summary>
    /// Stop patrolling.
    /// </summary>
    public void StopPatrol()
    {
        isPatrolling = false;
        isWaiting = false;

        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.ResetPath();
        }
    }

    /// <summary>
    /// Update patrol movement. Call from EnemyBase's UpdatePatrol().
    /// </summary>
    public void UpdatePatrol()
    {
        if (!isPatrolling || !HasWaypoints) return;

        // Handle waiting at waypoint
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTimeAtWaypoint)
            {
                isWaiting = false;
                waitTimer = 0f;
                MoveToNextWaypoint();
            }
            return;
        }

        // Check if we reached the current waypoint
        float dist = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position);

        if (dist <= waypointReachedDistance)
        {
            // Arrived at waypoint
            if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.ResetPath();
            }

            isWaiting = true;
            waitTimer = 0f;
        }

        // Rotate toward waypoint while moving
        if (!isWaiting && waypoints.Length > 0)
        {
            enemyBase.RotateTowards(waypoints[currentWaypointIndex].position);
        }
    }

    /// <summary>
    /// Get the nearest waypoint to a given position (used for return-to-patrol).
    /// </summary>
    public Vector3 GetNearestWaypoint(Vector3 position)
    {
        if (!HasWaypoints) return transform.position;

        int nearestIndex = 0;
        float nearestDist = float.MaxValue;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            float dist = Vector3.Distance(position, waypoints[i].position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearestIndex = i;
            }
        }

        currentWaypointIndex = nearestIndex;
        return waypoints[nearestIndex].position;
    }

    // ===== PRIVATE METHODS =====

    private void MoveToNextWaypoint()
    {
        if (!HasWaypoints) return;

        if (randomPatrol)
        {
            // Pick a random waypoint that's different from the current one
            int nextIndex = Random.Range(0, waypoints.Length);
            while (nextIndex == currentWaypointIndex && waypoints.Length > 1)
            {
                nextIndex = Random.Range(0, waypoints.Length);
            }
            currentWaypointIndex = nextIndex;
        }
        else
        {
            // Ordered patrol
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
            {
                if (loopPatrol)
                    currentWaypointIndex = 0;
                else
                {
                    // Reverse direction if not looping
                    currentWaypointIndex = waypoints.Length - 1;
                    isPatrolling = false;
                    return;
                }
            }
        }

        SetDestinationToWaypoint(currentWaypointIndex);
    }

    private void SetDestinationToWaypoint(int index)
    {
        if (!HasWaypoints || index < 0 || index >= waypoints.Length) return;
        if (waypoints[index] == null) return;

        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(waypoints[index].position);
        }
    }

    // ===== GIZMOS =====

    private void OnDrawGizmosSelected()
    {
        if (waypoints == null) return;

        Gizmos.color = waypointColor;

        // Draw waypoint spheres
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);

            // Draw connection lines
            if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
            {
                Gizmos.color = connectionColor;
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }

        // Draw looping connection
        if (loopPatrol && waypoints.Length > 1 && waypoints[0] != null && waypoints[waypoints.Length - 1] != null)
        {
            Gizmos.color = connectionColor;
            Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
        }
    }
}
