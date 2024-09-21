using UnityEngine;

public class MegamanController : MonoBehaviour
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
    public GameObject chargeEffect; // Reference to the charge effect GameObject

    public float walkSpeed = 5f;
    public float dashSpeed = 10f;
    public float jumpForce = 10f;
    private bool isDashing = false;
    private bool isJumping = false;
    private bool isCharging = false;
    private float chargeStartTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        chargeEffectRenderer = chargeEffect.GetComponent<SpriteRenderer>(); // Get the renderer of the charge effect
        chargeEffect.SetActive(false); // Start with the charge effect hidden
    }

    private void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    void Update()
    {
        HandleMovement();
        HandleActions();
        HandleIdleState();
    }

    void HandleMovement()
    {
        if (isDashing)
        {
            // Skip movement handling while dashing
            return;
        }

        float moveX = 0;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveX = -1;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveX = 1;

        Vector2 movement = new Vector2(moveX, 0).normalized * walkSpeed;
        rb.velocity = new Vector2(movement.x, rb.velocity.y);

        if (movement != Vector2.zero)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        if (moveX > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveX < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }


    void HandleActions()
    {
        if (Input.GetKeyDown(KeyCode.L) && !isDashing)
        {
            
            isDashing = true;
            animator.SetTrigger("Dash");
            Debug.Log(transform.localScale);
            rb.velocity = new Vector2(transform.localScale.x * dashSpeed, rb.velocity.y);
            Invoke("ResetDash", 0.5f);

            PlaySound(dashSound);
        }

        // Shooting / Charging
        if (Input.GetKeyDown(KeyCode.J))
        {
            isCharging = true;
            chargeStartTime = Time.time;
            animator.SetTrigger("Shoot");
            animator.SetBool("isCharging", true);
            chargeEffect.SetActive(true); // Show the charge effect

            SetChargeEffectTransparency(0.5f); // Set transparency to 50% (0.5 alpha)
            if (!audioSource.isPlaying || audioSource.clip != shootSound)
            {
                audioSource.clip = chargeSound;
                audioSource.loop = true; // Loop the charging sound
                audioSource.Play();
            }
        }

        if (Input.GetKeyUp(KeyCode.J) && isCharging)
        {
            isCharging = false;
            animator.SetBool("isCharging", false);
            float chargeTime = Time.time - chargeStartTime;
            audioSource.loop = false;
            audioSource.Stop();
            chargeEffect.SetActive(false); // Hide the charge effect

            if (chargeTime >= 2f)
            {
                Shoot(strongShotPrefab);
                animator.SetTrigger("StrongShoot");
            }
            else if (chargeTime >= 1f)
            {
                Shoot(middleShotPrefab);
                animator.SetTrigger("StrongShoot");
            }
            else
            {
                Shoot(weakShotPrefab);
            }

            PlaySound(shootSound);
        }

        // Jumping
        if (Input.GetKeyDown(KeyCode.K) && !isJumping)
        {
            isJumping = true;
            animator.SetBool("isJumping", true);
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

            PlaySound(jumpSound);
        }
    }

    void Shoot(GameObject projectilePrefab)
    {
        float offsetX = 0.1f * transform.localScale.x;
        float offsetY = transform.localScale.y * 0.1f;

        Vector3 spawnPosition = new Vector3(transform.position.x + offsetX, transform.position.y + offsetY, transform.position.z);

        GameObject bullet = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        // Get the BulletController component and set the direction
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.SetDirection(new Vector2(transform.localScale.x, 0)); // Set the direction based on the player's facing direction
        }

        Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), GetComponent<Collider2D>());
    }

    void HandleIdleState()
    {
        if (rb.velocity.x == 0 && !isJumping && !isDashing)
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

    private void SetChargeEffectTransparency(float alpha)
    {
        Color color = chargeEffectRenderer.color;
        color.a = alpha; // Set the alpha for transparency
        chargeEffectRenderer.color = color;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
            animator.SetBool("isJumping", false);
            if (rb.velocity.x != 0)
            {
                animator.SetBool("isWalking", true);
            }
        }
    }
}
