using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MainMenuMusic : MonoBehaviour
{
    [SerializeField] private AudioClip musicClip;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;

    private void Awake()
    {
        AudioSource audioSource = GetComponent<AudioSource>();

        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.volume = volume;
        audioSource.playOnAwake = false;

        if (musicClip != null)
            audioSource.Play();
        else
            Debug.LogWarning("Asigná una canción en el campo Music Clip del Inspector.");
    }
}