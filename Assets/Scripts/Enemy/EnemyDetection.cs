using UnityEngine;

/// <summary>
/// Handles player detection for enemy AI.
/// 
/// Uses:
/// - Sphere-based detection radius
/// - Line of sight check (raycast to player)
/// - Optional field-of-view cone
/// - Chase memory timer (enemy keeps chasing briefly after losing sight)
/// 
/// Inspector Setup:
/// 1. Add this component to the enemy GameObject
/// 2. Set detection radius, chase range, and field of view
/// 3. Assign layers for line of sight obstruction
/// 4. (Optional) Create a DetectionPoint child Transform at eye level
/// </summary>
public class EnemyDetection : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float chaseRange = 12f;
    [SerializeField] private float fieldOfView = 180f; // degrees, 360 for full awareness
    [SerializeField] private float detectHeightOffset = 0.5f; // how high from enemy base

    [Header("Line of Sight")]
    [SerializeField] private bool requireLineOfSight = false;
    [SerializeField] private LayerMask obstacleLayers = -1;
    [SerializeField] private Transform detectionPoint; // optional: where to cast from

    [Header("Chase Memory")]
    [SerializeField] private float loseTargetDelay = 3f; // how long to chase after losing sight
    [SerializeField] private bool detectOnTakeDamage = true; // become aware when hit

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    // Events
    public System.Action OnPlayerDetected;
    public System.Action OnPlayerLost;

    // References
    private EnemyBase enemyBase;
    private Transform cachedTransform;

    // Runtime state
    private bool playerDetected = false;
    private bool playerInSight = false;
    private float lastSeenTime = 0f;
    private bool forceDetected = false;

    public bool PlayerDetected => playerDetected;
    public bool PlayerInSight => playerInSight;

    // ===== INITIALIZATION =====

    public void Initialize(EnemyBase enemy)
    {
        enemyBase = enemy;
        cachedTransform = transform;
    }

    // ===== PUBLIC API =====

    /// <summary>
    /// Check if player is detected. Call every frame from EnemyBase.Update().
    /// </summary>
    public void CheckDetection(Transform playerTransform)
    {
        if (playerTransform == null) return;

        // Distance check: is player within detection radius?
        float distToPlayer = Vector3.Distance(cachedTransform.position, playerTransform.position);

        // Line of sight check
        bool sightBlocked = false;
        if (requireLineOfSight)
        {
            sightBlocked = !HasLineOfSight(playerTransform);
        }

        // Field of view check
        bool inFOV = IsInFieldOfView(playerTransform);

        bool wasDetected = playerDetected;

        // Determine if player is detected right now
        bool currentlyInSight = distToPlayer <= detectionRadius && inFOV && !sightBlocked;
        bool inChaseRange = distToPlayer <= chaseRange;

        playerInSight = currentlyInSight;

        if (forceDetected)
        {
            // Force detection overrides everything for a brief moment
            playerDetected = true;
            lastSeenTime = Time.time;
            forceDetected = false;

            if (!wasDetected)
                OnPlayerDetected?.Invoke();
            return;
        }

        if (currentlyInSight)
        {
            // Player is visible
            lastSeenTime = Time.time;

            if (!playerDetected)
            {
                playerDetected = true;
                OnPlayerDetected?.Invoke();
            }
        }
        else if (playerDetected)
        {
            // Player was detected but is no longer visible
            // Chase memory: keep chasing for a while
            if (inChaseRange && (Time.time - lastSeenTime) < loseTargetDelay)
            {
                // Still in chase memory window — stay detected
            }
            else
            {
                // Lost the player
                playerDetected = false;
                OnPlayerLost?.Invoke();
            }
        }
    }

    /// <summary>
    /// Force detection of the player (e.g. when the enemy takes damage).
    /// </summary>
    public void ForceDetectPlayer()
    {
        forceDetected = true;
    }

    /// <summary>
    /// Check if player is within the chase range (used for lose-target check).
    /// </summary>
    public bool IsPlayerInChaseRange(Transform playerTransform)
    {
        if (playerTransform == null) return false;

        float dist = Vector3.Distance(cachedTransform.position, playerTransform.position);
        return dist <= chaseRange;
    }

    /// <summary>
    /// Check if player is within the initial detection radius.
    /// </summary>
    public bool IsPlayerInDetectionRange(Transform playerTransform)
    {
        if (playerTransform == null) return false;

        float dist = Vector3.Distance(cachedTransform.position, playerTransform.position);
        return dist <= detectionRadius;
    }

    // ===== PRIVATE METHODS =====

    private bool HasLineOfSight(Transform target)
    {
        // Cast from detection point (or enemy position with height offset)
        Vector3 origin = detectionPoint != null
            ? detectionPoint.position
            : cachedTransform.position + Vector3.up * detectHeightOffset;

        Vector3 targetPos = target.position + Vector3.up * 1f; // aim at player torso

        RaycastHit hit;
        if (Physics.Linecast(origin, targetPos, out hit, obstacleLayers))
        {
            // If we hit the player or something on the player layer, we have line of sight
            return hit.transform.root.CompareTag("Player");
        }

        // Nothing in the way — line of sight is clear
        return true;
    }

    private bool IsInFieldOfView(Transform target)
    {
        if (fieldOfView >= 360f) return true; // full awareness

        Vector3 directionToTarget = (target.position - cachedTransform.position).normalized;
        float angle = Vector3.Angle(cachedTransform.forward, directionToTarget);

        return angle <= fieldOfView * 0.5f; // FOV is split left/right
    }

    // ===== GIZMOS =====

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // Detection radius
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f); // yellow, semi-transparent
        Gizmos.DrawSphere(transform.position, detectionRadius);

        // Chase range
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.1f); // orange, semi-transparent
        Gizmos.DrawSphere(transform.position, chaseRange);

        // Field of view cone
        if (fieldOfView < 360f)
        {
            Gizmos.color = Color.yellow;
            Vector3 forward = transform.forward * detectionRadius;
            float halfFOV = fieldOfView * 0.5f;

            Quaternion leftRay = Quaternion.Euler(0, -halfFOV, 0);
            Quaternion rightRay = Quaternion.Euler(0, halfFOV, 0);

            Gizmos.DrawRay(transform.position, leftRay * forward);
            Gizmos.DrawRay(transform.position, rightRay * forward);
        }

        // Detection point
        if (detectionPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(detectionPoint.position, 0.1f);
        }
    }
}
