using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Wall/climbing movement behaviour for spiders.
/// 
/// Gives the illusion of wall and ceiling traversal by:
/// - Raycasting to find nearby walkable surfaces (walls, floor, ceiling)
/// - Aligning the enemy's up vector to the surface normal
/// - Using NavMeshAgent while overriding rotation to stick to surfaces
/// 
/// NOTE: This is a simplified "illusion" system. For full wall traversal,
/// you would need custom NavMesh baking on walls. This implementation
/// works best on simple geometry where the spider walks on the ground
/// but rotates to match nearby walls for visual effect.
/// 
/// Inspector Setup:
/// 1. Add this component to the spider enemy
/// 2. Set the surface detection parameters
/// 3. Assign the walkable layers
/// 
/// Known Limitation:
/// True wall/ceiling navigation requires NavMesh baking on vertical surfaces
/// which is beyond standard Unity NavMesh. This script provides a visual
/// approximation suitable for game jams.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class SpiderWallMovement : MonoBehaviour
{
    [Header("Surface Detection")]
    [SerializeField] private float surfaceCheckDistance = 1f;
    [SerializeField] private float surfaceCheckRadius = 0.3f;
    [SerializeField] private LayerMask walkableLayers = -1;
    [SerializeField] private float surfaceAlignmentSpeed = 5f;

    [Header("Ground Settings")]
    [SerializeField] private bool alignToSurface = true; // rotate to match surface normal
    [SerializeField] private float maxSurfaceAngle = 80f; // max angle considered walkable

    [Header("Ceiling Settings")]
    [SerializeField] private bool canWalkOnCeiling = false;
    [SerializeField] private float ceilingCheckDistance = 2f;

    // References
    private Transform cachedTransform;
    private NavMeshAgent navMeshAgent;

    // Runtime state
    private Vector3 surfaceNormal = Vector3.up;
    private bool onWalkableSurface = false;

    // ===== INITIALIZATION =====

    public void Initialize(EnemyBase enemy)
    {
        cachedTransform = transform;
        navMeshAgent = enemy.Agent;

        if (navMeshAgent != null)
        {
            navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        }
    }

    // ===== PUBLIC API =====

    /// <summary>
    /// Update surface alignment. Call from EnemyBase's Update() or UpdatePatrol().
    /// </summary>
    public void UpdateSurfaceAlignment()
    {
        if (!alignToSurface) return;

        // Check for nearby surfaces
        FindNearestSurface();

        if (onWalkableSurface)
        {
            // Smoothly rotate to align with surface normal
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, surfaceNormal);
            // Preserve forward direction as much as possible
            Vector3 forward = cachedTransform.forward;
            Vector3 projectedForward = Vector3.ProjectOnPlane(forward, surfaceNormal);

            if (projectedForward.magnitude > 0.01f)
            {
                targetRotation = Quaternion.LookRotation(projectedForward, surfaceNormal);
            }

            cachedTransform.rotation = Quaternion.Slerp(
                cachedTransform.rotation,
                targetRotation,
                surfaceAlignmentSpeed * Time.deltaTime
            );
        }
        else
        {
            // No surface found — return to upright
            Quaternion upright = Quaternion.FromToRotation(Vector3.up, Vector3.up);
            cachedTransform.rotation = Quaternion.Slerp(
                cachedTransform.rotation,
                upright,
                surfaceAlignmentSpeed * Time.deltaTime
            );
        }
    }

    // ===== PRIVATE METHODS =====

    private void FindNearestSurface()
    {
        Vector3 origin = cachedTransform.position;
        surfaceNormal = Vector3.up;
        onWalkableSurface = false;

        // Check downward (ground)
        RaycastHit groundHit;
        if (Physics.SphereCast(origin + Vector3.up * 0.1f, surfaceCheckRadius,
            Vector3.down, out groundHit, surfaceCheckDistance, walkableLayers))
        {
            float angle = Vector3.Angle(Vector3.up, groundHit.normal);
            if (angle <= maxSurfaceAngle)
            {
                surfaceNormal = groundHit.normal;
                onWalkableSurface = true;
                return;
            }
        }

        // Check in movement direction
        RaycastHit sideHit;
        Vector3 checkDir = cachedTransform.forward + cachedTransform.right * 0.5f;
        checkDir.Normalize();

        if (Physics.SphereCast(origin + checkDir * 0.1f, surfaceCheckRadius * 0.5f,
            checkDir, out sideHit, surfaceCheckDistance, walkableLayers))
        {
            float angle = Vector3.Angle(Vector3.up, sideHit.normal);
            if (angle <= maxSurfaceAngle)
            {
                surfaceNormal = sideHit.normal;
                onWalkableSurface = true;
                return;
            }
        }

        // Check ceiling (if enabled)
        if (canWalkOnCeiling)
        {
            RaycastHit ceilingHit;
            if (Physics.SphereCast(origin, surfaceCheckRadius,
                Vector3.up, out ceilingHit, ceilingCheckDistance, walkableLayers))
            {
                float angle = Vector3.Angle(Vector3.up, ceilingHit.normal);
                if (angle >= (180f - maxSurfaceAngle))
                {
                    surfaceNormal = ceilingHit.normal;
                    onWalkableSurface = true;
                    return;
                }
            }
        }
    }

    // ===== GIZMOS =====

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        // Surface detection sphere
        Vector3 origin = transform.position;
        Gizmos.DrawWireSphere(origin + Vector3.down * surfaceCheckDistance * 0.5f, surfaceCheckRadius);

        // Surface normal
        if (onWalkableSurface)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, surfaceNormal * 0.5f);
        }

        // Ceiling check
        if (canWalkOnCeiling)
        {
            Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.3f);
            Gizmos.DrawRay(transform.position, Vector3.up * ceilingCheckDistance);
        }
    }
}
