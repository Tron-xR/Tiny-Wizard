using UnityEngine;

/// <summary>
/// Base class for all interactable objects.
/// Extend this to create specific interaction types (PickupObject, PushableObject, Lever, etc.).
/// 
/// Inspector defaults:
/// - Interaction Distance: 3 (how close the player must be)
/// - Prompt Text: "Interact" (shown in UI when looking at this object)
/// - Collider: make sure this object has a Collider (trigger recommended for non-physics objects)
/// - Layer: assign to a dedicated "Interactable" layer for raycast filtering
/// </summary>
public abstract class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] protected float interactionDistance = 3f;
    [SerializeField] protected string promptText = "Interact";
    [SerializeField] protected bool canInteract = true;

    // ===== IINTERACTABLE PROPERTIES =====

    public float InteractionDistance => interactionDistance;
    public string PromptText => promptText;
    public bool CanInteract => canInteract;
    public Transform InteractionTransform => transform;

    // ===== FOCUS EVENTS =====

    /// <summary>
    /// Called when the player first looks at this object.
    /// Override to play sounds, show highlights, etc.
    /// </summary>
    public virtual void OnFocusEnter(InteractionController controller) { }

    /// <summary>
    /// Called when the player looks away from this object.
    /// Override to remove highlights, stop sounds, etc.
    /// </summary>
    public virtual void OnFocusExit(InteractionController controller) { }

    /// <summary>
    /// Called when the player presses Interact while looking at this object.
    /// Each derived class implements its own behaviour.
    /// </summary>
    public abstract void OnInteract(InteractionController controller);

    // ===== PUBLIC HELPERS =====

    /// <summary>
    /// Enable or disable interaction without affecting other components.
    /// </summary>
    public void SetInteractable(bool value)
    {
        canInteract = value;
    }

    /// <summary>
    /// Override the prompt text at runtime (e.g. after picking up an item).
    /// </summary>
    public void SetPromptText(string text)
    {
        promptText = text;
    }
}
