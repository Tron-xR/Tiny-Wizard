using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private float speedSmoothTime = 0.1f;

    private Animator animator;
    private float animationSpeed;
    private float speedVelocity;

    private void OnEnable()
    {
        animator = GetComponent<Animator>();
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;

        float speed = playerController != null ? playerController.GetCurrentSpeed() : 0f;
        animationSpeed = Mathf.SmoothDamp(animationSpeed, speed, ref speedVelocity, speedSmoothTime);

        animator.SetFloat("Speed", animationSpeed);
        animator.SetBool("IsGrounded", playerController != null && playerController.IsGrounded);
    }
}
