
using UnityEngine;
using TMPro;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    public Transform respawnPoint;
    public AudioClip checkpointSound;

    [Header("Visual Elements")]
    public LineRenderer checkpointRing;
    public TMP_Text checkpointText;

    [Header("Colors")]
    public Color inactiveColor = new Color(0f, 0.8f, 1f);
    public Color activeColor = new Color(0.1f, 1f, 0.3f);

    [Header("Activation Effect")]
    public GameObject checkpointEffectPrefab;

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

        UpdateVisuals(inactiveColor);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        PlayerLife playerLife =
            other.GetComponentInParent<PlayerLife>();

        if (playerLife != null && respawnPoint != null)
        {
            playerLife.SetCheckpoint(
                respawnPoint.position,
                respawnPoint.rotation
            );

            activated = true;

            // Zmiana koloru
            UpdateVisuals(activeColor);

            // Efekt czasteczkowy
            if (checkpointEffectPrefab != null)
            {
                Vector3 effectPosition =
                    checkpointRing != null
                    ? checkpointRing.transform.position
                    : transform.position;

                Instantiate(
                    checkpointEffectPrefab,
                    effectPosition,
                    Quaternion.identity
                );
            }

            // Powiadomienie UI
            if (checkpointUI != null)
                checkpointUI.ShowCheckpoint();

            // Dzwiek aktywacji
            if (audioSource != null && checkpointSound != null)
                audioSource.PlayOneShot(checkpointSound);

            Debug.Log("Checkpoint activated: " + gameObject.name);
        }
    }

    private void UpdateVisuals(Color color)
    {
        if (checkpointRing != null)
        {
            checkpointRing.startColor = color;
            checkpointRing.endColor = color;
        }

        if (checkpointText != null)
        {
            checkpointText.color = color;
        }
    }
}
