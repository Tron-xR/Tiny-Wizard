using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Central input handler using the NEW Unity Input System.
/// Exposes input through properties and events.
/// Gameplay systems should NEVER read InputActions directly - use this class only.
/// All controls can be remapped in the Input Actions asset.
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Actions Asset")]
    [SerializeField] private PlayerInput playerInput;

    // Cached action references
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction interactAction;
    private InputAction castSpellAction;
    private InputAction pauseAction;
    private InputAction zoomAction;
    private InputAction spellSlot1Action;
    private InputAction spellSlot2Action;
    private InputAction spellSlot3Action;

    // ===== CONTINUOUS INPUT PROPERTIES =====
    // Gameplay systems read these every frame

    /// <summary>WASD / left stick movement input. Normalized Vector2 (-1 to 1).</summary>
    public Vector2 MoveInput { get; private set; }

    /// <summary>Mouse delta / right stick look input.</summary>
    public Vector2 LookInput { get; private set; }

    /// <summary>True while sprint button is held.</summary>
    public bool IsSprinting { get; private set; }

    /// <summary>Mouse scroll wheel input (Y axis).</summary>
    public float ZoomInput { get; private set; }

    // ===== ONE-SHOT ACTION EVENTS =====
    // Gameplay systems subscribe to these via Inspector or code

    [Header("Input Events")]
    public UnityEvent OnJump;
    public UnityEvent OnInteract;
    public UnityEvent OnCastSpell;
    public UnityEvent OnPause;

    // Code-based event for systems that prefer it over UnityEvents
    public System.Action JumpPressed;
    public System.Action InteractPressed;
    public System.Action CastSpellPressed;
    public System.Action PausePressed;
    public System.Action<int> SpellSlotPressed;

    // Polled jump flag (consumed once per press — more reliable than events for physics)
    private bool jumpRequested = false;

    private void OnEnable()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogWarning("PlayerInputHandler: No Input Actions asset assigned to PlayerInput. Movement will not work.", this);
            return;
        }

        CacheActions();
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void CacheActions()
    {
        var actions = playerInput.actions;
        moveAction = actions["Move"];
        lookAction = actions["Look"];
        jumpAction = actions["Jump"];
        sprintAction = actions["Sprint"];
        interactAction = actions["Interact"];
        castSpellAction = actions["CastSpell"];
        pauseAction = actions["Pause"];
        zoomAction = actions["Zoom"];
        spellSlot1Action = actions["SpellSlot1"];
        spellSlot2Action = actions["SpellSlot2"];
        spellSlot3Action = actions["SpellSlot3"];
    }

    private void SubscribeToEvents()
    {
        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;

        lookAction.performed += OnLookPerformed;
        lookAction.canceled += OnLookCanceled;

        sprintAction.performed += OnSprintStarted;
        sprintAction.canceled += OnSprintCanceled;

        jumpAction.performed += OnJumpPerformed;
        interactAction.performed += OnInteractPerformed;
        castSpellAction.performed += OnCastSpellPerformed;
        pauseAction.performed += OnPausePerformed;

        spellSlot1Action.performed += OnSpellSlot1Performed;
        spellSlot2Action.performed += OnSpellSlot2Performed;
        spellSlot3Action.performed += OnSpellSlot3Performed;

        zoomAction.performed += OnZoomPerformed;
        zoomAction.canceled += OnZoomCanceled;
    }

    private void UnsubscribeFromEvents()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
        }

        if (lookAction != null)
        {
            lookAction.performed -= OnLookPerformed;
            lookAction.canceled -= OnLookCanceled;
        }

        if (sprintAction != null)
        {
            sprintAction.performed -= OnSprintStarted;
            sprintAction.canceled -= OnSprintCanceled;
        }

        if (jumpAction != null) jumpAction.performed -= OnJumpPerformed;
        if (interactAction != null) interactAction.performed -= OnInteractPerformed;
        if (castSpellAction != null) castSpellAction.performed -= OnCastSpellPerformed;
        if (pauseAction != null) pauseAction.performed -= OnPausePerformed;

        if (spellSlot1Action != null) spellSlot1Action.performed -= OnSpellSlot1Performed;
        if (spellSlot2Action != null) spellSlot2Action.performed -= OnSpellSlot2Performed;
        if (spellSlot3Action != null) spellSlot3Action.performed -= OnSpellSlot3Performed;

        if (zoomAction != null)
        {
            zoomAction.performed -= OnZoomPerformed;
            zoomAction.canceled -= OnZoomCanceled;
        }
    }

    // ===== INPUT EVENT HANDLERS =====

    private void OnMovePerformed(InputAction.CallbackContext context) => MoveInput = context.ReadValue<Vector2>();
    private void OnMoveCanceled(InputAction.CallbackContext context) => MoveInput = Vector2.zero;

    private void OnLookPerformed(InputAction.CallbackContext context) => LookInput = context.ReadValue<Vector2>();
    private void OnLookCanceled(InputAction.CallbackContext context) => LookInput = Vector2.zero;

    private void OnSprintStarted(InputAction.CallbackContext context) => IsSprinting = true;
    private void OnSprintCanceled(InputAction.CallbackContext context) => IsSprinting = false;

    /// <summary>
    /// Returns true once per jump press. Call from FixedUpdate.
    /// More reliable than event subscription for physics-timed actions.
    /// </summary>
    public bool ConsumeJump()
    {
        if (jumpRequested)
        {
            Debug.Log("<color=green>InputHandler: Jump consumed!</color>");
            jumpRequested = false;
            return true;
        }
        return false;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("<color=cyan>InputHandler: Jump performed!</color>");
        jumpRequested = true;
        OnJump?.Invoke();
        JumpPressed?.Invoke();
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        OnInteract?.Invoke();
        InteractPressed?.Invoke();
    }

    private void OnCastSpellPerformed(InputAction.CallbackContext context)
    {
        OnCastSpell?.Invoke();
        CastSpellPressed?.Invoke();
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        OnPause?.Invoke();
        PausePressed?.Invoke();
    }

    private void OnSpellSlot1Performed(InputAction.CallbackContext context) => SpellSlotPressed?.Invoke(0);
    private void OnSpellSlot2Performed(InputAction.CallbackContext context) => SpellSlotPressed?.Invoke(1);
    private void OnSpellSlot3Performed(InputAction.CallbackContext context) => SpellSlotPressed?.Invoke(2);

    private void OnZoomPerformed(InputAction.CallbackContext context) => ZoomInput = context.ReadValue<float>();
    private void OnZoomCanceled(InputAction.CallbackContext context) => ZoomInput = 0f;
}
