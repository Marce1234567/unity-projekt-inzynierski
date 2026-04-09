using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Vector3 respawnOffset = new Vector3(0f, 1.5f, 0f);

    private bool activated = false;
    private Renderer checkpointRenderer;
    private CheckpointUI checkpointUI;

    void Start()
    {
        checkpointRenderer = GetComponent<Renderer>();

        if (checkpointRenderer != null)
            checkpointRenderer.material.color = Color.yellow;

        checkpointUI = FindFirstObjectByType<CheckpointUI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        PlayerLife playerLife = other.GetComponentInParent<PlayerLife>();

        if (playerLife != null)
        {
            Vector3 respawnPosition = transform.position + respawnOffset;
            playerLife.SetCheckpoint(respawnPosition, transform.rotation);
            activated = true;

            if (checkpointRenderer != null)
                checkpointRenderer.material.color = Color.green;

            if (checkpointUI != null)
                checkpointUI.ShowCheckpoint();

            Debug.Log("Checkpoint activated: " + gameObject.name);
        }
    }
}