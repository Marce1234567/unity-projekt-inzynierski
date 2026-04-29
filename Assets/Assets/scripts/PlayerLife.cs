using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerLife : MonoBehaviour
{
    [Header("Start / Checkpoint")]
    public Transform startPoint;

    public float fallHeight = -5f;
    public int lives = 3;

    public Animator animator;
    public TMP_Text livesText;
    public GameObject gameOverPanel;

    [Header("Respawn Safety")]
    public float respawnDelay = 2f;
    public float invulnerableTime = 1f;
    public float respawnUpOffset = 0.2f;
    public float physicsFreezeTime = 0.2f;

    private bool isDead = false;
    private bool isInvulnerable = false;

    private Vector3 respawnPoint;
    private Quaternion respawnRotation;

    private Rigidbody rb;
    private PlayerController controller;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<PlayerController>();

        if (startPoint != null)
        {
            respawnPoint = startPoint.position;
            respawnRotation = startPoint.rotation;
        }
        else
        {
            respawnPoint = transform.position;
            respawnRotation = transform.rotation;
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateUI();
    }

    void Update()
    {
        if (transform.position.y < fallHeight && !isDead && !isInvulnerable)
            Die();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            Transform spawnPoint = other.transform.Find("SpawnPoint");

            if (spawnPoint != null)
                SetCheckpoint(spawnPoint.position, spawnPoint.rotation);
            else
                SetCheckpoint(other.transform.position, other.transform.rotation);

            Debug.Log("Checkpoint zapisany: " + other.name);
        }

        if (other.CompareTag("Kill"))
        {
            KillPlayer();
        }
    }

    void Die()
    {
        if (isDead || isInvulnerable)
            return;

        isDead = true;
        lives--;

        if (controller != null)
        {
            controller.isDead = true;
            controller.PlayDeathSound();
        }

        if (animator != null)
            animator.SetTrigger("Die");

        UpdateUI();

        StartCoroutine(RespawnRoutine());
    }

    public void KillPlayer()
    {
        Die();
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (lives > 0)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            transform.position = respawnPoint + Vector3.up * respawnUpOffset;
            transform.rotation = respawnRotation;

            ResetAnimator();

            if (controller != null)
                controller.isDead = false;

            isInvulnerable = true;
            isDead = false;

            yield return new WaitForSeconds(physicsFreezeTime);

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = false;
            }

            yield return new WaitForSeconds(invulnerableTime);

            isInvulnerable = false;
        }
        else
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            Time.timeScale = 0f;
        }
    }

    void ResetAnimator()
    {
        if (animator == null) return;

        animator.ResetTrigger("Jump");
        animator.ResetTrigger("Die");

        animator.Rebind();
        animator.Update(0f);

        animator.SetFloat("Speed", 0f);
        animator.SetBool("Run", false);
        animator.SetBool("Crawl", false);
        animator.SetBool("IsGrounded", true);
        animator.SetBool("isSliding", false);
        animator.SetBool("isWallHolding", false);
        animator.SetBool("isWallLeft", false);
        animator.SetBool("isWallRight", false);
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