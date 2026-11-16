
using UnityEngine;
using TMPro;

public class Levelfinish : MonoBehaviour
{
    [Header("UI")]
    public GameObject levelCompletePanel;

    [Header("Coins UI")]
    public TMP_Text finishCoinsText;

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

        // Zatrzymanie licznika czasu
        LevelTimer timer =
            FindFirstObjectByType<LevelTimer>();

        if (timer != null)
            timer.StopTimer();

        // Zatrzymanie muzyki
        BackgroundMusic music =
            FindFirstObjectByType<BackgroundMusic>();

        if (music != null)
            music.StopMusic();

        // Zatrzymanie gracza
        PlayerController controller =
            other.GetComponent<PlayerController>();

        if (controller != null)
            controller.isDead = true;

        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // NOWE: Wyswietlanie liczby monet
        UpdateFinishCoins();

        // Pokazanie panelu mety
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void UpdateFinishCoins()
    {
        if (finishCoinsText == null)
        {
            Debug.LogWarning(
                "Nie przypisano Finish Coins Text!"
            );
            return;
        }

        if (CoinManager.Instance == null)
        {
            finishCoinsText.text =
                "Zebrane monety: 0 / 0";

            Debug.LogWarning(
                "Nie znaleziono CoinManager!"
            );

            return;
        }

        int collected =
            CoinManager.Instance.GetCollectedCoins();

        int total =
            CoinManager.Instance.GetTotalCoins();

        finishCoinsText.text =
            "Zebrane monety: " +
            collected + " / " + total;
    }

    private void UnlockNextLevel()
    {
        int unlockedLevel =
            PlayerPrefs.GetInt("UnlockedLevel", 1);

        int nextLevel =
            currentLevelNumber + 1;

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
