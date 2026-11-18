
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform modelRoot;
    public Transform cameraTransform;

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
    public float slideDuration = 1f;

    [Header("Dash")]
    public KeyCode dashKey = KeyCode.X;
    public float dashSpeed = 12f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.6f;
    public bool allowAirDash = true;

    [Header("Slide Visual")]
    public float slideModelYOffset = -0.55f;
    public float slideVisualLerpSpeed = 12f;

    // NOWE: Podnoszenie modelu podczas czolgania
    [Header("Crawl Visual")]
    public float crawlModelYOffset = 0.1f;

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
    public AudioClip dashSound;

    [Header("Effects")]
    public TrailRenderer dashTrail;
    public ParticleSystem movementParticles;
    public CameraFOVEffect cameraFOVEffect;

    [Header("Movement Particles")]
    public float walkParticleRate = 5f;
    public float runParticleRate = 15f;
    public float slideParticleRate = 35f;

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
    private bool isCrawling = false;
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

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        currentJumpForce = jumpForce;

        if (rb != null)
            rb.freezeRotation = true;

        if (modelRoot != null)
            modelStartLocalPos = modelRoot.localPosition;

        if (capsule != null)
        {
            capsuleNormalHeight = capsule.height;
            capsuleNormalCenter = capsule.center;

            capsuleSlideHeight =
                capsuleNormalHeight * slideColliderHeightMultiplier;

            float heightDifference =
                capsuleNormalHeight - capsuleSlideHeight;

            capsuleSlideCenter =
                capsuleNormalCenter -
                new Vector3(0f, heightDifference * 0.5f, 0f);
        }

        if (dashTrail != null)
            dashTrail.emitting = false;

        if (movementParticles != null)
        {
            SetMovementParticleRate(0f);
            movementParticles.Stop();
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

        ReadInput();

        bool isMoving = inputDirection.magnitude > 0.1f;
        bool controlHeld = Input.GetKey(KeyCode.LeftControl);
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift);

        bool wantsToCrawl =
            isMoving &&
            controlHeld &&
            !isSliding &&
            !isDashing;

        bool canStand = CanStandUp();

        isCrawling =
            !isSliding &&
            !isDashing &&
            (wantsToCrawl || !canStand);

        bool isRunning =
            isMoving &&
            shiftHeld &&
            !controlHeld &&
            !isSliding &&
            !isDashing &&
            canStand;

        currentJumpForce =
            isRunning ? runJumpForce : jumpForce;

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

            animator.SetBool(
                "isWallLeft",
                isWallHolding && isWallLeft
            );

            animator.SetBool(
                "isWallRight",
                isWallHolding && isWallRight
            );
        }

        HandleFootsteps(isMoving, isRunning, isCrawling);
        HandleMovementParticles(isMoving, isRunning, isCrawling);
        HandleJumpInput(isCrawling);
    }

    void FixedUpdate()
    {
        if (isDead || rb == null) return;

        UpdateCapsuleSlideShape();

        bool isMoving = inputDirection.magnitude > 0.1f;
        bool controlHeld = Input.GetKey(KeyCode.LeftControl);
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift);
        bool crawlingNow = isCrawling;

        bool isRunning = isMoving && shiftHeld && !controlHeld &&
                         !isSliding && !isDashing && !crawlingNow;

        // Ruch platformy przekazujemy jako predkosc, a nie przesuniecie pozycji.
        Vector3 platformVelocity = currentPlatform != null
            ? currentPlatform.PlatformVelocity
            : Vector3.zero;

        if (isWallHolding)
        {
            Vector3 wallVelocity = rb.linearVelocity;
            if (wallVelocity.y < wallSlideSpeed)
                wallVelocity.y = wallSlideSpeed;
            rb.linearVelocity = wallVelocity;
            return;
        }

        Vector3 desiredMovementVelocity = Vector3.zero;

        if (isDashing)
        {
            desiredMovementVelocity = dashDirection * dashSpeed;
        }
        else if (isSliding)
        {
            desiredMovementVelocity = slideDirection * slideSpeed;
        }
        else
        {
            float targetSpeed = 0f;
            if (isMoving)
            {
                targetSpeed = crawlingNow ? crawlSpeed :
                    (isRunning ? runSpeed : walkSpeed);
            }

            float speedChangeRate = isMoving ? acceleration : deceleration;
            currentMoveSpeed = Mathf.MoveTowards(
                currentMoveSpeed, targetSpeed,
                speedChangeRate * Time.fixedDeltaTime
            );

            if (isMoving)
                desiredMovementVelocity = inputDirection * currentMoveSpeed;
        }

        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontalVelocity = desiredMovementVelocity +
            new Vector3(platformVelocity.x, 0f, platformVelocity.z);
        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;
        rb.linearVelocity = velocity;

        Vector3 facingDirection = isDashing ? dashDirection :
            (isSliding ? slideDirection : inputDirection);
        bool canRotate = isDashing || isSliding ||
            (isMoving && !(!isGrounded && (isWallLeft || isWallRight)));

        if (canRotate && facingDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(facingDirection);
            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation, targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            ));
        }

        if (jumpPressed)
        {
            jumpPressed = false;
            Vector3 jumpVelocity = rb.linearVelocity;
            jumpVelocity.y = 0f;
            rb.linearVelocity = jumpVelocity;
            rb.AddForce(Vector3.up * currentJumpForce, ForceMode.Impulse);
        }

        if (wallJumpPressed)
        {
            wallJumpPressed = false;
            Vector3 jumpVelocity = rb.linearVelocity;
            jumpVelocity.y = 0f;
            rb.linearVelocity = jumpVelocity;
            rb.AddForce(wallJumpDirection * wallJumpForce, ForceMode.Impulse);
        }
    }

    void ReadInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        if (cameraTransform != null)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            inputDirection =
                (camForward * z + camRight * x).normalized;
        }
        else
        {
            inputDirection =
                new Vector3(x, 0f, z).normalized;
        }

        if (inputDirection.sqrMagnitude > 0.01f)
            lastMoveDirection = inputDirection;
    }

    void UpdateDash()
    {
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
            {
                isDashing = false;

                if (dashTrail != null)
                    dashTrail.emitting = false;

                if (cameraFOVEffect != null)
                    cameraFOVEffect.NormalFOV();
            }
        }
    }

    void HandleDashInput()
    {
        if (!Input.GetKeyDown(dashKey)) return;
        if (isDashing) return;
        if (dashCooldownTimer > 0f) return;
        if (!allowAirDash && !isGrounded) return;

        Vector3 rawDashDirection =
            inputDirection.sqrMagnitude > 0.01f
            ? inputDirection
            : lastMoveDirection;

        rawDashDirection.y = 0f;

        if (rawDashDirection.sqrMagnitude <= 0.0001f)
            rawDashDirection = transform.forward;

        dashDirection = rawDashDirection.normalized;

        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        PlaySound(dashSound);

        if (dashTrail != null)
            dashTrail.emitting = true;

        if (cameraFOVEffect != null)
            cameraFOVEffect.DashFOV();

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

        Vector3 origin =
            transform.position +
            Vector3.up * wallCheckHeight;

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

        if (Input.GetKeyDown(slideKey) &&
            isGrounded &&
            isRunning &&
            !isCrawling &&
            !isSliding)
        {
            isSliding = true;
            isWallHolding = false;
            slideTimer = slideDuration;

            slideDirection =
                inputDirection.sqrMagnitude > 0.01f
                ? inputDirection
                : lastMoveDirection;
        }

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;

            if (slideTimer <= 0f || !isGrounded)
            {
                isSliding = false;
            }
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

        isWallHolding =
            touchingWall &&
            pressingAnyMoveKey &&
            falling;

        if (!touchingWall)
        {
            isWallLeft = false;
            isWallRight = false;
        }
    }

    void HandleJumpInput(bool isCrawling)
    {
        if (isDashing) return;

        if (!Input.GetKeyDown(KeyCode.Space) || isCrawling)
            return;

        if (isWallHolding)
        {
            wallJumpPressed = true;
            jumpPressed = false;
            currentPlatform = null;

            Vector3 awayFromWall =
                isWallLeft
                ? transform.right
                : -transform.right;

            wallJumpDirection =
                (awayFromWall + Vector3.up).normalized;

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

        bool canGroundJump =
            isGrounded || coyoteTimer > 0f;

        bool canAirJump =
            !canGroundJump &&
            jumpCount < maxJumps;

        if (canGroundJump || canAirJump)
        {
            jumpPressed = true;
            wallJumpPressed = false;
            currentPlatform = null;
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
    }

    // POPRAWKA: Obsluga modelu podczas czolgania.
    void UpdateModelSlideOffset()
    {
        if (modelRoot == null) return;

        Vector3 targetPos = modelStartLocalPos;

        // Przesuniecie modelu podczas slizgu.
        if (isSliding)
        {
            targetPos =
                modelStartLocalPos +
                new Vector3(0f, slideModelYOffset, 0f);
        }
        // Podniesienie modelu podczas czolgania.
        else if (isCrawling)
        {
            targetPos =
                modelStartLocalPos +
                new Vector3(0f, crawlModelYOffset, 0f);
        }

        // Plynne przejscie miedzy pozycjami.
        modelRoot.localPosition = Vector3.Lerp(
            modelRoot.localPosition,
            targetPos,
            Time.deltaTime * slideVisualLerpSpeed
        );
    }

    // Collider aktualizowany w FixedUpdate.
    void UpdateCapsuleSlideShape()
    {
        if (capsule == null) return;

        bool shouldStayLow =
            isSliding ||
            isCrawling ||
            !CanStandUp();

        if (shouldStayLow)
        {
            capsule.height = capsuleSlideHeight;
            capsule.center = capsuleSlideCenter;
        }
        else
        {
            capsule.height = capsuleNormalHeight;
            capsule.center = capsuleNormalCenter;
        }
    }

    bool CanStandUp()
    {
        if (capsule == null) return true;

        float scaleY =
            Mathf.Abs(transform.lossyScale.y);

        float scaleXZ = Mathf.Max(
            Mathf.Abs(transform.lossyScale.x),
            Mathf.Abs(transform.lossyScale.z)
        );

        float radius = Mathf.Max(
            0.05f,
            capsule.radius * scaleXZ * 0.95f
        );

        float height =
            capsuleNormalHeight * scaleY;

        float checkHeight = Mathf.Max(
            0.01f,
            height - radius * 2f
        );

        Vector3 worldCenter =
            transform.TransformPoint(capsuleNormalCenter);

        Vector3 bottom =
            worldCenter +
            Vector3.down * (height * 0.5f - radius);

        Vector3 top =
            bottom +
            Vector3.up * checkHeight;

        int standCheckMask =
            ~(1 << gameObject.layer);

        bool blocked = Physics.CheckCapsule(
            bottom,
            top,
            radius,
            standCheckMask,
            QueryTriggerInteraction.Ignore
        );

        return !blocked;
    }

    void HandleFootsteps(
        bool isMoving,
        bool isRunning,
        bool isCrawling)
    {
        if (!isGrounded ||
            !isMoving ||
            isSliding ||
            isWallHolding ||
            isDashing ||
            footstepSound == null)
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

    void HandleMovementParticles(
        bool isMoving,
        bool isRunning,
        bool isCrawling)
    {
        if (movementParticles == null)
            return;

        if (!isGrounded ||
            isCrawling ||
            isWallHolding ||
            isDashing)
        {
            SetMovementParticleRate(0f);

            if (movementParticles.isPlaying)
                movementParticles.Stop();

            return;
        }

        if (isSliding)
        {
            SetMovementParticleRate(slideParticleRate);

            if (!movementParticles.isPlaying)
                movementParticles.Play();

            return;
        }

        if (isMoving)
        {
            if (isRunning)
                SetMovementParticleRate(runParticleRate);
            else
                SetMovementParticleRate(walkParticleRate);

            if (!movementParticles.isPlaying)
                movementParticles.Play();
        }
        else
        {
            SetMovementParticleRate(0f);

            if (movementParticles.isPlaying)
                movementParticles.Stop();
        }
    }

    void SetMovementParticleRate(float rate)
    {
        if (movementParticles == null)
            return;

        var emission = movementParticles.emission;
        emission.rateOverTime = rate;
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

        if (collision.gameObject.CompareTag("Ground") ||
            collision.gameObject.CompareTag("Platform"))
        {
            if (HasGroundContact(collision))
            {
                isGrounded = true;
                jumpCount = 0;
                isWallHolding = false;
                isWallLeft = false;
                isWallRight = false;

                if (collision.gameObject.CompareTag("Platform"))
                {
                    currentPlatform = collision.gameObject.GetComponentInParent<Ruchomaplatforma>();
                }
                else if (collision.gameObject.CompareTag("Ground"))
                {
                    currentPlatform = null;
                }

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
        if (collision.gameObject.CompareTag("Ground") ||
            collision.gameObject.CompareTag("Platform"))
        {
            if (HasGroundContact(collision))
            {
                isGrounded = true;
                jumpCount = 0;
                isWallHolding = false;

                if (collision.gameObject.CompareTag("Platform"))
                {
                    currentPlatform = collision.gameObject.GetComponentInParent<Ruchomaplatforma>();
                }
                else if (collision.gameObject.CompareTag("Ground"))
                {
                    currentPlatform = null;
                }

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
        if (collision.gameObject.CompareTag("Platform") &&
            collision.gameObject.GetComponentInParent<Ruchomaplatforma>() == currentPlatform)
            currentPlatform = null;

        if (collision.gameObject.CompareTag("Ground") ||
            collision.gameObject.CompareTag("Platform"))
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
            if (Vector3.Dot(
                contact.normal,
                Vector3.up) > 0.4f)
            {
                return true;
            }
        }

        return false;
    }
}
