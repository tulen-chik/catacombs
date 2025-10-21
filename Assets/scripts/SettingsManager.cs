using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer audioMixer; // Ссылка на ваш Audio Mixer
    public Slider musicSlider;    // Слайдер для громкости музыки
    public Slider soundSlider;    // Слайдер для громкости звуков

    [Header("Graphics")]
    public Toggle fullscreenToggle; // Переключатель полноэкранного режима

    [Header("Menu Panels")]
    public GameObject mainMenuPanel; // Панель главного меню (необязательно)

    // Ключи для сохранения настроек
    private const string MusicVolumeKey = "MusicVolume";
    private const string SoundVolumeKey = "SoundVolume";
    private const string FullscreenKey = "IsFullscreen";

    // Этот метод вызывается, когда объект становится активным
    void OnEnable()
    {
        LoadSettings(); // Загружаем настройки при открытии меню
    }

    // Загрузка сохраненных настроек
    private void LoadSettings()
    {
        // Загружаем громкость, по умолчанию 0.75 (75%)
        musicSlider.value = PlayerPrefs.GetFloat(MusicVolumeKey, 0.75f);
        soundSlider.value = PlayerPrefs.GetFloat(SoundVolumeKey, 0.75f);

        // Загружаем режим экрана, по умолчанию включен (1)
        fullscreenToggle.isOn = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;

        // Сразу применяем загруженные значения
        SetMusicVolume(musicSlider.value);
        SetSoundVolume(soundSlider.value);
        SetFullscreen(fullscreenToggle.isOn);
    }

    // Метод для слайдера музыки
    public void SetMusicVolume(float volume)
    {
        // Audio Mixer использует логарифмическую шкалу (дБ), поэтому конвертируем значение
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
    }

    // Метод для слайдера звуков
    public void SetSoundVolume(float volume)
    {
        audioMixer.SetFloat("SoundVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(SoundVolumeKey, volume);
    }

    // Метод для переключателя полноэкранного режима
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0);
    }

    // Метод для кнопки "Назад"
    public void GoBack()
    {
        // Выключаем панель настроек
        gameObject.SetActive(false);

        // Если указана панель главного меню, включаем ее
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }
}