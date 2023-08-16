using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenuHolder;
    public GameObject operationMenuHolder;

    public Slider[] volumeSliders;
    public Toggle[] resolutionToggles;
    public Toggle fullScrren;
    public int[] screenWidths;
    private int activeScreenIndex;

    private void Start()
    {
        activeScreenIndex = PlayerPrefs.GetInt("screen res index");
        bool isFullscreen = (PlayerPrefs.GetInt("isfullscreen") == 1);

        volumeSliders[0].value = AudioManager.instance.masterVolumePercent;
        volumeSliders[1].value = AudioManager.instance.musicVolumePercent;
        volumeSliders[2].value = AudioManager.instance.sfxVolumePercent;

        for (int i = 0; i < resolutionToggles.Length; i++)
        {
            resolutionToggles[i].isOn = (i == activeScreenIndex);
        }

        fullScrren.isOn = isFullscreen;// SetFullScreen(isFullscreen);
    }

    public void Play()
    {
        SceneManager.LoadScene("Game");
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void OperationMenu()
    {
        mainMenuHolder.SetActive(false);
        operationMenuHolder.SetActive(true);
    }
    public void MainMenu()
    {
        mainMenuHolder.SetActive(true);
        operationMenuHolder.SetActive(false);
    }
    public void SetScreenResolution(int index)
    {
        if (resolutionToggles[index].isOn)
        {
            activeScreenIndex = index;
            float aspectRatio = 16 / 9f;
            Screen.SetResolution(screenWidths[index], (int)(screenWidths[index] / aspectRatio), false);
            PlayerPrefs.SetInt("screen res index", activeScreenIndex);
            PlayerPrefs.Save();
        }
    }
    public void SetFullScreen(bool isFullScreen)
    {
        for (int i = 0; i < resolutionToggles.Length; i++)
        {
            resolutionToggles[i].interactable = !isFullScreen;
        }

        if (isFullScreen)
        {
            Resolution[] allResolutions = Screen.resolutions;
            Resolution maxResolution = allResolutions[allResolutions.Length - 1];
            Screen.SetResolution(maxResolution.width, maxResolution.height, true);
        }
        else
        {
            SetScreenResolution(activeScreenIndex);
        }
        PlayerPrefs.SetInt("isfullscreen", 1);
        PlayerPrefs.Save();
    }

    public void SetMaterVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Master);
    }
    public void SetMusicVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Music);
    }
    public void SetSFXVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.SFX);
    }
}
