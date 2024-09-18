using UnityEngine;

public class MegamanController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    // Movement parameters
    public float walkSpeed = 5f;
    public float dashSpeed = 10f;
    public float jumpForce = 10f;
    private bool isDashing = false;
    private bool isJumping = false;
    private bool isShooting = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        HandleMovement();
        HandleActions();
        HandleIdleState();
    }

    void HandleMovement()
    {
        float moveX = 0;
        float moveY = 0;

        // Walking with WASD or Arrow keys
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            moveY = 1;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            moveY = -1;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveX = -1;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveX = 1;

        Vector2 movement = new Vector2(moveX, moveY).normalized * walkSpeed;
        rb.velocity = new Vector2(movement.x, rb.velocity.y); // Move horizontally only

        // Set walking animation
        if (movement != Vector2.zero && !isDashing)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        // Flip sprite depending on the direction
        if (moveX > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveX < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void HandleActions()
    {
        // Dash with L
        if (Input.GetKeyDown(KeyCode.L) && !isDashing)
        {
            isDashing = true;
            animator.SetTrigger("Dash");
            rb.velocity = new Vector2(transform.localScale.x * dashSpeed, rb.velocity.y);
            Invoke("ResetDash", 0.5f); // Reset dash state after 0.5 seconds
        }

        // Shoot with J
        if (Input.GetKeyDown(KeyCode.J) && !isShooting)
        {
            isShooting = true;
            animator.SetTrigger("Shoot");
            Invoke("ResetShoot", 0.5f); // Reset shooting state after 0.5 seconds
        }

        // Jump with K
        if (Input.GetKeyDown(KeyCode.K) && !isJumping)
        {
            isJumping = true;
            animator.SetBool("isJumping", true);
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    void HandleIdleState()
    {
        if (rb.velocity.x == 0 && !isShooting && !isJumping && !isDashing)
        {
            animator.SetBool("isIdle", true);
        }
        else
        {
            animator.SetBool("isIdle", false);
        }
    }

    private void ResetDash()
    {
        isDashing = false;
    }

    private void ResetShoot()
    {
        isShooting = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Reset jumping state when landing
        if (collision.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
            animator.SetBool("isJumping", false);
        }
    }
}
