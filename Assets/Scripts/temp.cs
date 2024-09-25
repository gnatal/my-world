
using UnityEngine;

public class MegamanController2 : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private SpriteRenderer chargeEffectRenderer;

    public AudioClip dashSound;
    public AudioClip shootSound;
    public AudioClip jumpSound;
    public AudioClip chargeSound;

    public GameObject weakShotPrefab;
    public GameObject middleShotPrefab;
    public GameObject strongShotPrefab;
    public GameObject chargeEffect;

    public float walkSpeed = 5f;
    public float dashSpeed = 10f;
    public float jumpForce = 10f;
    public float wallSlideSpeed = 2f;
    private bool isDashing = false;
    private bool isJumping = false;
    private bool isCharging = false;
    private float chargeStartTime;

    private enum PlayerState { Grounded, Air, Wall }
    private PlayerState currentState;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        chargeEffectRenderer = chargeEffect.GetComponent<SpriteRenderer>();
        chargeEffect.SetActive(false); // Start with the charge effect hidden
        currentState = PlayerState.Grounded; // Start grounded
    }

    void Update()
    {
        HandleState();
        HandleActions();
    }

    void HandleState()
    {
        switch (currentState)
        {
            case PlayerState.Grounded:
                HandleGroundedState();
                break;
            case PlayerState.Air:
                HandleAirState();
                break;
            case PlayerState.Wall:
                HandleWallState();
                break;
        }
    }

    void HandleGroundedState()
    {
        HandleMovement();

        if (Input.GetKeyDown(KeyCode.K)) // Jump
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJumping = true;
            animator.SetBool("isJumping", true);
            PlaySound(jumpSound);
            currentState = PlayerState.Air;
        }

        HandleIdleState();
    }

    void HandleAirState()
    {
        if (rb.velocity.y > 0) // Going up
        {
            animator.SetBool("isGoingUp", true);
            animator.SetBool("isFalling", false);
        }
        else if (rb.velocity.y < 0) // Falling
        {
            animator.SetBool("isGoingUp", false);
            animator.SetBool("isFalling", true);
        }

        if (isJumping && Input.GetKeyDown(KeyCode.J)) // Air shooting
        {
            animator.SetTrigger("AirShoot");
            PlaySound(shootSound);
        }

        if (IsTouchingWall() && Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) // Wall slide/climb
        {
            currentState = PlayerState.Wall;
            animator.SetBool("isWallSliding", true);
        }

        if (rb.velocity.y <= 0 && IsGrounded())
        {
            isJumping = false;
            currentState = PlayerState.Grounded;
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
        }
    }

    void HandleWallState()
    {
        rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed); // Slide down

        if (Input.GetKeyDown(KeyCode.K)) // Wall climb (jump off)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            currentState = PlayerState.Air;
            animator.SetBool("isWallSliding", false);
            animator.SetBool("isJumping", true);
        }
    }

    void HandleMovement()
    {
        if (isDashing) return; // Skip movement during dash

        float moveX = 0;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveX = -1;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveX = 1;

        Vector2 movement = new Vector2(moveX, 0).normalized * walkSpeed;
        rb.velocity = new Vector2(movement.x, rb.velocity.y);

        animator.SetBool("isWalking", movement != Vector2.zero);

        if (moveX > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveX < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }


    private void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }


    void HandleActions()
    {
        if (Input.GetKeyDown(KeyCode.L) && !isDashing) // Dash
        {
            isDashing = true;
            animator.SetTrigger("Dash");
            rb.velocity = new Vector2(transform.localScale.x * dashSpeed, rb.velocity.y);
            Invoke("ResetDash", 0.5f);
            PlaySound(dashSound);
        }

        // Shooting / Charging logic (same across all states)
        if (Input.GetKeyDown(KeyCode.J))
        {
            isCharging = true;
            chargeStartTime = Time.time;
            animator.SetTrigger("Shoot");
            animator.SetBool("isCharging", true);
            chargeEffect.SetActive(true);
            SetChargeEffectTransparency(0.5f);
            PlaySound(chargeSound);
        }

        if (Input.GetKeyUp(KeyCode.J) && isCharging)
        {
            isCharging = false;
            animator.SetBool("isCharging", false);
            float chargeTime = Time.time - chargeStartTime;
            chargeEffect.SetActive(false);

            if (chargeTime >= 2f)
                Shoot(strongShotPrefab);
            else if (chargeTime >= 1f)
                Shoot(middleShotPrefab);
            else
                Shoot(weakShotPrefab);

            PlaySound(shootSound);
        }
    }

    void HandleIdleState()
    {
        if (rb.velocity.x == 0 && !isJumping && !isDashing)
            animator.SetBool("isIdle", true);
        else
            animator.SetBool("isIdle", false);
    }

    private void ResetDash()
    {
        isDashing = false;
    }

    private void SetChargeEffectTransparency(float alpha)
    {
        Color color = chargeEffectRenderer.color;
        color.a = alpha;
        chargeEffectRenderer.color = color;
    }

    void Shoot(GameObject projectilePrefab)
    {
        float offsetX = 0.1f * transform.localScale.x;
        float offsetY = transform.localScale.y * 0.1f;

        Vector3 spawnPosition = new Vector3(transform.position.x + offsetX, transform.position.y + offsetY, transform.position.z);

        GameObject bullet = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.SetDirection(new Vector2(transform.localScale.x, 0)); // Set the direction based on the player's facing direction
        }

        Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), GetComponent<Collider2D>());
    }

    private bool IsGrounded()
    {
        
        return true; // Replace with ground check logic
    }

    private bool IsTouchingWall()
    {
        // Add logic to check if the player is touching a wall
        return true; // Replace with wall check logic
    }
}
