using UnityEngine;

public static class GameSettings
{
    // Ключи для сохранения в PlayerPrefs
    private const string SENSITIVITY_KEY = "MouseSensitivity";
    private const string VOLUME_KEY = "MasterVolume";

    // Значения по умолчанию
    private const float DEFAULT_SENSITIVITY = 1.0f;
    private const float DEFAULT_VOLUME = 0.8f;

    // Статические поля для хранения настроек в рантайме
    public static float MouseSensitivity { get; set; }
    public static float MasterVolume { get; set; }

    static GameSettings()
    {
        // Конструктор, который вызывается один раз при первом обращении к классу
        LoadSettings();
    }

    public static void SaveSettings()
    {
        PlayerPrefs.SetFloat(SENSITIVITY_KEY, MouseSensitivity);
        PlayerPrefs.SetFloat(VOLUME_KEY, MasterVolume);
        PlayerPrefs.Save();
        Debug.Log("Настройки сохранены!");
    }

    public static void LoadSettings()
    {
        MouseSensitivity = PlayerPrefs.GetFloat(SENSITIVITY_KEY, DEFAULT_SENSITIVITY);
        MasterVolume = PlayerPrefs.GetFloat(VOLUME_KEY, DEFAULT_VOLUME);
        Debug.Log("Настройки загружены!");
    }
}