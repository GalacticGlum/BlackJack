using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioController : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioClip[] audioClips;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioClips = Resources.LoadAll<AudioClip>("Audio/Music");
    }

    private void Update()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(audioClips.Choose());
        }
    }
}
