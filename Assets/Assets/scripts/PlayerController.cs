using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 5f;
    public float crawlSpeed = 1.2f;
    public float rotationSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 4f;
    public float runJumpForce = 5.2f;
    public int maxJumps = 2;

    [Header("Acceleration")]
    public float acceleration = 8f;
    public float deceleration = 10f;

    [Header("Jump Assist")]
    public float coyoteTime = 0.2f;

    [Header("Wall Jump")]
    public float wallCheckDistance = 0.7f;
    public float wallJumpForce = 6f;
    public LayerMask wallLayer;

    [Header("Audio")]
    public AudioClip jumpSound;
    public AudioClip footstepSound;
    public AudioClip landingSound;
    public AudioClip deathSound;

    [Header("Footstep Timing")]
    public float footstepIntervalWalk = 0.5f;
    public float footstepIntervalRun = 0.3f;
    public float footstepIntervalCrawl = 0.7f;

    private Rigidbody rb;
    private Animator animator;
    private AudioSource audioSource;

    private Vector3 inputDirection;
    private bool isGrounded = true;
    private bool jumpPressed = false;
    private bool wallJumpPressed = false;

    private float footstepTimer = 0f;
    private float currentMoveSpeed = 0f;
    private float currentJumpForce = 0f;
    private float coyoteTimer = 0f;

    private int jumpCount = 0;
    private Vector3 wallJumpDirection = Vector3.zero;

    public bool isDead = false;

    private Ruchomaplatforma currentPlatform;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        currentJumpForce = jumpForce;

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("Run", false);
            animator.SetBool("Crawl", false);
            animator.SetBool("IsGrounded", true);
        }
    }

    void Update()
    {
        if (isDead) return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        inputDirection = new Vector3(x, 0f, z).normalized;

        bool isMoving = inputDirection.magnitude > 0.1f;
        bool controlHeld = Input.GetKey(KeyCode.LeftControl);
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift);

        bool isCrawling = isMoving && controlHeld;
        bool isRunning = isMoving && shiftHeld && !controlHeld;

        currentJumpForce = isRunning ? runJumpForce : jumpForce;

        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        bool isTouchingWall = false;

        if (!isGrounded)
        {
            isTouchingWall = Physics.Raycast(
                transform.position,
                transform.forward,
                wallCheckDistance,
                wallLayer
            );
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", isMoving ? 1f : 0f);
            animator.SetBool("Run", isRunning);
            animator.SetBool("Crawl", isCrawling);
            animator.SetBool("IsGrounded", isGrounded);
        }

        HandleFootsteps(isMoving, isRunning, isCrawling);

        if (Input.GetKeyDown(KeyCode.Space) && !isCrawling)
        {
            bool canNormalJump = (coyoteTimer > 0f || jumpCount < maxJumps) && jumpCount < maxJumps;

            if (canNormalJump)
            {
                jumpPressed = true;
                wallJumpPressed = false;

                if (!isGrounded)
                    jumpCount++;
                else
                    jumpCount = 1;

                isGrounded = false;
                coyoteTimer = 0f;

                if (animator != null)
                {
                    animator.SetBool("IsGrounded", false);
                    animator.SetTrigger("Jump");
                }

                PlaySound(jumpSound);
            }
            else if (isTouchingWall)
            {
                RaycastHit wallHit;

                if (Physics.Raycast(
                    transform.position,
                    transform.forward,
                    out wallHit,
                    wallCheckDistance,
                    wallLayer))
                {
                    wallJumpPressed = true;
                    jumpPressed = false;

                    wallJumpDirection = (-wallHit.normal + Vector3.up).normalized;

                    if (animator != null)
                    {
                        animator.SetBool("IsGrounded", false);
                        animator.SetTrigger("Jump");
                    }

                    PlaySound(jumpSound);
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;
        if (rb == null) return;

        bool isMoving = inputDirection.magnitude > 0.1f;
        bool controlHeld = Input.GetKey(KeyCode.LeftControl);
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift);

        bool isCrawling = isMoving && controlHeld;
        bool isRunning = isMoving && shiftHeld && !controlHeld;

        float targetSpeed = 0f;

        if (isMoving)
        {
            targetSpeed = walkSpeed;

            if (isCrawling)
                targetSpeed = crawlSpeed;
            else if (isRunning)
                targetSpeed = runSpeed;
        }

        float speedChangeRate = isMoving ? acceleration : deceleration;
        currentMoveSpeed = Mathf.MoveTowards(
            currentMoveSpeed,
            targetSpeed,
            speedChangeRate * Time.fixedDeltaTime
        );

        Vector3 platformMove = Vector3.zero;
        if (currentPlatform != null)
        {
            platformMove = currentPlatform.PlatformDelta;
        }

        Vector3 playerMove = Vector3.zero;
        if (isMoving)
        {
            playerMove = inputDirection * currentMoveSpeed * Time.fixedDeltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            Quaternion smoothRotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );

            rb.MoveRotation(smoothRotation);
        }

        rb.MovePosition(rb.position + platformMove + playerMove);

        if (jumpPressed)
        {
            jumpPressed = false;

            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;
            rb.linearVelocity = velocity;

            rb.AddForce(Vector3.up * currentJumpForce, ForceMode.Impulse);
        }

        if (wallJumpPressed)
        {
            wallJumpPressed = false;

            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;
            rb.linearVelocity = velocity;

            rb.AddForce(wallJumpDirection * wallJumpForce, ForceMode.Impulse);
        }
    }

    void HandleFootsteps(bool isMoving, bool isRunning, bool isCrawling)
    {
        if (!isGrounded || !isMoving || footstepSound == null)
        {
            footstepTimer = 0f;
            return;
        }

        float interval = footstepIntervalWalk;

        if (isRunning)
            interval = footstepIntervalRun;
        else if (isCrawling)
            interval = footstepIntervalCrawl;

        footstepTimer += Time.deltaTime;

        if (footstepTimer >= interval)
        {
            PlaySound(footstepSound);
            footstepTimer = 0f;
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayDeathSound()
    {
        PlaySound(deathSound);
    }

    void OnCollisionEnter(Collision collision)
    {
        bool landedNow = !isGrounded;

        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            jumpCount = 0;

            if (animator != null)
                animator.SetBool("IsGrounded", true);

            if (landedNow)
                PlaySound(landingSound);
        }

        if (collision.gameObject.CompareTag("Platform"))
        {
            currentPlatform = collision.gameObject.GetComponent<Ruchomaplatforma>();
            isGrounded = true;
            jumpCount = 0;

            if (animator != null)
                animator.SetBool("IsGrounded", true);

            if (landedNow)
                PlaySound(landingSound);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            currentPlatform = null;
        }

        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = false;

            if (animator != null)
                animator.SetBool("IsGrounded", false);
        }
    }
}