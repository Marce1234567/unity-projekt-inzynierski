using System.Collections;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private AudioSource audioSource;
    private bool stopped = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.spatialBlend = 0f;

            if (!audioSource.isPlaying)
                audioSource.Play();
        }
    }

    public void StopMusic()
    {
        if (stopped) return;
        stopped = true;

        StartCoroutine(FadeOutMusic());
    }

    private IEnumerator FadeOutMusic()
    {
        if (audioSource == null) yield break;

        float startVolume = audioSource.volume;

        while (audioSource.volume > 0f)
        {
            audioSource.volume -= Time.deltaTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }
}