using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public AudioClip defaultBGM;
    public AudioClip receptionBGM;
    public AudioClip homeBGM;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // 根据场景选择BGM
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Reception")
            audioSource.clip = receptionBGM;
        else if (sceneName == "Home")
            audioSource.clip = homeBGM;
        else
            audioSource.clip = defaultBGM;

        audioSource.loop = true;
        audioSource.playOnAwake = true;
        audioSource.volume = 0.5f;
        audioSource.Play();
    }
}
