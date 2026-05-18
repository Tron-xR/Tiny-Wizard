using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player input using the NEW Unity Input System.
/// Integrates with PlayerInput component and exposes input values to PlayerController.
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    
    // Input action references
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction lookAction;

    // Input state
    private Vector2 moveInput;
    private bool jumpInputPressed;
    private bool sprintInputPressed;
    private Vector2 lookInput;

    /// <summary>
    /// Returns the current movement input (WASD).
    /// Normalized to [-1, 1] range.
    /// </summary>
    public Vector2 MoveInput => moveInput;

    /// <summary>
    /// Returns true if jump was pressed this frame.
    /// Automatically consumed after being read.
    /// </summary>
    public bool JumpPressed
    {
        get
        {
            bool pressed = jumpInputPressed;
            jumpInputPressed = false; // Consume input
            return pressed;
        }
    }

    /// <summary>
    /// Returns true while sprint button is held.
    /// </summary>
    public bool SprintPressed => sprintInputPressed;

    /// <summary>
    /// Returns the current look input from mouse/gamepad.
    /// </summary>
    public Vector2 LookInput => lookInput;

    private void OnEnable()
    {
        // Get reference to PlayerInput if not assigned
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        // Cache action references for better performance
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];
        lookAction = playerInput.actions["Look"];

        // Subscribe to input events
        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;
        
        jumpAction.performed += OnJumpPerformed;
        
        sprintAction.performed += OnSprintPerformed;
        sprintAction.canceled += OnSprintCanceled;
        
        lookAction.performed += OnLookPerformed;
        lookAction.canceled += OnLookCanceled;
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
        }

        if (jumpAction != null)
            jumpAction.performed -= OnJumpPerformed;

        if (sprintAction != null)
        {
            sprintAction.performed -= OnSprintPerformed;
            sprintAction.canceled -= OnSprintCanceled;
        }

        if (lookAction != null)
        {
            lookAction.performed -= OnLookPerformed;
            lookAction.canceled -= OnLookCanceled;
        }
    }

    /// <summary>
    /// Called when movement input is detected.
    /// </summary>
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Called when movement input is released.
    /// </summary>
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    /// <summary>
    /// Called when jump is pressed.
    /// </summary>
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpInputPressed = true;
    }

    /// <summary>
    /// Called when sprint is pressed.
    /// </summary>
    private void OnSprintPerformed(InputAction.CallbackContext context)
    {
        sprintInputPressed = true;
    }

    /// <summary>
    /// Called when sprint is released.
    /// </summary>
    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        sprintInputPressed = false;
    }

    /// <summary>
    /// Called when look input is detected.
    /// </summary>
    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Called when look input is released.
    /// </summary>
    private void OnLookCanceled(InputAction.CallbackContext context)
    {
        lookInput = Vector2.zero;
    }
}
