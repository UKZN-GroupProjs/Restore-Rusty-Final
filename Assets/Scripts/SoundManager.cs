using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance; 

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;  // For short sound effects

    [Header("Clips")]
    public AudioClip jumpClip;
    public AudioClip deathClip;
    public AudioClip collectClip;
    public AudioClip npcTalkClip;
    public AudioClip hitTakenClip;
    

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }
}
