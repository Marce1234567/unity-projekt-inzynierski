using UnityEngine;

public class Playercontroler : MonoBehaviour
{
    public float walkSpeed = 2.5f;
    public float runSpeed = 5f;
    public float crawlSpeed = 1.2f;
    public float rotationSpeed = 10f;
    public float jumpForce = 4f;

    private Rigidbody rb;
    private Animator animator;
    private AudioSource audioSource;

    private Vector3 inputDirection;
    private bool isGrounded = true;
    private bool jumpPressed = false;

    public bool isDead = false;

    private Ruchomaplatforma currentPlatform;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        animator.SetFloat("Speed", 0f);
        animator.SetBool("Run", false);
        animator.SetBool("Crawl", false);
        animator.SetBool("IsGrounded", true);
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

        animator.SetFloat("Speed", isMoving ? 1f : 0f);
        animator.SetBool("Run", isRunning);
        animator.SetBool("Crawl", isCrawling);
        animator.SetBool("IsGrounded", isGrounded);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isCrawling)
        {
            isGrounded = false;
            jumpPressed = true;

            animator.SetBool("IsGrounded", false);
            animator.SetTrigger("Jump");

            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.PlayOneShot(audioSource.clip);
            }
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        bool isMoving = inputDirection.magnitude > 0.1f;
        bool controlHeld = Input.GetKey(KeyCode.LeftControl);
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift);

        bool isCrawling = isMoving && controlHeld;
        bool isRunning = isMoving && shiftHeld && !controlHeld;

        float currentSpeed = walkSpeed;

        if (isCrawling)
            currentSpeed = crawlSpeed;
        else if (isRunning)
            currentSpeed = runSpeed;

        Vector3 platformMove = Vector3.zero;
        if (currentPlatform != null)
        {
            platformMove = currentPlatform.PlatformDelta;
        }

        Vector3 playerMove = Vector3.zero;
        if (isMoving)
        {
            playerMove = inputDirection * currentSpeed * Time.fixedDeltaTime;

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

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("IsGrounded", true);
        }

        if (collision.gameObject.CompareTag("Platform"))
        {
            currentPlatform = collision.gameObject.GetComponent<Ruchomaplatforma>();
            isGrounded = true;
            animator.SetBool("IsGrounded", true);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            currentPlatform = null;
        }
    }
}