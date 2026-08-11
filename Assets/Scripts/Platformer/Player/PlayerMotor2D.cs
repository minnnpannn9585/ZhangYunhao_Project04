using UnityEngine;

namespace Platformer.Player
{
    /// <summary>
    /// Pure physics/movement layer for the player: horizontal velocity, jumping and ground
    /// detection. Knows nothing about input or key bindings - PlayerController drives it -
    /// which keeps it reusable if the game ever needs an AI-controlled actor with the same
    /// movement feel.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMotor2D : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 9f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.15f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float lowJumpGravityMultiplier = 2.5f;

        private Rigidbody2D body;
        private float defaultGravityScale;

        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            defaultGravityScale = body.gravityScale;
        }

        private void FixedUpdate()
        {
            IsGrounded = groundCheck != null &&
                Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            // Falling faster than rising gives jumps a snappier, less floaty arc.
            body.gravityScale = body.velocity.y < 0f
                ? defaultGravityScale * lowJumpGravityMultiplier
                : defaultGravityScale;
        }

        public void Move(float horizontal)
        {
            body.velocity = new Vector2(horizontal * moveSpeed, body.velocity.y);
        }

        public void Jump()
        {
            if (!IsGrounded) return;
            body.velocity = new Vector2(body.velocity.x, jumpForce);
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
