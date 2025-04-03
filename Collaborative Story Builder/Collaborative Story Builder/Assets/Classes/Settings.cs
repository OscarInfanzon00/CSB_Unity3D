using UnityEngine;

public static class Settings
{
    public static float musicVolume = 0.5f;
    public static string inputLangTranslation = "en";
    public static string outputLangTranslation = "ru";
    public static void LoadSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("musicVolume", 0.5f); // default to 0.5 if not saved
    }

    // Call this to save settings when they change or before exiting
    public static void SaveSettings()
    {
        PlayerPrefs.SetFloat("musicVolume", musicVolume);
        PlayerPrefs.Save();
    }
}
