using UnityEngine;
using System.Collections;

public class BackgroundSoundManager : MonoBehaviour
{
    public static BackgroundSoundManager Instance { get; private set; }

    public AudioSource musicSource;
    public float fadeDuration = 1.5f;

    private Coroutine currentFadeCoroutine;
    private float defaultVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        defaultVolume = musicSource.volume;
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        if (musicSource.isPlaying && musicSource.clip == clip)
            return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = defaultVolume;
        musicSource.Play();
    }

    public void CrossfadeTo(AudioClip newClip, bool loop = true)
    {
        if (newClip == null || musicSource.clip == newClip)
            return;

        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        currentFadeCoroutine = StartCoroutine(FadeMusic(newClip, loop));
    }

    public void StopMusic()
    {
        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        StartCoroutine(FadeOutAndStop());
    }

    public void SetVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume);
        defaultVolume = musicSource.volume;
    }


    private IEnumerator FadeMusic(AudioClip newClip, bool loop)
    {
        float startVolume = musicSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();

        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();

        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, defaultVolume, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = defaultVolume;
    }

    private IEnumerator FadeOutAndStop()
    {
        float startVolume = musicSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = defaultVolume;
    }
}
