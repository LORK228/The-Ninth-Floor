using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Названия сцен")]
    [SerializeField] private string gameSceneName = "1"; // <-- ВАЖНО: Укажите здесь точное имя вашей игровой сцены

    [Header("UI Элементы")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject loadingScreenPanel;
    [SerializeField] private Slider loadingSlider;

    [Header("Элементы настроек")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioMixer mainMixer; // <-- Сюда перетащите ваш AudioMixer

    private void Start()
    {
        // При старте меню всегда показываем главный экран и выключаем остальные
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        loadingScreenPanel.SetActive(false);

        // Загружаем настройки и выставляем значения слайдеров
        GameSettings.LoadSettings();
        sensitivitySlider.value = GameSettings.MouseSensitivity;
        volumeSlider.value = GameSettings.MasterVolume;

        // Сразу применяем громкость
        SetVolume(GameSettings.MasterVolume);

        // Устанавливаем слушателей для слайдеров
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void PlayGame()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        loadingScreenPanel.SetActive(true);

        StartCoroutine(LoadSceneAsynchronously(gameSceneName));
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        GameSettings.SaveSettings(); // Сохраняем настройки при выходе из меню
    }

    public void ExitGame()
    {
        Debug.Log("Выход из игры...");
        Application.Quit();
    }

    public void SetSensitivity(float value)
    {
        GameSettings.MouseSensitivity = value;
    }

    public void SetVolume(float value)
    {
        GameSettings.MasterVolume = value;
        // Громкость в микшере измеряется в децибелах (логарифмическая шкала)
        // Формула для перевода линейного значения (0-1) в децибелы (-80 до 0)
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
    }

    private IEnumerator LoadSceneAsynchronously(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingSlider.value = progress;
            yield return null;
        }
    }
}