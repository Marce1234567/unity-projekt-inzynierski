using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Transform respawnPoint;
    public AudioClip checkpointSound;

    private bool activated = false;
    private AudioSource audioSource;
    private CheckpointUI checkpointUI;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        checkpointUI = FindFirstObjectByType<CheckpointUI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        PlayerLife playerLife = other.GetComponentInParent<PlayerLife>();

        if (playerLife != null && respawnPoint != null)
        {
            playerLife.SetCheckpoint(respawnPoint.position, respawnPoint.rotation);
            activated = true;

            if (checkpointUI != null)
                checkpointUI.ShowCheckpoint();

            if (audioSource != null && checkpointSound != null)
                audioSource.PlayOneShot(checkpointSound);

            Debug.Log("Checkpoint activated: " + gameObject.name);
        }
    }
}