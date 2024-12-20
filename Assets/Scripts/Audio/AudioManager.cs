using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    private AudioSource audioSource;

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
}
