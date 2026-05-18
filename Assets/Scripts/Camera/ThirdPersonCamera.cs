using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("References — Drag these in Inspector")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private PlayerInputHandler inputHandler;

    [Header("Distance & Height")]
    [SerializeField] private float defaultDistance = 4f;
    [SerializeField] private float height = 0.5f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 8f;

    [Header("Orbit")]
    [SerializeField] private Vector2 sensitivity = new Vector2(4f, 2f);
    [SerializeField] private float verticalMinAngle = -20f;
    [SerializeField] private float verticalMaxAngle = 60f;
    [SerializeField] private bool invertY = false;

    [Header("Smoothing")]
    [SerializeField] private float followSmoothTime = 0.15f;
    [SerializeField] private float zoomSmoothTime = 0.2f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 5f;

    [Header("Collision")]
    [SerializeField] private LayerMask obstacleLayers = -1;
    [SerializeField] private float collisionRadius = 0.3f;
    [SerializeField] private float collisionSmoothTime = 0.1f;

    private float yaw = 0f;
    private float pitch = 0f;
    private float currentDistance;
    private float targetDistance;
    private Vector3 followVelocity = Vector3.zero;
    private float distanceVelocity = 0f;
    private float cameraNear = 0f;

    private void OnEnable()
    {
        TryFindReferences();
    }

    private void Start()
    {
        if (target == null || cameraPivot == null || inputHandler == null)
            TryFindReferences();
    }

    private void TryFindReferences()
    {
        if (inputHandler == null)
            inputHandler = FindFirstObjectByType<PlayerInputHandler>();

        if (cameraPivot == null)
        {
            GameObject pivot = GameObject.Find("CameraPivot");
            if (pivot != null) cameraPivot = pivot.transform;
        }

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player != null)
            {
                Transform ct = player.transform.Find("CameraTarget");
                if (ct == null) ct = FindDeepChild(player.transform, "CameraTarget");
                if (ct != null) target = ct;
            }
            if (target == null)
            {
                GameObject ct = GameObject.Find("CameraTarget");
                if (ct != null) target = ct.transform;
            }
        }

        Camera cam = GetComponent<Camera>();
        if (cam != null) cameraNear = cam.nearClipPlane;

        currentDistance = defaultDistance;
        targetDistance = defaultDistance;

        if (target != null && cameraPivot != null)
            cameraPivot.position = target.position + Vector3.up * height;

        if (inputHandler == null)
            Debug.LogWarning("Camera: No PlayerInputHandler found. Add one to the Player.");
        if (cameraPivot == null)
            Debug.LogWarning("Camera: No CameraPivot assigned. Create an empty child under Main Camera called 'CameraPivot' and drag it here.");
        if (target == null)
            Debug.LogWarning("Camera: No target assigned. Create 'CameraTarget' child under Player and drag it here.");
    }

    private void LateUpdate()
    {
        if (target == null || cameraPivot == null) return;

        HandleInput();
        FollowTarget();
        HandleCollision();
        ApplyCameraTransform();
    }

    private void HandleInput()
    {
        if (inputHandler == null) return;

        Vector2 look = inputHandler.LookInput;

        yaw += look.x * sensitivity.x * Time.deltaTime;
        pitch += look.y * sensitivity.y * Time.deltaTime * (invertY ? 1f : -1f);
        pitch = Mathf.Clamp(pitch, verticalMinAngle, verticalMaxAngle);

        float scroll = inputHandler.ZoomInput;
        targetDistance -= scroll * zoomSpeed;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref distanceVelocity, zoomSmoothTime);
    }

    private void FollowTarget()
    {
        if (target == null || cameraPivot == null) return;

        Vector3 targetPos = target.position + Vector3.up * height;
        cameraPivot.position = Vector3.SmoothDamp(cameraPivot.position, targetPos, ref followVelocity, followSmoothTime);
    }

    private void HandleCollision()
    {
        if (target == null || cameraPivot == null) return;

        Vector3 dir = (transform.position - cameraPivot.position).normalized;

        if (Physics.SphereCast(cameraPivot.position, collisionRadius, dir, out RaycastHit hit, currentDistance, obstacleLayers))
        {
            float hitDist = Vector3.Distance(cameraPivot.position, hit.point) - collisionRadius - cameraNear;
            currentDistance = Mathf.Lerp(currentDistance, Mathf.Max(hitDist, 0.1f), Time.deltaTime / (collisionSmoothTime + 0.001f));
        }
    }

    private void ApplyCameraTransform()
    {
        if (cameraPivot == null) return;

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pos = cameraPivot.position - (rot * Vector3.forward * currentDistance);
        transform.position = pos;
        transform.LookAt(cameraPivot.position);
    }

    private static Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }

    public void SetTarget(Transform newTarget) => target = newTarget;

    public void TeleportToTarget()
    {
        if (target == null || cameraPivot == null) return;
        cameraPivot.position = target.position + Vector3.up * height;
        currentDistance = targetDistance;
        followVelocity = Vector3.zero;
    }
}
