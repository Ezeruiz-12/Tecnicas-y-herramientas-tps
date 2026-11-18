using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class MainMenuMusic : MonoBehaviour
{
    [Header("Música")]
    [SerializeField] private AudioClip musicClip;

    [Header("Barra de volumen en Options")]
    [SerializeField] private Slider volumeSlider;

    private const string VolumeKey = "GameVolume";
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        AudioListener.volume = savedVolume;

        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(savedVolume);

        if (musicClip != null)
        {
            audioSource.clip = musicClip;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.Play();
        }
    }

    private void Start()
    {
        if (volumeSlider == null)
        {
            Debug.LogWarning("Arrastrá el Slider de Options al campo Volume Slider.");
            return;
        }

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.onValueChanged.AddListener(ChangeVolume);
    }

    public void ChangeVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(ChangeVolume);
    }
}