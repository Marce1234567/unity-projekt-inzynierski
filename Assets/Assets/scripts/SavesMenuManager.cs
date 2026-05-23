using UnityEngine;
using UnityEngine.SceneManagement;

public class SavesMenuManager : MonoBehaviour
{
    public string mainMenuSceneName = "Menu";

    public void Slot1()
    {
        UseSlot(1);
    }

    public void Slot2()
    {
        UseSlot(2);
    }

    public void Slot3()
    {
        UseSlot(3);
    }

    void UseSlot(int slot)
    {
        string mode = PlayerPrefs.GetString("SaveMenuMode", "Load");

        if (mode == "Save")
        {
            string sceneName = PlayerPrefs.GetString("PendingScene", "");

            Vector3 position = new Vector3(
                PlayerPrefs.GetFloat("PendingX"),
                PlayerPrefs.GetFloat("PendingY"),
                PlayerPrefs.GetFloat("PendingZ")
            );

            Quaternion rotation = Quaternion.Euler(
                0f,
                PlayerPrefs.GetFloat("PendingRotY"),
                0f
            );

            SaveSystem.SaveGame(slot, sceneName, position, rotation);

            PlayerPrefs.SetString("SaveMenuMode", "Load");
            PlayerPrefs.SetInt("ReturnPausedAfterSave", 1);
            PlayerPrefs.Save();

            SceneManager.LoadScene(sceneName);
        }
        else
        {
            if (!SaveSystem.SaveExists(slot))
                return;

            string sceneName = SaveSystem.GetSavedScene(slot);

            if (string.IsNullOrEmpty(sceneName))
                return;

            PlayerPrefs.SetInt("SlotToLoad", slot);
            PlayerPrefs.Save();

            SceneManager.LoadScene(sceneName);
        }
    }

    public void Back()
    {
        string mode = PlayerPrefs.GetString("SaveMenuMode", "Load");

        if (mode == "Save")
        {
            string previousScene = PlayerPrefs.GetString(
                "PendingScene",
                mainMenuSceneName
            );

            PlayerPrefs.SetString("SaveMenuMode", "Load");
            PlayerPrefs.Save();

            SceneManager.LoadScene(previousScene);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}