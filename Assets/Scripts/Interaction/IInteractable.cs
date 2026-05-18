using UnityEngine;

/// <summary>
/// Interface for all interactable objects in the game.
/// Implement this to create custom interaction behaviours like pickups, levers, buttons, etc.
/// </summary>
public interface IInteractable
{
    /// <summary>Maximum distance the player can interact with this object from.</summary>
    float InteractionDistance { get; }

    /// <summary>Text displayed in the interaction prompt UI (e.g. "Pick up", "Push", "Open").</summary>
    string PromptText { get; }

    /// <summary>False to temporarily disable interaction (e.g. on cooldown, already used).</summary>
    bool CanInteract { get; }

    /// <summary>Transform used for distance checks and focus highlighting.</summary>
    Transform InteractionTransform { get; }

    /// <summary>Called when the player presses Interact while looking at this object.</summary>
    void OnInteract(InteractionController controller);

    /// <summary>Called when the player's crosshair first hits this object.</summary>
    void OnFocusEnter(InteractionController controller);

    /// <summary>Called when the player's crosshair moves away from this object.</summary>
    void OnFocusExit(InteractionController controller);
}
