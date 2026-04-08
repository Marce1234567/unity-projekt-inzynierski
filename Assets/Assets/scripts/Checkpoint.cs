using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Vector3 respawnOffset = new Vector3(0f, 1f, 0f);
    private bool activated = false;

    private Renderer checkpointRenderer;

    void Start()
    {
        checkpointRenderer = GetComponent<Renderer>();

        if (checkpointRenderer != null)
        {
            checkpointRenderer.material.color = Color.yellow;
        }
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
            {
                checkpointRenderer.material.color = Color.green;
            }

            Debug.Log("Checkpoint activated: " + gameObject.name);
        }
    }
}