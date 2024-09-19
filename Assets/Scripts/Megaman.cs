using UnityEngine;

public class MegamanController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private AudioSource audioSource;

    public AudioClip dashSound;
    public AudioClip shootSound;
    public AudioClip jumpSound;

    public AudioClip chargeSound;

    public GameObject weakShotPrefab;
    public GameObject middleShotPrefab;
    public GameObject strongShotPrefab;
    public float walkSpeed = 5f;
    public float dashSpeed = 10f;
    public float jumpForce = 10f;
    private bool isDashing = false;
    private bool isJumping = false;
    private bool isShooting = false;

    private bool isCharging = false;
    private float chargeStartTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
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
        float moveX = 0;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveX = -1;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveX = 1;

        Vector2 movement = new Vector2(moveX, 0).normalized * walkSpeed;
        rb.velocity = new Vector2(movement.x, rb.velocity.y);

        if (movement != Vector2.zero && !isDashing)
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
        // Dashing
        if (Input.GetKeyDown(KeyCode.L) && !isDashing)
        {
            isDashing = true;
            animator.SetTrigger("Dash");
            rb.velocity = new Vector2(transform.localScale.x * dashSpeed, rb.velocity.y);
            Invoke("ResetDash", 0.5f);

            PlaySound(dashSound);
        }

        // Shooting
        if (Input.GetKeyDown(KeyCode.J))
        {
            isCharging = true;

            chargeStartTime = Time.time; // Record the time when the button was pressed
            isShooting = true;
            animator.SetTrigger("Shoot");
        }

        if (Input.GetKeyUp(KeyCode.J) && isCharging)
        {
            isCharging = false;
            float chargeTime = Time.time - chargeStartTime;

            if (chargeTime >= 2f)
            {
                Shoot(strongShotPrefab);
            }
            else if (chargeTime >= 1f)
            {
                Shoot(middleShotPrefab);
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
        float offsetX = 0.4f;
        float offsetY = transform.localScale.y * 0.1f;

        Vector3 spawnPosition = new Vector3(transform.position.x + offsetX, transform.position.y + offsetY, transform.position.z);

        GameObject bullet = Instantiate(projectilePrefab, spawnPosition,  Quaternion.identity);
        Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), GetComponent<Collider2D>());
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
