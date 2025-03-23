using UnityEngine;
using System.Collections.Generic;

public class MegamanController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private float originalGravityScale = 1f;

    // Movement parameters
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float dashSpeed = 10f; // Separate dash speed variable

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
    private bool isPlayingOneShot = false;
    private float lastJumpTime = -1f;
    private float lastDashTime = -1f;
    private bool hasAirDashed = false; // Track if player has used air dash

    // Ground check
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        originalGravityScale = rb.gravityScale;
        currentAnimation = "idle";

        if (idleFrames.Length > 0)
            spriteRenderer.sprite = idleFrames[0];
    }

    void Update()
    {
        // Maintain horizontal-only movement during dash
        if (isDashing)
        {
            float dashDirection = spriteRenderer.flipX ? -1 : 1;
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0);
        }
        
        CheckGrounded();
        HandleInput();
        UpdateAnimation();
    }

    void CheckGrounded()
    {
        // Skip ground check briefly after jumping
        if (Time.time < lastJumpTime + 0.1f)
        {
            isGrounded = false;
            return;
        }
        
        bool linearVelocityCheck = Mathf.Abs(rb.linearVelocity.y) < 0.1f || rb.linearVelocity.y < 0;
        bool physicalGroundContact = false;

        if (groundCheck != null)
        {
            physicalGroundContact = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckRadius, physicalGroundContact ? Color.green : Color.red);
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position - new Vector3(0, 0.5f, 0),
                Vector2.down,
                0.6f,
                groundLayer
            );

            physicalGroundContact = hit.collider != null;
            Debug.DrawRay(transform.position - new Vector3(0, 0.5f, 0), Vector2.down * 0.6f, physicalGroundContact ? Color.green : Color.red);
        }

        // Player is grounded if they're touching ground AND their Y velocity is appropriate
        bool wasGrounded = isGrounded;
        isGrounded = physicalGroundContact && linearVelocityCheck;
        
        // Reset air dash when landing
        if (!wasGrounded && isGrounded)
        {
            hasAirDashed = false;
        }
    }

    void HandleInput()
    {
        // Allow specific inputs even during one-shot animations
        bool canJump = isGrounded && !isClimbing;
        bool canDash = !isClimbing && (Time.time > lastDashTime + 0.4f) && (!hasAirDashed || isGrounded);
        
        // Movement handling
        if (!isPlayingOneShot || isDashing)
        {
            float moveInput = Input.GetAxisRaw("Horizontal");

            if (moveInput != 0 && !isClimbing)
            {
                // Don't override dash velocity with normal movement
                if (!isDashing)
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
                else
                {
                    // Just update the sprite direction while dashing
                    spriteRenderer.flipX = moveInput < 0;
                }
            }
            else if (isGrounded && !isDashing && !isClimbing && !isShooting)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                SetAnimation("idle");
            }
        }

        // Jump handling (can now jump during dash)
        if (Input.GetKeyDown(KeyCode.K) && canJump)
        {
            Jump();
        }

        // Fall animation - only change if not playing a one-shot animation
        if (!isGrounded && rb.linearVelocity.y < 0 && !isClimbing && !isDashing && !isPlayingOneShot)
        {
            SetAnimation(isShooting ? "jumpShoot" : "fall");
        }

        // Shooting
        if (Input.GetKeyDown(KeyCode.J) && !isDashing)
        {
            isShooting = true;

            if (isGrounded && Mathf.Abs(rb.linearVelocity.x) < 0.1f)
            {
                SetAnimation("shoot", true, true);
                isPlayingOneShot = true;
            }
            else if (isGrounded)
            {
                SetAnimation("runShoot");
            }
            else if (!isGrounded)
            {
                // For shooting while jumping or falling
                if (rb.linearVelocity.y > 0 || currentAnimation == "jump")
                {
                    SetAnimation("jumpShoot", true, false);
                }
                else if (rb.linearVelocity.y < 0 || currentAnimation == "fall")
                {
                    SetAnimation("jumpShoot", true, false);
                }
            }

            Invoke("ResetShooting", 0.5f);
        }

        // Dash handling (can now dash in the air)
        if (Input.GetKeyDown(KeyCode.L) && canDash)
        {
            StartDash();
        }
    }

    void Jump()
    {
        lastJumpTime = Time.time;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        
        // If we're dashing, we interrupt it
        if (isDashing)
        {
            CancelInvoke("EndDash");
            isDashing = false;
            
            // Make sure to restore gravity if we interrupt a dash with a jump
            rb.gravityScale = originalGravityScale;
        }
        
        // Set jump or jumpShoot animation based on shooting state
        if (isShooting)
        {
            SetAnimation("jumpShoot", true, true); 
        }
        else 
        {
            SetAnimation("jump", true, true);
        }
        
        isPlayingOneShot = true;
    }

    void StartDash()
    {
        isDashing = true;
        lastDashTime = Time.time;
        
        // Track air dash
        if (!isGrounded)
        {
            hasAirDashed = true;
        }
        
        // We always force dash animation since it's a priority
        SetAnimation("dash", true, true);
        isPlayingOneShot = true;

        float dashDirection = spriteRenderer.flipX ? -1 : 1;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0); // Zero out vertical velocity for consistent dash
        
        // Disable gravity during dash
        rb.gravityScale = 0;

        CancelInvoke("EndDash"); // Cancel any previous dash that might be ending
        Invoke("EndDash", 0.3f);
    }

    void EndDash()
    {
        isDashing = false;
        isPlayingOneShot = false;
        
        // Restore normal gravity
        rb.gravityScale = originalGravityScale;
        
        // Transition to appropriate animation
        if (isGrounded)
        {
            SetAnimation(isShooting ? "shoot" : "idle");
        }
        else if (rb.linearVelocity.y > 0)
        {
            SetAnimation(isShooting ? "jumpShoot" : "jump");
        }
        else
        {
            SetAnimation(isShooting ? "jumpShoot" : "fall");
        }
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
                // Reset current frame BEFORE accessing the array
                currentFrame = 0;
                OnAnimationComplete();
            }
            else
            {
                currentFrame %= currentFrames.Length;
                spriteRenderer.sprite = currentFrames[currentFrame];
            }
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
        isPlayingOneShot = false;

        // Handle returning to idle after one-shot animations
        if (currentAnimation == "jump")
        {
            // If still shooting, transition to jumpShoot
            if (isShooting)
            {
                SetAnimation("jumpShoot");
                return;
            }
            
            // If we're falling, go to fall animation
            if (rb.linearVelocity.y < 0)
            {
                SetAnimation("fall");
            }
            // Otherwise go back to idle or run based on horizontal input
            else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                SetAnimation("run");
            }
            else
            {
                SetAnimation("idle");
            }
        }
        else if (currentAnimation == "jumpShoot")
        {
            // If still in air and shooting, maintain jumpShoot
            if (!isGrounded && isShooting)
            {
                SetAnimation("jumpShoot");
                return;
            }
            
            // If falling and still shooting
            if (rb.linearVelocity.y < 0 && isShooting)
            {
                SetAnimation("jumpShoot");
            }
            // If on ground now
            else if (isGrounded)
            {
                if (isShooting && Mathf.Abs(rb.linearVelocity.x) > 0.1f)
                {
                    SetAnimation("runShoot");
                }
                else if (isShooting)
                {
                    SetAnimation("shoot");
                }
                else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
                {
                    SetAnimation("run");
                }
                else
                {
                    SetAnimation("idle");
                }
            }
            // If no longer shooting but still in air
            else if (!isShooting && rb.linearVelocity.y < 0)
            {
                SetAnimation("fall");
            }
            else if (!isShooting && rb.linearVelocity.y > 0)
            {
                SetAnimation("jump");
            }
        }
        else if (currentAnimation == "shoot")
        {
            isShooting = false;
            SetAnimation("idle");
        }
        else if (currentAnimation == "dash")
        {
            // Let EndDash handle the transition
            // This allows dashes to be interrupted for jumping
        }
        else if (currentAnimation == "hurt")
        {
            SetAnimation("idle");
        }
    }

    public void TakeDamage()
    {
        SetAnimation("hurt", true, true);
        isPlayingOneShot = true;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}