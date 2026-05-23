using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveSystem
{
    public static void SaveGame(int slot, string sceneName, Vector3 position, Quaternion rotation)
    {
        string key = "SaveSlot_" + slot + "_";

        PlayerPrefs.SetInt(key + "Exists", 1);
        PlayerPrefs.SetString(key + "Scene", sceneName);

        PlayerPrefs.SetFloat(key + "X", position.x);
        PlayerPrefs.SetFloat(key + "Y", position.y);
        PlayerPrefs.SetFloat(key + "Z", position.z);
        PlayerPrefs.SetFloat(key + "RotY", rotation.eulerAngles.y);

        PlayerPrefs.Save();
    }

    public static bool SaveExists(int slot)
    {
        return PlayerPrefs.GetInt("SaveSlot_" + slot + "_Exists", 0) == 1;
    }

    public static string GetSavedScene(int slot)
    {
        return PlayerPrefs.GetString("SaveSlot_" + slot + "_Scene", "");
    }

    public static Vector3 GetSavedPosition(int slot)
    {
        string key = "SaveSlot_" + slot + "_";

        return new Vector3(
            PlayerPrefs.GetFloat(key + "X"),
            PlayerPrefs.GetFloat(key + "Y"),
            PlayerPrefs.GetFloat(key + "Z")
        );
    }

    public static Quaternion GetSavedRotation(int slot)
    {
        string key = "SaveSlot_" + slot + "_";
        float rotY = PlayerPrefs.GetFloat(key + "RotY", 0f);
        return Quaternion.Euler(0f, rotY, 0f);
    }
}