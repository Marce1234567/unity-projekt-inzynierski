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
    private Playercontroler controller;

    void Start()
    {
        respawnPoint = transform.position;
        respawnRotation = transform.rotation;

        rb = GetComponent<Rigidbody>();
        controller = GetComponent<Playercontroler>();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

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
            controller.isDead = true;

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
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            transform.position = respawnPoint;
            transform.rotation = respawnRotation;

            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);

                animator.SetFloat("Speed", 0f);
                animator.SetBool("Run", false);
                animator.SetBool("Crawl", false);
                animator.SetBool("IsGrounded", true);
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
}