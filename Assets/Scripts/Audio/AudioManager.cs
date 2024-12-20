using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    private AudioSource audioSource;
    private bool isPlayingBackgroundMusic = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            audioSource = gameObject.AddComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void PlayClip(AudioClip clip)
    {
        if (instance != null && instance.audioSource != null && clip != null)
        {
            instance.audioSource.PlayOneShot(clip);
        }
    }

    public static void PlayBackgroundMusic(AudioClip clip, bool loop = true)
    {
        if (instance != null && instance.audioSource != null)
        {
            if (clip != null && instance.audioSource.clip != clip)
            {
                instance.audioSource.clip = clip;
                instance.audioSource.loop = loop;
                instance.audioSource.Play();
                instance.isPlayingBackgroundMusic = true;
            }
        }
    }

    public static void StopBackgroundMusic()
    {
        if (instance != null && instance.audioSource != null && instance.isPlayingBackgroundMusic)
        {
            instance.audioSource.Stop();
            instance.audioSource.clip = null;
            instance.isPlayingBackgroundMusic = false;
        }
    }
}
