
using UnityEngine;



public class MegamanController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private SpriteRenderer chargeEffectRenderer;

    private static class PlayerAnimations
    {
        public const string JUMP = "Jumping";
        public const string WALK = "Walking";
        public const string IDLE = "Idle2";
        public const string SHOOT = "WeakShot";
        public const string STRONG_SHOOT = "StrongShooting";
        public const string DASH = "Dashing";
        public const string CHARGE = "Charging";
        public const string BORNING = "Borning";
        public const string JUMP_SHOOT = "JumpAndShoot";
        public const string WALL_SLIDE = "WallSlide";
        public const string DASH_SHOOT = "DashAndShoot";
        public const string WALK_SHOOT = "WalkAndShot";
        public const string FALL = "Falling";
    }

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
    public float shootDuration = 0.3f;
    private bool isDashing = false;
    private bool isJumping = false;
    private bool isCharging = false;
    private float chargeStartTime;
    private bool isBorning = true;
    private bool isShooting = false;

    private enum PlayerState { Grounded, Air, Wall }
    private PlayerState currentState;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        chargeEffectRenderer = chargeEffect.GetComponent<SpriteRenderer>();
        chargeEffect.SetActive(false);
        currentState = PlayerState.Grounded;
        Invoke("EndBorning", 2f);
    }

    void Update()
    {
        if (isBorning) return;

        HandleState();
        HandleActions();
    }

    void HandleState()
    {
        switch (currentState) {
            case PlayerState.Grounded:
                HandleGroundedState();
                break;
            case PlayerState.Air:
                HandleAirState();
                break;
        }
    }

    void HandleGroundedState()
    {
        HandleMovement();

        if (Input.GetKeyDown(KeyCode.K))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJumping = true;
            animator.Play(PlayerAnimations.JUMP);
            currentState = PlayerState.Air;
            PlaySound(jumpSound);
        }

        HandleIdleState();
    }

    void HandleAirState()
    {
        if (rb.velocity.y < 0 && !isJumping) {
            animator.Play(PlayerAnimations.FALL);
            if (Input.GetKeyDown(KeyCode.J))
            {
                isCharging = true;
                chargeStartTime = Time.time;
                this.isShooting = true;
                animator.Play(PlayerAnimations.JUMP_SHOOT);
            }
            
            if (Input.GetKeyUp(KeyCode.J) && isCharging)
            {
                isCharging = false;
                float chargeTime = Time.time - chargeStartTime;
                chargeEffect.SetActive(false);
                Invoke("EndShooting", shootDuration);

                if (chargeTime >= 2f)
                    Shoot(strongShotPrefab);
                else if (chargeTime >= 1f)
                    Shoot(middleShotPrefab);
                else
                    Shoot(weakShotPrefab);

                PlaySound(shootSound);
            }
        }   
    }

    void HandleMovement()
    {
        if (isDashing) return; 
        float moveX = 0;

        if (Input.GetKey(KeyCode.A))
            moveX = -1;
        else if (Input.GetKey(KeyCode.D))
            moveX = 1;

        Vector2 movement = new Vector2(moveX, 0).normalized * walkSpeed;
        rb.velocity = new Vector2(movement.x, rb.velocity.y);

        AnimatorStateInfo currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (rb.velocity.y == 0 && rb.velocity.x != 0  && !currentStateInfo.IsName(PlayerAnimations.WALK)) {
            if(!currentStateInfo.IsName(PlayerAnimations.WALK_SHOOT) && !this.isShooting) animator.Play(PlayerAnimations.WALK);
        }        


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
        if (isCharging) {
            float chargeTime = Time.time - chargeStartTime;
            if (chargeTime >= 0.2f) StartCharging();
        }

        if (Input.GetKeyDown(KeyCode.L) && !isDashing) // Dash
        {
            isDashing = true;
            animator.Play(PlayerAnimations.DASH);
            rb.velocity = new Vector2(transform.localScale.x * dashSpeed, rb.velocity.y);
            Invoke("ResetDash", 0.5f);
            PlaySound(dashSound);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            isCharging = true;
            chargeStartTime = Time.time;
            this.isShooting = true;
        }

        if (Input.GetKeyUp(KeyCode.J) && isCharging)
        {
            isCharging = false;
            float chargeTime = Time.time - chargeStartTime;
            chargeEffect.SetActive(false);
            CancelInvoke("EndShooting");
            Invoke("EndShooting", shootDuration);

            AnimatorStateInfo currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float currentWalkTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            if (rb.velocity.x == 0 && !currentStateInfo.IsName(PlayerAnimations.SHOOT)) animator.Play(PlayerAnimations.SHOOT);
            else {
                if (!currentStateInfo.IsName(PlayerAnimations.WALK_SHOOT)) animator.Play(PlayerAnimations.WALK_SHOOT,0 ,currentWalkTime);
                else if (currentWalkTime > 0.8) animator.Play(PlayerAnimations.WALK_SHOOT);
            }

            if (chargeTime >= 2f)
                Shoot(strongShotPrefab);
            else if (chargeTime >= 1f)
                Shoot(middleShotPrefab);
            else
                Shoot(weakShotPrefab);

            PlaySound(shootSound);
        }
    }

    private void StartCharging() {
        animator.Play(PlayerAnimations.CHARGE, 1, 0f);
        chargeEffect.SetActive(true);
        SetChargeEffectTransparency(0.5f);
        PlaySound(chargeSound);
    }

    void HandleIdleState()
    {
        if (rb.velocity.x == 0 && rb.velocity.y == 0 && !isJumping && !isDashing && !isShooting) {
            AnimatorStateInfo currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (!currentStateInfo.IsName(PlayerAnimations.IDLE))
                animator.Play(PlayerAnimations.IDLE);
        }
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
            bulletController.SetDirection(new Vector2(transform.localScale.x, 0));
        }

        Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), GetComponent<Collider2D>());
    }

    private bool IsGrounded()
    {
        return true; // Replace with ground check logic
    }

    private bool IsTouchingWall()
    {
        return true; // Replace with wall check logic
    }

   void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            this.isJumping = false;
            currentState = PlayerState.Grounded;
        }

    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            currentState = PlayerState.Air;
        }
    }

    void EndBorning()
    {
        this.isBorning = false;
    }

    void EndShooting () {
        this.isShooting = false;
        HandleIdleState();
    }
}
