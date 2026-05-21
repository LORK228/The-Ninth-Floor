using UnityEngine;
using UnityEngine.Audio;

public class SettingsApplier : MonoBehaviour
{
    [SerializeField] private AudioMixer mainMixer;

    void Start()
    {
        // Применяем громкость при загрузке игровой сцены
        float volume = GameSettings.MasterVolume;
        // Предотвращаем деление на ноль и ошибку логарифма
        if (volume <= 0.0001f) volume = 0.0001f;
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }
}