using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MenuManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject settingsMenu;

    public Slider[] volumeSliders;
    public Toggle[] resolutionsToggles;
    public int[] screenWidths;
    int activeRes;

    void Start()
    {
        activeRes = PlayerPrefs.GetInt("ScreenResIndex");
        bool isFS = PlayerPrefs.GetInt("fullscreen") == 1;

        volumeSliders[0].value = AudioManager.instance.masterVolume;
        volumeSliders[1].value = AudioManager.instance.musicVolume;
        volumeSliders[2].value = AudioManager.instance.sfxVolume;

        for (int i = 0; i < resolutionsToggles.Length; i++)
        {
            resolutionsToggles[i].isOn = (i == activeRes);
        }

        SetFullscreen(isFS);
    }

    public void Play()
    {
        SceneManager.LoadScene("Game");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Settings()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void MainMenu()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }

    public void SetScreenResolution(int i)
    {
        if (resolutionsToggles[i].isOn)
        {
            activeRes = i;
            float ratio = 16 / 9f;
            Screen.SetResolution(screenWidths[i], (int)(screenWidths[i] / ratio), false);
            PlayerPrefs.SetInt("ScreenResIndex", activeRes);
            PlayerPrefs.Save();
        }
    }

    public void SetFullscreen(bool fs)
    {
        for (int i = 0; i < resolutionsToggles.Length; i++)
        {
            resolutionsToggles[i].interactable = !fs;
        }

        if (fs)
        {
            Resolution[] allResolutions = Screen.resolutions;
            Resolution maxRes = allResolutions[allResolutions.Length - 1];
            Screen.SetResolution(maxRes.width, maxRes.height, true);
        }
        else
        {
            SetScreenResolution(activeRes);
        }

        PlayerPrefs.SetInt("Fullscreen", fs ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float volume)
    {
        AudioManager.instance.SetVolume(volume, AudioManager.AudioChannel.Master);
    }

    public void SetMusicVolume(float volume)
    {
        AudioManager.instance.SetVolume(volume, AudioManager.AudioChannel.Music);
    }

    public void SetSFXVolume(float volume)
    {
        AudioManager.instance.SetVolume(volume, AudioManager.AudioChannel.SFX);
    }
}
