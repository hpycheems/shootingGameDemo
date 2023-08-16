using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class AudioManager : MonoBehaviour
{
    public enum AudioChannel
    {
        Master,
        Music,
        SFX
    }

    public float masterVolumePercent { get; private set; }
    public float sfxVolumePercent { get; private set; }
    public float musicVolumePercent { get; private set; }

    private AudioSource sfx2DSource;
    private AudioSource[] musicSource;
    private int activeMusicSourceIndex;

    public static AudioManager instance;

    private Transform audioListenerPos;
    [SerializeField] private Transform playerT;
    private SoundLibrary soundLibrary;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            musicSource = new AudioSource[2];
            for (int i = 0; i < 2; i++)
            {
                GameObject newMusicSource = new GameObject("Music Source" + (i + 1));
                musicSource[i] = newMusicSource.AddComponent<AudioSource>();
                newMusicSource.transform.parent = transform;
            }
            GameObject sfx2DObject = new GameObject("sfx 2D Sound");
            sfx2DSource = sfx2DObject.AddComponent<AudioSource>();
            sfx2DObject.transform.parent = transform;

            audioListenerPos = transform.Find("AudioListener").transform;
            if(FindObjectOfType<Player>()!= null)
                playerT = FindObjectOfType<Player>().transform;
            soundLibrary = GetComponent<SoundLibrary>();
            
            masterVolumePercent = PlayerPrefs.GetFloat("master vol",1 );
            sfxVolumePercent = PlayerPrefs.GetFloat("sfx vol",1);
            musicVolumePercent = PlayerPrefs.GetFloat("music vol",1);
            //Debug.Log(masterVolumePercent + " " + musicVolumePercent + " " + musicVolumePercent);
        }
    }

    private void Update()
    {
        if(playerT != null)
            audioListenerPos.position = playerT.position;
    }

    public void SetVolume(float volumePercent, AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Master:
                masterVolumePercent = volumePercent;
                break;
            case AudioChannel.SFX:
                sfxVolumePercent = volumePercent;
                break;
            case AudioChannel.Music:
                musicVolumePercent = volumePercent;
                break;
        }

        musicSource[0].volume = musicVolumePercent * masterVolumePercent;
        musicSource[1].volume = sfxVolumePercent * masterVolumePercent;
        
        PlayerPrefs.SetFloat("master vol", masterVolumePercent);
        PlayerPrefs.SetFloat("sfx vol", sfxVolumePercent);
        PlayerPrefs.SetFloat("music vol", musicVolumePercent);
        PlayerPrefs.Save();
        
    }
    public void PlayMusic(AudioClip clip, float fadeDuration = 1)
    {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSource[activeMusicSourceIndex].clip = clip;
        musicSource[activeMusicSourceIndex].Play();

        StartCoroutine(AnimateMusicCrossFade(fadeDuration));
    }

    public void PlaySound(AudioClip clip, Vector3 pos)
    {
        if(clip != null)
            AudioSource.PlayClipAtPoint(clip, pos, sfxVolumePercent * masterVolumePercent);
    }

    public void PlaySound(string name, Vector3 pos)
    {
        PlaySound(soundLibrary.GetClipFromName(name), pos);
    }
    public void PlaySound2D(string name)
    {
        sfx2DSource.PlayOneShot(soundLibrary.GetClipFromName(name), sfxVolumePercent * masterVolumePercent);
    }

    IEnumerator AnimateMusicCrossFade(float duration)
    {
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * (1 / duration);
            musicSource[activeMusicSourceIndex].volume =
                Mathf.Lerp(0, musicVolumePercent * masterVolumePercent, percent);
            musicSource[1 - activeMusicSourceIndex].volume =
                Mathf.Lerp(musicVolumePercent * masterVolumePercent, 0, percent);
            yield return null;
        }
    }
}
