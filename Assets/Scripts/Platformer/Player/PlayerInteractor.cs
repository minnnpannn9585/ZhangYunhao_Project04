using System.Collections.Generic;
using UnityEngine;
using Platformer.Interaction;

namespace Platformer.Player
{
    /// <summary>
    /// Tracks IInteractable objects the player is currently overlapping (via a dedicated
    /// trigger collider, separate from the player's solid physics collider) and exposes the
    /// closest one for PlayerController to fire on the Interact action.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        private readonly List<IInteractable> inRange = new List<IInteractable>();

        public IInteractable Current => inRange.Count > 0 ? inRange[0] : null;

        public void InteractWithCurrent()
        {
            Current?.Interact(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var interactable = other.GetComponent<IInteractable>();
            if (interactable != null && !inRange.Contains(interactable))
            {
                inRange.Add(interactable);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var interactable = other.GetComponent<IInteractable>();
            if (interactable != null)
            {
                inRange.Remove(interactable);
            }
        }
    }
}
