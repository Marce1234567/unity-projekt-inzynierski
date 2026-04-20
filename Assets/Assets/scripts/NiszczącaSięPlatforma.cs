using UnityEngine;

public class NiszczącaSięPlatforma : MonoBehaviour
{
    [Header("Timing")]
    public float delay = 2f;
    public float respawnTime = 3f;
    public float warningTime = 1f;

    [Header("Visual")]
    public Color warningColor = Color.red;

    [Header("Audio")]
    public AudioClip crackSound;

    private bool triggered = false;

    private Renderer rend;
    private Collider col;
    private AudioSource audioSource;
    private Color originalColor;

    void Start()
    {
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        originalColor = rend.material.color;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (triggered) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            triggered = true;

            Invoke(nameof(StartWarning), delay - warningTime);
            Invoke(nameof(DisablePlatform), delay);
        }
    }

    void StartWarning()
    {
        if (crackSound != null)
            audioSource.PlayOneShot(crackSound);

        StartCoroutine(FlashEffect());
    }

    System.Collections.IEnumerator FlashEffect()
    {
        float time = 0f;

        while (time < warningTime)
        {
            rend.material.color = warningColor;
            yield return new WaitForSeconds(0.1f);

            rend.material.color = originalColor;
            yield return new WaitForSeconds(0.1f);

            time += 0.2f;
        }
    }

    void DisablePlatform()
    {
        rend.enabled = false;
        col.enabled = false;

        Invoke(nameof(EnablePlatform), respawnTime);
    }

    void EnablePlatform()
    {
        rend.enabled = true;
        col.enabled = true;
        rend.material.color = originalColor;
        triggered = false;
    }
}
