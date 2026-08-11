using UnityEngine;

namespace Platformer.Interaction
{
    /// <summary>
    /// Anything the player can trigger with the Interact action: levers, signs, dialogue
    /// starters, pickups. Implementations should stay short - complex behaviour belongs in
    /// whatever the interactable calls out to.
    /// </summary>
    public interface IInteractable
    {
        void Interact(GameObject interactor);
    }
}
