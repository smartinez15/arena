using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioClip theme;
    public AudioClip menu;

    string sceneName;

    void Start()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
        OnLevelFinishedLoading(0);
    }

    void OnLevelFinishedLoading(int sceneIndex)
    {
        string newScene = SceneManager.GetActiveScene().name;
        if (newScene != sceneName)
        {
            sceneName = newScene;
            Invoke("PlayMusic", 0.2f);
        }
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode sceneMode)
    {
        string newScene = SceneManager.GetActiveScene().name;
        if (newScene != sceneName)
        {
            sceneName = newScene;
            Invoke("PlayMusic", 0.2f);
        }
    }

    void PlayMusic()
    {
        AudioClip clipToPlay = null;

        if (sceneName == "Menu")
        {
            clipToPlay = menu;
        }
        else if (sceneName == "Game")
        {
            clipToPlay = theme;
        }

        if (clipToPlay != null)
        {
            AudioManager.instance.PlayMusic(clipToPlay, 2);
            Invoke("PlayMusic", clipToPlay.length);
        }
    }
}
