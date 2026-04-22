using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform modelRoot;

    [Header("Movement")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 5f;
    public float crawlSpeed = 1.2f;
    public float rotationSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 4f;
    public float runJumpForce = 5.2f;
    public int maxJumps = 2;
    public float coyoteTime = 0.2f;

    [Header("Acceleration")]
    public float acceleration = 8f;
    public float deceleration = 10f;

    [Header("Slide")]
    public KeyCode slideKey = KeyCode.C;
    public float slideSpeed = 7f;
    public float slideDuration = 0.45f;

    [Header("Dash")]
    public KeyCode dashKey = KeyCode.X;
    public float dashSpeed = 12f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.6f;
    public bool allowAirDash = true;

    [Header("Slide Visual")]
    public float slideModelYOffset = -0.55f;
    public float slideVisualLerpSpeed = 12f;

    [Header("Slide Collider")]
    [Range(0.3f, 1f)]
    public float slideColliderHeightMultiplier = 0.55f;
    public float colliderLerpSpeed = 12f;

    [Header("Wall")]
    public float wallCheckDistance = 0.8f;
    public float wallCheckHeight = 1.0f;
    public float wallSlideSpeed = -1.5f;
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
    private CapsuleCollider capsule;

    private Vector3 inputDirection;
    private Vector3 lastMoveDirection = Vector3.forward;
    private Vector3 wallJumpDirection = Vector3.zero;
    private Vector3 slideDirection = Vector3.zero;
    private Vector3 dashDirection = Vector3.zero;

    private Vector3 modelStartLocalPos;
    private float capsuleNormalHeight;
    private Vector3 capsuleNormalCenter;
    private float capsuleSlideHeight;
    private Vector3 capsuleSlideCenter;

    private bool isGrounded = true;
    private bool jumpPressed = false;
    private bool wallJumpPressed = false;

    private bool isSliding = false;
    private float slideTimer = 0f;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    private bool isWallLeft = false;
    private bool isWallRight = false;
    private bool isWallHolding = false;

    private float footstepTimer = 0f;
    private float currentMoveSpeed = 0f;
    private float currentJumpForce = 0f;
    private float coyoteTimer = 0f;

    private int jumpCount = 0;

    public bool isDead = false;

    private Ruchomaplatforma currentPlatform;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        capsule = GetComponent<CapsuleCollider>();

        currentJumpForce = jumpForce;

        if (rb != null)
            rb.freezeRotation = true;

        if (modelRoot != null)
            modelStartLocalPos = modelRoot.localPosition;

        if (capsule != null)
        {
            capsuleNormalHeight = capsule.height;
            capsuleNormalCenter = capsule.center;

            capsuleSlideHeight = capsuleNormalHeight * slideColliderHeightMultiplier;
            float heightDifference = capsuleNormalHeight - capsuleSlideHeight;
            capsuleSlideCenter = capsuleNormalCenter - new Vector3(0f, heightDifference * 0.5f, 0f);
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("Run", false);
            animator.SetBool("Crawl", false);
            animator.SetBool("IsGrounded", true);
            animator.SetBool("isSliding", false);
            animator.SetBool("isWallLeft", false);
            animator.SetBool("isWallRight", false);
            animator.SetBool("isWallHolding", false);
        }
    }

    void Update()
    {
        if (isDead) return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        inputDirection = new Vector3(x, 0f, z).normalized;

        if (inputDirection.sqrMagnitude > 0.01f)
            lastMoveDirection = inputDirection;

        bool isMoving = inputDirection.magnitude > 0.1f;
        bool controlHeld = Input.GetKey(KeyCode.LeftControl);
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift);

        bool isCrawling = isMoving && controlHeld && !isSliding && !isDashing;
        bool isRunning = isMoving && shiftHeld && !controlHeld && !isSliding && !isDashing;

        currentJumpForce = isRunning ? runJumpForce : jumpForce;

        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        UpdateDash();
        CheckWalls();
        UpdateSlide(isRunning, isCrawling);
        UpdateWallHold();
        HandleDashInput();
        UpdateSlideBodyShape();

        if (animator != null)
        {
            animator.SetFloat("Speed", isMoving ? 1f : 0f);
            animator.SetBool("Run", isRunning);
            animator.SetBool("Crawl", isCrawling);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("isSliding", isSliding);
            animator.SetBool("isWallHolding", isWallHolding);
            animator.SetBool("isWallLeft", isWallHolding && isWallLeft);
            animator.SetBool("isWallRight", isWallHolding && isWallRight);
        }

        HandleFootsteps(isMoving, isRunning, isCrawling);
        HandleJumpInput(isCrawling);
    }

    void FixedUpdate()
    {
        if (isDead || rb == null) return;

        bool isMoving = inputDirection.magnitude > 0.1f;
        bool controlHeld = Input.GetKey(KeyCode.LeftControl);
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift);

        bool isCrawling = isMoving && controlHeld && !isSliding && !isDashing;
        bool isRunning = isMoving && shiftHeld && !controlHeld && !isSliding && !isDashing;

        Vector3 platformMove = Vector3.zero;
        if (currentPlatform != null)
            platformMove = currentPlatform.PlatformDelta;

        if (isDashing)
        {
            Vector3 dashMove = dashDirection * dashSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + platformMove + dashMove);

            if (dashDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(dashDirection);
                Quaternion smoothRotation = Quaternion.Slerp(
                    rb.rotation,
                    targetRotation,
                    rotationSpeed * Time.fixedDeltaTime
                );
                rb.MoveRotation(smoothRotation);
            }

            return;
        }

        if (isWallHolding)
        {
            Vector3 velocity = rb.linearVelocity;
            if (velocity.y < wallSlideSpeed)
                velocity.y = wallSlideSpeed;

            rb.linearVelocity = velocity;
            return;
        }

        if (isSliding)
        {
            Vector3 slideMove = slideDirection * slideSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + platformMove + slideMove);

            if (slideDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(slideDirection);
                Quaternion smoothRotation = Quaternion.Slerp(
                    rb.rotation,
                    targetRotation,
                    rotationSpeed * Time.fixedDeltaTime
                );
                rb.MoveRotation(smoothRotation);
            }

            return;
        }

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

        Vector3 playerMove = Vector3.zero;

        if (isMoving)
        {
            playerMove = inputDirection * currentMoveSpeed * Time.fixedDeltaTime;

            bool canRotate =
                !isWallHolding &&
                !(!isGrounded && (isWallLeft || isWallRight));

            if (canRotate)
            {
                Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
                Quaternion smoothRotation = Quaternion.Slerp(
                    rb.rotation,
                    targetRotation,
                    rotationSpeed * Time.fixedDeltaTime
                );

                rb.MoveRotation(smoothRotation);
            }
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

    void UpdateDash()
    {
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
                isDashing = false;
        }
    }

    void HandleDashInput()
    {
        if (!Input.GetKeyDown(dashKey))
            return;

        if (isDashing)
            return;

        if (dashCooldownTimer > 0f)
            return;

        if (!allowAirDash && !isGrounded)
            return;

        Vector3 rawDashDirection = inputDirection.sqrMagnitude > 0.01f ? inputDirection : lastMoveDirection;
        rawDashDirection.y = 0f;

        if (rawDashDirection.sqrMagnitude <= 0.0001f)
            rawDashDirection = transform.forward;

        dashDirection = rawDashDirection.normalized;

        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        isSliding = false;
        isWallHolding = false;
        isWallLeft = false;
        isWallRight = false;

        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;

        if (animator != null)
        {
            animator.SetBool("isSliding", false);
            animator.SetBool("isWallHolding", false);
            animator.SetBool("isWallLeft", false);
            animator.SetBool("isWallRight", false);
        }
    }

    void CheckWalls()
    {
        if (isGrounded)
        {
            isWallLeft = false;
            isWallRight = false;
            return;
        }

        Vector3 origin = transform.position + Vector3.up * wallCheckHeight;

        bool hitLeft = Physics.Raycast(
            origin,
            -transform.right,
            wallCheckDistance,
            wallLayer,
            QueryTriggerInteraction.Ignore
        );

        bool hitRight = Physics.Raycast(
            origin,
            transform.right,
            wallCheckDistance,
            wallLayer,
            QueryTriggerInteraction.Ignore
        );

        bool hitForward = Physics.Raycast(
            origin,
            transform.forward,
            wallCheckDistance,
            wallLayer,
            QueryTriggerInteraction.Ignore
        );

        bool hitBackward = Physics.Raycast(
            origin,
            -transform.forward,
            wallCheckDistance,
            wallLayer,
            QueryTriggerInteraction.Ignore
        );

        isWallLeft = hitLeft || hitForward;
        isWallRight = hitRight || hitBackward;
    }

    void UpdateSlide(bool isRunning, bool isCrawling)
    {
        if (isDashing)
        {
            isSliding = false;
            return;
        }

        if (Input.GetKeyDown(slideKey) && isGrounded && isRunning && !isCrawling && !isSliding)
        {
            isSliding = true;
            isWallHolding = false;
            slideTimer = slideDuration;
            slideDirection = inputDirection.sqrMagnitude > 0.01f ? inputDirection : lastMoveDirection;
        }

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;

            if (slideTimer <= 0f || !isGrounded)
                isSliding = false;
        }
    }

    void UpdateWallHold()
    {
        if (isGrounded || isSliding || isDashing)
        {
            isWallHolding = false;
            return;
        }

        bool touchingWall = isWallLeft || isWallRight;

        bool pressingAnyMoveKey =
            Input.GetKey(KeyCode.UpArrow) ||
            Input.GetKey(KeyCode.DownArrow) ||
            Input.GetKey(KeyCode.LeftArrow) ||
            Input.GetKey(KeyCode.RightArrow) ||
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.D);

        bool falling = rb.linearVelocity.y <= 0f;

        isWallHolding = touchingWall && pressingAnyMoveKey && falling;

        if (!touchingWall)
        {
            isWallLeft = false;
            isWallRight = false;
        }
    }

    void HandleJumpInput(bool isCrawling)
    {
        if (isDashing)
            return;

        if (!Input.GetKeyDown(KeyCode.Space) || isCrawling)
            return;

        if (isWallHolding)
        {
            wallJumpPressed = true;
            jumpPressed = false;

            Vector3 awayFromWall = isWallLeft ? transform.right : -transform.right;
            wallJumpDirection = (awayFromWall + Vector3.up).normalized;

            isWallHolding = false;
            isWallLeft = false;
            isWallRight = false;
            isSliding = false;
            isGrounded = false;
            coyoteTimer = 0f;
            jumpCount = 1;

            if (animator != null)
            {
                animator.ResetTrigger("Jump");
                animator.SetBool("IsGrounded", false);
                animator.SetBool("isWallHolding", false);
                animator.SetBool("isWallLeft", false);
                animator.SetBool("isWallRight", false);
                animator.SetTrigger("Jump");
            }

            PlaySound(jumpSound);
            return;
        }

        bool canGroundJump = isGrounded || coyoteTimer > 0f;
        bool canAirJump = !canGroundJump && jumpCount < maxJumps;

        if (canGroundJump || canAirJump)
        {
            jumpPressed = true;
            wallJumpPressed = false;
            isSliding = false;
            isWallHolding = false;
            isWallLeft = false;
            isWallRight = false;

            if (canGroundJump)
                jumpCount = 1;
            else
                jumpCount++;

            isGrounded = false;
            coyoteTimer = 0f;

            if (animator != null)
            {
                animator.ResetTrigger("Jump");
                animator.SetBool("IsGrounded", false);
                animator.SetBool("isWallHolding", false);
                animator.SetBool("isWallLeft", false);
                animator.SetBool("isWallRight", false);
                animator.SetTrigger("Jump");
            }

            PlaySound(jumpSound);
        }
    }

    void UpdateSlideBodyShape()
    {
        UpdateModelSlideOffset();
        UpdateCapsuleSlideShape();
    }

    void UpdateModelSlideOffset()
    {
        if (modelRoot == null) return;

        Vector3 targetPos = modelStartLocalPos;

        if (isSliding)
            targetPos = modelStartLocalPos + new Vector3(0f, slideModelYOffset, 0f);

        modelRoot.localPosition = Vector3.Lerp(
            modelRoot.localPosition,
            targetPos,
            Time.deltaTime * slideVisualLerpSpeed
        );
    }

    void UpdateCapsuleSlideShape()
    {
        if (capsule == null) return;

        bool shouldStayLow = isSliding || !CanStandUp();

        float targetHeight = shouldStayLow ? capsuleSlideHeight : capsuleNormalHeight;
        Vector3 targetCenter = shouldStayLow ? capsuleSlideCenter : capsuleNormalCenter;

        capsule.height = Mathf.Lerp(capsule.height, targetHeight, Time.deltaTime * colliderLerpSpeed);
        capsule.center = Vector3.Lerp(capsule.center, targetCenter, Time.deltaTime * colliderLerpSpeed);
    }

    bool CanStandUp()
    {
        if (capsule == null) return true;

        float radius = Mathf.Max(0.05f, capsule.radius * 0.95f);
        float checkHeight = capsuleNormalHeight - (radius * 2f);

        Vector3 bottom = transform.TransformPoint(
            capsuleNormalCenter + Vector3.down * (capsuleNormalHeight * 0.5f - radius)
        );

        Vector3 top = bottom + Vector3.up * Mathf.Max(0.01f, checkHeight);

        bool blocked = Physics.CheckCapsule(
            bottom,
            top,
            radius,
            wallLayer,
            QueryTriggerInteraction.Ignore
        );

        return !blocked;
    }

    void HandleFootsteps(bool isMoving, bool isRunning, bool isCrawling)
    {
        if (!isGrounded || !isMoving || isSliding || isWallHolding || isDashing || footstepSound == null)
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
            audioSource.PlayOneShot(clip);
    }

    public void PlayDeathSound()
    {
        PlaySound(deathSound);
    }

    void OnCollisionEnter(Collision collision)
    {
        bool landedNow = !isGrounded;

        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            if (HasGroundContact(collision))
            {
                isGrounded = true;
                jumpCount = 0;
                isWallHolding = false;
                isWallLeft = false;
                isWallRight = false;

                if (collision.gameObject.CompareTag("Platform"))
                    currentPlatform = collision.gameObject.GetComponent<Ruchomaplatforma>();

                if (animator != null)
                {
                    animator.SetBool("IsGrounded", true);
                    animator.SetBool("isWallHolding", false);
                    animator.SetBool("isWallLeft", false);
                    animator.SetBool("isWallRight", false);
                }

                if (landedNow)
                    PlaySound(landingSound);
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            if (HasGroundContact(collision))
            {
                isGrounded = true;
                jumpCount = 0;
                isWallHolding = false;

                if (collision.gameObject.CompareTag("Platform"))
                    currentPlatform = collision.gameObject.GetComponent<Ruchomaplatforma>();

                if (animator != null)
                {
                    animator.SetBool("IsGrounded", true);
                    animator.SetBool("isWallHolding", false);
                }
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
            currentPlatform = null;

        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = false;

            if (animator != null)
                animator.SetBool("IsGrounded", false);
        }
    }

    bool HasGroundContact(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.4f)
                return true;
        }

        return false;
    }
}