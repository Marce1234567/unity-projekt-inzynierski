using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject levelsPanel;
    public GameObject controlsPanel;

    [Header("Level Buttons")]
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;

    void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        UpdateLevelButtons();
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        levelsPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }

    public void ShowLevels()
    {
        UpdateLevelButtons();

        mainMenuPanel.SetActive(false);
        levelsPanel.SetActive(true);
        controlsPanel.SetActive(false);
    }

    public void ShowControls()
    {
        mainMenuPanel.SetActive(false);
        levelsPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    void UpdateLevelButtons()
    {
        int unlockedLevel =
            PlayerPrefs.GetInt("UnlockedLevel", 1);

        level1Button.interactable = true;
        level2Button.interactable =
            unlockedLevel >= 2;

        level3Button.interactable =
            unlockedLevel >= 3;
    }

    public void LoadLevel1()
    {
        PlayerPrefs.SetInt("SlotToLoad", -1);
        SceneManager.LoadScene("Level1");
    }

    public void LoadLevel2()
    {
        if (PlayerPrefs.GetInt("UnlockedLevel", 1) < 2)
            return;

        PlayerPrefs.SetInt("SlotToLoad", -1);
        SceneManager.LoadScene("Level 2");
    }

    public void LoadLevel3()
    {
        if (PlayerPrefs.GetInt("UnlockedLevel", 1) < 3)
            return;

        PlayerPrefs.SetInt("SlotToLoad", -1);
        SceneManager.LoadScene("Level 3");
    }

    public void OpenSavesMenu()
    {
        PlayerPrefs.SetString("SaveMenuMode", "Load");
        PlayerPrefs.Save();

        SceneManager.LoadScene("SavesMenu");
    }

    public void ResetProgress()
    {
        PlayerPrefs.SetInt("UnlockedLevel", 1);
        PlayerPrefs.Save();

        UpdateLevelButtons();
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}