using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer audioMixer; // ������ �� ��� Audio Mixer
    public Slider musicSlider;    // ������� ��� ��������� ������
    public Slider soundSlider;    // ������� ��� ��������� ������

    [Header("Graphics")]
    public Toggle fullscreenToggle; // ������������� �������������� ������

    [Header("Menu Panels")]
    public GameObject mainMenuPanel; // ������ �������� ���� (�������������)

    // ����� ��� ���������� ��������
    private const string MusicVolumeKey = "MusicVolume";
    private const string SoundVolumeKey = "SoundVolume";
    private const string FullscreenKey = "IsFullscreen";

    // ���� ����� ����������, ����� ������ ���������� ��������
    void OnEnable()
    {
        LoadSettings(); // ��������� ��������� ��� �������� ����
    }

    // �������� ����������� ��������
    private void LoadSettings()
    {
        // ��������� ���������, �� ��������� 0.75 (75%)
        musicSlider.value = PlayerPrefs.GetFloat(MusicVolumeKey, 0.75f);
        soundSlider.value = PlayerPrefs.GetFloat(SoundVolumeKey, 0.75f);

        // ��������� ����� ������, �� ��������� ������� (1)
        fullscreenToggle.isOn = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;

        // ����� ��������� ����������� ��������
        SetMusicVolume(musicSlider.value);
        SetSoundVolume(soundSlider.value);
        SetFullscreen(fullscreenToggle.isOn);
    }

    // ����� ��� �������� ������
    public void SetMusicVolume(float volume)
    {
        // Audio Mixer ���������� ��������������� ����� (��), ������� ������������ ��������
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
    }

    // ����� ��� �������� ������
    public void SetSoundVolume(float volume)
    {
        audioMixer.SetFloat("SoundVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(SoundVolumeKey, volume);
    }

    // ����� ��� ������������� �������������� ������
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0);
    }

    // ����� ��� ������ "�����"
    public void GoBack()
    {
        // ��������� ������ ��������
        gameObject.SetActive(false);

        // ���� ������� ������ �������� ����, �������� ��
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }
}