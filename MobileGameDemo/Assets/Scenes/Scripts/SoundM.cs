using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SoundM : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;

    [Header("Buttons")]
    [SerializeField] private Button musicButton;
    [SerializeField] private Button soundButton;

    [Header("Music Sprites")]
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;

    [Header("Sound Sprites")]
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    private bool isMusicMuted;
    private bool isSoundMuted;

    private const string MusicMutedKey = "MusicMuted";
    private const string SoundMutedKey = "SoundMuted";
    
    private const string VolumeKey = "MasterVolumeValue";
    private const string MixerVolumeParameter = "MasterVolume";

    private void Start()
    {
        isMusicMuted = PlayerPrefs.GetInt(MusicMutedKey, 0) == 1;
        isSoundMuted = PlayerPrefs.GetInt(SoundMutedKey, 0) == 1;

        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);

        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(savedVolume);
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        SetVolume(savedVolume);

        ApplyMusicState();
        ApplySoundState();
    }

    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;

        PlayerPrefs.SetInt(MusicMutedKey, isMusicMuted ? 1 : 0);
        PlayerPrefs.Save();

        ApplyMusicState();
    }

    public void ToggleSound()
    {
        isSoundMuted = !isSoundMuted;

        PlayerPrefs.SetInt(SoundMutedKey, isSoundMuted ? 1 : 0);
        PlayerPrefs.Save();

        ApplySoundState();
    }

    public void SetVolume(float value)
{
    value = Mathf.Clamp(value, 0.0001f, 1f);

    float volumeInDecibels = Mathf.Log10(value) * 20f;

    if (audioMixer != null)
    {
        audioMixer.SetFloat(MixerVolumeParameter, volumeInDecibels);
    }

    PlayerPrefs.SetFloat(VolumeKey, value);
    PlayerPrefs.Save();
}

    private void ApplyMusicState()
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.mute = isMusicMuted;
        }

        if (musicButton == null)
        {
            Debug.LogWarning("Music Button bağlanmamış!");
            return;
        }

        Image buttonImage = musicButton.GetComponent<Image>();

        if (buttonImage != null)
        {
            buttonImage.sprite =
                isMusicMuted ? musicOffSprite : musicOnSprite;
        }
    }

 

    private void ApplySoundState()
    {
        AudioListener.volume = isSoundMuted ? 0f : 1f;

        if (soundButton == null)
        {
            Debug.LogWarning("Sound Button bağlanmamış!");
            return;
        }

        Image buttonImage = soundButton.GetComponent<Image>();

        if (buttonImage != null)
        {
            buttonImage.sprite =
                isSoundMuted ? soundOffSprite : soundOnSprite;
        }

        if (volumeSlider != null)
        {
            volumeSlider.interactable = !isSoundMuted;
        }
    }
}