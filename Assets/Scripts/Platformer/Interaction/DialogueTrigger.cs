using UnityEngine;
using Platformer.Dialogue;

namespace Platformer.Interaction
{
    /// <summary>
    /// Placed on an NPC or sign: each Interact press shows the next line of the attached
    /// DialogueData through DialogueUIController.
    /// </summary>
    public class DialogueTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueData dialogue;

        public void Interact(GameObject interactor)
        {
            DialogueUIController.Instance?.Advance(dialogue);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                DialogueUIController.Instance?.EndConversation(dialogue);
            }
        }
    }
}
