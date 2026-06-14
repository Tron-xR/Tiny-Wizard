using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float defaultDuration = 0.2f;
    [SerializeField] private float defaultMagnitude = 0.15f;

    public static CameraShake Instance { get; private set; }

    private Vector3 originalLocalPosition;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        originalLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (shakeDuration > 0f)
        {
            Vector3 offset = Random.insideUnitSphere * shakeMagnitude;
            transform.localPosition = originalLocalPosition + offset;
            shakeDuration -= Time.deltaTime;
        }
        else
        {
            transform.localPosition = originalLocalPosition;
        }
    }

    public void Shake()
    {
        Shake(defaultDuration, defaultMagnitude);
    }

    public void Shake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }
}
