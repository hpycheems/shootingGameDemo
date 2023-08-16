using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioClip menuTheme;
    public AudioClip mainTheme;

    private string sceneName;
    private void Start()
    {
        OnLevelWaveLoaded(0);
    }

    void OnLevelWaveLoaded(int sceneIndex)
    {
        string newSceneName = SceneManager.GetActiveScene().name;
        if (newSceneName != sceneName)
        {
            sceneName = newSceneName;
            Invoke(nameof(PlayMusic),.2f);
        }
    }

    void PlayMusic()
    {
        AudioClip clip = null;
        if (sceneName == "Game")
        {
            clip = mainTheme;
        }
        else if (sceneName == "MainMenu")
        {
            clip = menuTheme;
        }

        if (clip != null)
        {
            AudioManager.instance.PlayMusic(clip, 2);
            Invoke(nameof(PlayMusic), clip.length);
        }
    }
}
