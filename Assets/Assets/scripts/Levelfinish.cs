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
            levelCompletePanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (finished) return;
        if (!other.CompareTag("Player")) return;

        finished = true;

        UnlockNextLevel();

        LevelTimer timer = FindFirstObjectByType<LevelTimer>();
        if (timer != null)
            timer.StopTimer();

        BackgroundMusic music = FindFirstObjectByType<BackgroundMusic>();
        if (music != null)
            music.StopMusic();

        PlayerController controller = other.GetComponent<PlayerController>();
        if (controller != null)
            controller.isDead = true;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void UnlockNextLevel()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        int nextLevel = currentLevelNumber + 1;

        if (nextLevel > unlockedLevel)
        {
            PlayerPrefs.SetInt("UnlockedLevel", nextLevel);
            PlayerPrefs.Save();
        }
    }
}