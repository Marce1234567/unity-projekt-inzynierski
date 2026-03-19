using UnityEngine;

public class playercontroler : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float rotationSpeed = 12f;

    [Header("Jump")]
    public float jumpForce = 5f;

    private Rigidbody rb;
    private Animator animator;

    private Vector3 inputDirection;
    private bool isGrounded = true;
    private bool jumpPressed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        animator.SetFloat("Speed", 0f);
        animator.SetBool("Run", false);
        animator.SetBool("IsGrounded", true);

        rb.freezeRotation = true;
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        inputDirection = new Vector3(x, 0f, z).normalized;

        bool isMoving = inputDirection.magnitude > 0.1f;
        bool isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);

        animator.SetFloat("Speed", isMoving ? 0.5f : 0f);
        animator.SetBool("Run", isRunning);
        animator.SetBool("IsGrounded", isGrounded);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            isGrounded = false;
            jumpPressed = true;

            animator.SetBool("IsGrounded", false);
            animator.SetTrigger("Jump");
        }
    }

    void FixedUpdate()
    {
        bool isMoving = inputDirection.magnitude > 0.1f;
        bool isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (isMoving)
        {
            Vector3 move = inputDirection * currentSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);

            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            Quaternion smoothRotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );

            rb.MoveRotation(smoothRotation);
        }

        if (jumpPressed)
        {
            jumpPressed = false;

            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;
            rb.linearVelocity = velocity;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("IsGrounded", true);
        }
    }
}