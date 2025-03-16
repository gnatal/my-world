using UnityEngine;
using System.Collections.Generic;

public class MegamanController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    // Movement parameters
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    // Animation arrays
    [Header("Animation Sprites")]
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] runFrames;
    [SerializeField] private Sprite[] jumpFrames;
    [SerializeField] private Sprite[] fallFrames;
    [SerializeField] private Sprite[] shootFrames;
    [SerializeField] private Sprite[] runShootFrames;
    [SerializeField] private Sprite[] jumpShootFrames;
    [SerializeField] private Sprite[] dashFrames;
    [SerializeField] private Sprite[] climbFrames;
    [SerializeField] private Sprite[] hurtFrames;

    // Animation parameters
    [Header("Animation Settings")]
    [SerializeField] private float frameRate = 10f;
    private float frameTimer = 0f;
    private int currentFrame = 0;

    // State tracking
    private string currentAnimation = "idle";
    private bool isGrounded = false;
    private bool isShooting = false;
    private bool isDashing = false;
    private bool isClimbing = false;
    private bool isOneShot = false;

    // Ground check
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        currentAnimation = "idle";

        if (idleFrames.Length > 0)
            spriteRenderer.sprite = idleFrames[0];
    }

    void Update()
    {
        CheckGrounded();
        HandleInput();
        UpdateAnimation();
    }

    void CheckGrounded()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        else
        {
            isGrounded = Physics2D.Raycast(
                transform.position,
                Vector2.down,
                1.1f,
                groundLayer
            );
        }
    }

    void HandleInput()
    {
        // Horizontal movement
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput != 0 && !isDashing && !isClimbing)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
            spriteRenderer.flipX = moveInput < 0;

            if (isGrounded && !isShooting)
            {
                SetAnimation("run");
            }
            else if (isGrounded && isShooting)
            {
                SetAnimation("runShoot");
            }
        }
        else if (isGrounded && !isDashing && !isClimbing && !isShooting)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            SetAnimation("idle");
        }

        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isClimbing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            SetAnimation(isShooting ? "jumpShoot" : "jump", true);
        }

        // Falling
        if (!isGrounded && rb.linearVelocity.y < 0 && !isClimbing)
        {
            SetAnimation(isShooting ? "jumpShoot" : "fall");
        }

        // Shooting
        if (Input.GetKeyDown(KeyCode.X) && !isDashing)
        {
            isShooting = true;

            if (isGrounded && Mathf.Abs(rb.linearVelocity.x) < 0.1f)
            {
                SetAnimation("shoot", true, true);
            }
            else if (isGrounded)
            {
                SetAnimation("runShoot");
            }
            else
            {
                SetAnimation("jumpShoot");
            }

            // Reset shooting after delay
            Invoke("ResetShooting", 0.5f);
        }

        // Dashing
        if (Input.GetKeyDown(KeyCode.Z) && !isDashing && !isClimbing)
        {
            StartDash();
        }
    }

    void StartDash()
    {
        isDashing = true;
        SetAnimation("dash", true, true);

        // Apply dash force
        float dashDirection = spriteRenderer.flipX ? -1 : 1;
        rb.linearVelocity = new Vector2(dashDirection * moveSpeed * 2, 0);

        // End dash after delay
        Invoke("EndDash", 0.3f);
    }

    void EndDash()
    {
        isDashing = false;
    }

    void ResetShooting()
    {
        isShooting = false;
    }

    void UpdateAnimation()
    {
        Sprite[] currentFrames = GetCurrentFrames();
        if (currentFrames == null || currentFrames.Length == 0) return;

        frameTimer += Time.deltaTime;

        if (frameTimer >= 1f / frameRate)
        {
            frameTimer = 0f;
            currentFrame++;

            if (isOneShot && currentFrame >= currentFrames.Length)
            {
                OnAnimationComplete();
            }
            else
            {
                currentFrame %= currentFrames.Length;
            }

            spriteRenderer.sprite = currentFrames[currentFrame];
        }
    }

    Sprite[] GetCurrentFrames()
    {
        switch (currentAnimation)
        {
            case "idle": return idleFrames;
            case "run": return runFrames;
            case "jump": return jumpFrames;
            case "fall": return fallFrames;
            case "shoot": return shootFrames;
            case "runShoot": return runShootFrames;
            case "jumpShoot": return jumpShootFrames;
            case "dash": return dashFrames;
            case "climb": return climbFrames;
            case "hurt": return hurtFrames;
            default: return idleFrames;
        }
    }

    public void SetAnimation(string animationName, bool forceChange = false, bool oneShot = false)
    {
        // Check animation state priorities
        if (!forceChange)
        {
            if (isDashing) return;
            if (isClimbing && animationName != "climb") return;
        }

        if (currentAnimation != animationName || forceChange)
        {
            currentAnimation = animationName;
            currentFrame = 0;
            frameTimer = 0f;
            isOneShot = oneShot;
        }
    }

    void OnAnimationComplete()
    {
        isOneShot = false;

        // Handle returning to idle after one-shot animations
        if (currentAnimation == "shoot")
        {
            isShooting = false;
            SetAnimation("idle");
        }
        else if (currentAnimation == "dash")
        {
            isDashing = false;
            SetAnimation("idle");
        }
        else if (currentAnimation == "hurt")
        {
            SetAnimation("idle");
        }
    }

    public void TakeDamage()
    {
        SetAnimation("hurt", true, true);
    }

    // Draw gizmos for ground check
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}