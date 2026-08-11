using UnityEngine;

namespace Platformer.KeyBinding
{
    /// <summary>
    /// Reusable trigger volume: blocks a configured set of physical keys while the player
    /// stands inside it, unblocking them on exit. Represents "special circumstances" -
    /// hazards, story beats, broken equipment - that take a key out of play and force the
    /// player to reallocate whatever action depended on it. For a permanent/one-shot block
    /// (e.g. a scripted event), call KeyBindingManager.Instance.SetKeyBlocked directly
    /// instead of using this component.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class KeyboardKeyBlocker : MonoBehaviour
    {
        [SerializeField] private KeyCode[] keysToBlock;
        [SerializeField] private string playerTag = "Player";

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            SetBlocked(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            SetBlocked(false);
        }

        private void SetBlocked(bool blocked)
        {
            var manager = KeyBindingManager.Instance;
            if (manager == null || keysToBlock == null) return;

            foreach (var key in keysToBlock)
            {
                manager.SetKeyBlocked(key, blocked);
            }
        }
    }
}
