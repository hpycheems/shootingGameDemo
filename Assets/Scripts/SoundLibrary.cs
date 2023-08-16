using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SoundLibrary : MonoBehaviour
{
    public SoundGroup[] soundGroups;

    private Dictionary<string, AudioClip[]> groupDictionary = new Dictionary<string, AudioClip[]>();

    private void Awake()
    {
        foreach (var VARIABLE in soundGroups)
        {
            groupDictionary[VARIABLE.groupID] = VARIABLE.group;
        }
    }

    public AudioClip GetClipFromName(string name)
    {
        if (groupDictionary.ContainsKey(name))
        {
            AudioClip[] sounds = groupDictionary[name];
            return sounds[Random.Range(0, sounds.Length)];
        }
        return null;
    }

    [Serializable]
    public class SoundGroup
    {
        public string groupID;
        public AudioClip[] group;
    }
}
