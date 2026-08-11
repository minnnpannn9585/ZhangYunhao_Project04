using UnityEngine;

namespace Platformer.Interaction
{
    /// <summary>
    /// Simple map interaction demo: toggles a target Door when the player interacts with it.
    /// </summary>
    public class Lever : MonoBehaviour, IInteractable
    {
        [SerializeField] private Door targetDoor;

        public void Interact(GameObject interactor)
        {
            if (targetDoor != null) targetDoor.Toggle();
        }
    }
}
