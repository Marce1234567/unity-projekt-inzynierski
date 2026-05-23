using UnityEngine;

public class Levelfinish : MonoBehaviour
{
    [Header("UI")]
    public GameObject levelCompletePanel;

    [Header("Level Progress")]
    public int currentLevelNumber = 1;

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

        // Odblokowanie nastêpnego poziomu
        UnlockNextLevel();

        // Stop muzyki
        BackgroundMusic music =
            FindFirstObjectByType<BackgroundMusic>();

        if (music != null)
        {
            music.StopMusic();
        }

        // Pokazanie panelu ukoñczenia
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }

        Debug.Log("Level Complete!");
    }

    private void UnlockNextLevel()
    {
        int unlockedLevel =
            PlayerPrefs.GetInt("UnlockedLevel", 1);

        int nextLevel = currentLevelNumber + 1;

        if (nextLevel > unlockedLevel)
        {
            PlayerPrefs.SetInt(
                "UnlockedLevel",
                nextLevel
            );

            PlayerPrefs.Save();
        }
    }
}