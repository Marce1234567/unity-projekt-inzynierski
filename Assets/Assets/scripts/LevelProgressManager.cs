using UnityEngine;

public static class LevelProgressManager
{
    public static int GetUnlockedLevel()
    {
        return PlayerPrefs.GetInt("UnlockedLevel", 1);
    }

    public static void UnlockLevel(int levelNumber)
    {
        int currentUnlocked = GetUnlockedLevel();

        if (levelNumber > currentUnlocked)
        {
            PlayerPrefs.SetInt("UnlockedLevel", levelNumber);
            PlayerPrefs.Save();
        }
    }

    public static void ResetProgress()
    {
        PlayerPrefs.SetInt("UnlockedLevel", 1);
        PlayerPrefs.Save();
    }
}