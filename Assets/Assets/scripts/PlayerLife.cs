using UnityEngine;
using TMPro;

public class PlayerLife : MonoBehaviour
{
    public float fallHeight = -5f;
    public int lives = 3;

    public Animator animator;
    public TMP_Text livesText;
    public GameObject gameOverPanel;

    private bool isDead = false;
    private Vector3 respawnPoint;
    private Quaternion respawnRotation;
    private Rigidbody rb;
    private PlayerController controller;

    void Start()
    {
        respawnPoint = transform.position;
        respawnRotation = transform.rotation;

        rb = GetComponent<Rigidbody>();
        controller = GetComponent<PlayerController>();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        UpdateUI();
    }

    void Update()
    {
        if (transform.position.y < fallHeight && !isDead)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        lives--;

        if (controller != null)
        {
            controller.isDead = true;
            controller.PlayDeathSound();
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        if (animator != null)
            animator.SetTrigger("Die");

        UpdateUI();

        Invoke(nameof(Respawn), 2f);
    }

    void Respawn()
    {
        if (lives > 0)
        {
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = false;
            }

            transform.position = respawnPoint;
            transform.rotation = Quaternion.Euler(0f, respawnRotation.eulerAngles.y, 0f);

            foreach (Transform child in transform)
            {
                child.localRotation = Quaternion.identity;
            }

            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);

                animator.SetFloat("Speed", 0f);
                animator.SetBool("Run", false);
                animator.SetBool("Crawl", false);
                animator.SetBool("IsGrounded", true);
            }

            if (rb != null)
            {
                rb.useGravity = true;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (controller != null)
                controller.isDead = false;

            isDead = false;
        }
        else
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            Time.timeScale = 0f;
        }
    }

    void UpdateUI()
    {
        if (livesText != null)
            livesText.text = "Lives: " + lives;
    }

    public void SetCheckpoint(Vector3 newPosition, Quaternion newRotation)
    {
        respawnPoint = newPosition;
        respawnRotation = newRotation;
    }
}