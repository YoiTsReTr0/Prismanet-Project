using UnityEngine;
using System;

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "ScriptableObjects/AudioLibrary")]
public class AudioLibrary : ScriptableObject
{
    public AudioClipDictionaryElement[] audioClips;
	
    public AudioClip GetAudioClipByKey(SoundKey key)
    {
        for(int i = 0; i < audioClips.Length; i++)
        {
            if (audioClips[i].key == key)
            {
                return audioClips[i].clip;
            }
        }

        return null;
    }
}

[Serializable]
public struct AudioClipDictionaryElement
{
    public SoundKey key;
    public AudioClip clip;
}

public enum SoundKey
{
    ButtonClick,

    LevelWin,

    Error,

    

}