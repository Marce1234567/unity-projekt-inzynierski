using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;

    [Header("Scene")]
    public string menuSceneName = "Menu";
    public string savesMenuSceneName = "SavesMenu";

    [Header("Keys")]
    public KeyCode pauseKey = KeyCode.Escape;
    public KeyCode alternativePauseKey = KeyCode.P;

    [Header("Player")]
    public Transform player;

    private bool isPaused = false;

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (PlayerPrefs.GetInt("ReturnPausedAfterSave", 0) == 1)
        {
            PlayerPrefs.SetInt("ReturnPausedAfterSave", 0);
            PlayerPrefs.Save();

            PauseGame();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey) || Input.GetKeyDown(alternativePauseKey))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (pausePanel == null)
            return;

        pausePanel.SetActive(true);

        Time.timeScale = 0f;
        AudioListener.pause = true;

        isPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        if (pausePanel == null)
            return;

        pausePanel.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;

        isPaused = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OpenSaveMenu()
    {
        if (player == null)
            return;

        PlayerPrefs.SetString("SaveMenuMode", "Save");
        PlayerPrefs.SetString("PendingScene", SceneManager.GetActiveScene().name);

        PlayerPrefs.SetFloat("PendingX", player.position.x);
        PlayerPrefs.SetFloat("PendingY", player.position.y);
        PlayerPrefs.SetFloat("PendingZ", player.position.z);
        PlayerPrefs.SetFloat("PendingRotY", player.eulerAngles.y);

        PlayerPrefs.Save();

        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene(savesMenuSceneName);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene(menuSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}