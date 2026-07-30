using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;

    [Header("Key Bindings")]
    [SerializeField] private KeyCode moveLeftKey = KeyCode.A;
    [SerializeField] private KeyCode moveRightKey = KeyCode.D;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool facingRight = true;

    public KeyCode MoveLeftKey => moveLeftKey;
    public KeyCode MoveRightKey => moveRightKey;
    public KeyCode JumpKey => jumpKey;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        moveInput = GetMoveInput();

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        if (moveInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && facingRight)
        {
            Flip();
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    private float GetMoveInput()
    {
        bool moveLeft = Input.GetKey(moveLeftKey);
        bool moveRight = Input.GetKey(moveRightKey);

        if (moveLeft == moveRight)
        {
            return 0f;
        }

        return moveRight ? 1f : -1f;
    }

    public bool IsKeyUsed(KeyCode key)
    {
        return moveLeftKey == key || moveRightKey == key || jumpKey == key;
    }

    public bool TryRebindKey(KeyCode oldKey, KeyCode newKey)
    {
        if (oldKey == newKey || IsKeyUsed(newKey))
        {
            return false;
        }

        if (moveLeftKey == oldKey)
        {
            moveLeftKey = newKey;
            return true;
        }

        if (moveRightKey == oldKey)
        {
            moveRightKey = newKey;
            return true;
        }

        if (jumpKey == oldKey)
        {
            jumpKey = newKey;
            return true;
        }

        return false;
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
