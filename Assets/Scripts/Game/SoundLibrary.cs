using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour
{
    public SoundGroup[] soundGroups;

    Dictionary<string, AudioClip[]> library = new Dictionary<string, AudioClip[]>();

    void Awake()
    {
        foreach (SoundGroup group in soundGroups)
        {
            library.Add(group.groupID, group.audioClips);
        }
    }

    public AudioClip GetClip(string clipName)
    {
        if (library.ContainsKey(clipName))
        {
            AudioClip[] sounds = library[clipName];
            return sounds[Random.Range(0, sounds.Length)];
        }

        return null;
    }

    [System.Serializable]
    public class SoundGroup
    {
        public string groupID;
        public AudioClip[] audioClips;
    }
}
