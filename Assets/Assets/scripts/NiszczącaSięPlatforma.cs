
using UnityEngine;
using System.Collections;

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

    private Renderer[] renderers;
    private Collider[] colliders;
    private AudioSource audioSource;

    private Material[][] materials;
    private Color[][] originalColors;

    private bool[] originalRendererStates;
    private bool[] originalColliderStates;

    private Coroutine flashCoroutine;

    void Start()
    {
        // Pobieranie wszystkich rendererow i colliderow.
        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        originalRendererStates = new bool[renderers.Length];
        originalColliderStates = new bool[colliders.Length];

        materials = new Material[renderers.Length][];
        originalColors = new Color[renderers.Length][];

        // Zapisywanie poczatkowych kolorow i stanow.
        for (int i = 0; i < renderers.Length; i++)
        {
            originalRendererStates[i] = renderers[i].enabled;

            materials[i] = renderers[i].materials;
            originalColors[i] = new Color[materials[i].Length];

            for (int j = 0; j < materials[i].Length; j++)
            {
                if (materials[i][j].HasProperty("_Color") ||
                    materials[i][j].HasProperty("_BaseColor"))
                {
                    originalColors[i][j] = materials[i][j].color;
                }
            }
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            originalColliderStates[i] = colliders[i].enabled;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (triggered) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            triggered = true;

            Invoke(
                nameof(StartWarning),
                Mathf.Max(0f, delay - warningTime)
            );

            Invoke(nameof(DisablePlatform), delay);
        }
    }

    void StartWarning()
    {
        if (crackSound != null)
            audioSource.PlayOneShot(crackSound);

        flashCoroutine = StartCoroutine(FlashEffect());
    }

    IEnumerator FlashEffect()
    {
        float time = 0f;

        while (time < warningTime)
        {
            SetWarningColor(true);

            yield return new WaitForSeconds(0.1f);

            SetWarningColor(false);

            yield return new WaitForSeconds(0.1f);

            time += 0.2f;
        }

        SetWarningColor(false);
        flashCoroutine = null;
    }

    void SetWarningColor(bool warning)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            for (int j = 0; j < materials[i].Length; j++)
            {
                Material mat = materials[i][j];

                if (mat.HasProperty("_Color") ||
                    mat.HasProperty("_BaseColor"))
                {
                    mat.color = warning
                        ? warningColor
                        : originalColors[i][j];
                }
            }
        }
    }

    void DisablePlatform()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        SetWarningColor(false);

        // Ukrywanie calej platformy.
        foreach (Renderer rend in renderers)
        {
            rend.enabled = false;
        }

        // Wylaczanie wszystkich colliderow.
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        Invoke(nameof(EnablePlatform), respawnTime);
    }

    void EnablePlatform()
    {
        // Przywracanie rendererow.
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = originalRendererStates[i];
        }

        // Przywracanie colliderow.
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = originalColliderStates[i];
        }

        SetWarningColor(false);

        triggered = false;
    }
}
