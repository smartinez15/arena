using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum AudioChannel
    {
        Master,
        SFX,
        Music
    };

    public float masterVolume { get; private set; }
    public float sfxVolume { get; private set; }
    public float musicVolume { get; private set; }

    AudioSource sfx2DSource;
    AudioSource[] musicSource;
    int activeMusicSource;
    Transform audioListener;
    Transform playerT;
    SoundLibrary library;

    public static AudioManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource = new AudioSource[2];
            for (int i = 0; i < musicSource.Length; i++)
            {
                GameObject newMusicSource = new GameObject("MusicSource " + (i + 1));
                musicSource[i] = newMusicSource.AddComponent<AudioSource>();
                newMusicSource.transform.parent = transform;
            }

            GameObject sfx2DS = new GameObject("SFX_Source");
            sfx2DSource = sfx2DS.AddComponent<AudioSource>();
            sfx2DS.transform.parent = transform;

            audioListener = FindObjectOfType<AudioListener>().transform;
            if (FindObjectOfType<Player>() != null)
            {
                playerT = FindObjectOfType<Player>().transform;
            }
            library = GetComponent<SoundLibrary>();

            masterVolume = PlayerPrefs.GetFloat("Master Volume", 1);
            sfxVolume = PlayerPrefs.GetFloat("SFX Volume", 1);
            musicVolume = PlayerPrefs.GetFloat("Music Volume", 0);
            PlayerPrefs.Save();
        }
    }

    void Update()
    {
        if (playerT != null)
        {
            audioListener.position = playerT.position;
        }
    }

    public void SetVolume(float volume, AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Master:
                masterVolume = volume;
                break;
            case AudioChannel.SFX:
                sfxVolume = volume;
                break;
            case AudioChannel.Music:
                musicVolume = volume;
                break;
        }

        musicSource[0].volume = musicVolume * masterVolume;
        musicSource[1].volume = musicVolume * masterVolume;

        PlayerPrefs.SetFloat("Master Volume", masterVolume);
        PlayerPrefs.SetFloat("SFX Volume", sfxVolume);
        PlayerPrefs.SetFloat("Music Volume", musicVolume);
        PlayerPrefs.Save();
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1)
    {
        activeMusicSource = 1 - activeMusicSource;
        musicSource[activeMusicSource].clip = clip;
        musicSource[activeMusicSource].Play();

        StartCoroutine(MusicCrossfade(fadeDuration));
    }

    public void PlaySound(AudioClip clip, Vector3 pos)
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, pos, sfxVolume * masterVolume);
    }

    public void PlaySound(string clipName, Vector3 pos)
    {
        AudioSource.PlayClipAtPoint(library.GetClip(clipName), pos, sfxVolume * masterVolume);
    }

    public void PlaySound2D(string clipName)
    {
        sfx2DSource.PlayOneShot(library.GetClip(clipName), sfxVolume * masterVolume);
    }

    IEnumerator MusicCrossfade(float duration)
    {
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / duration;
            musicSource[activeMusicSource].volume = Mathf.Lerp(0, musicVolume * masterVolume, percent);
            musicSource[1 - activeMusicSource].volume = Mathf.Lerp(musicVolume * masterVolume, 0, percent);
            yield return null;
        }
    }

    void OnLevelWasLoaded(int index)
    {
        if (playerT == null)
        {
            if (FindObjectOfType<Player>() != null)
            {
                playerT = FindObjectOfType<Player>().transform;
            }
        }
    }
}
