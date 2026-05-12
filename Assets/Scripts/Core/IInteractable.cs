using UnityEngine;

/// <summary>
/// Interface for all interactable objects in the game.
/// Implement this on any object the player can interact with.
/// </summary>
public interface IInteractable
{
    /// <summary>The prompt text shown to the player (e.g., "Talk to Noa", "Pick up Note")</summary>
    string InteractionPrompt { get; }
    
    /// <summary>Whether this object can currently be interacted with</summary>
    bool CanInteract { get; }
    
    /// <summary>Called when the player interacts with this object</summary>
    void Interact();
}
