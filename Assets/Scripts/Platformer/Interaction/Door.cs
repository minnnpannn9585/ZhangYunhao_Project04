using UnityEngine;

namespace Platformer.Interaction
{
    /// <summary>
    /// A blocking collider that can be opened/closed by a Lever (or any other script that
    /// calls SetOpen/Toggle) - a basic demo of character/map interaction beyond simple
    /// solid collision.
    /// </summary>
    public class Door : MonoBehaviour
    {
        [SerializeField] private bool startOpen;

        private Collider2D blockingCollider;
        private SpriteRenderer spriteRenderer;
        private bool isOpen;

        private void Awake()
        {
            blockingCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            SetOpen(startOpen);
        }

        public void Toggle() => SetOpen(!isOpen);

        public void SetOpen(bool open)
        {
            isOpen = open;
            if (blockingCollider != null) blockingCollider.enabled = !open;
            if (spriteRenderer != null)
            {
                var c = spriteRenderer.color;
                spriteRenderer.color = open ? new Color(c.r, c.g, c.b, 0.25f) : new Color(c.r, c.g, c.b, 1f);
            }
        }
    }
}
