using UnityEngine;

public class Levelfinish : MonoBehaviour
{
    public GameObject levelCompletePanel;

    private bool finished = false;

    private void Start()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (finished) return;
        if (!other.CompareTag("Player")) return;

        finished = true;

        BackgroundMusic music = FindFirstObjectByType<BackgroundMusic>();
        if (music != null)
        {
            music.StopMusic();
        }

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }

        Debug.Log("Level Complete!");
    }
}