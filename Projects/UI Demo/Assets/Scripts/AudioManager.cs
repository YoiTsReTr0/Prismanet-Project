using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioLibrary audioLibrary;


    [Tooltip("Source for non-looped sounds")]
    public AudioSource nonLoopedAudioSource;
    [Tooltip("Music source")]
    public AudioSource musicAudioSource;

    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip inGameMusic;
    [SerializeField] private AudioClip titleScreenMusic;
    private float maxVolume = 0.7f;

    private bool musicIsFading;
    private IEnumerator MusicAppear()
    {
        yield return new WaitUntil(() => musicIsFading == false);

        yield return new WaitForSeconds(0.1f);

        musicAudioSource.volume = 0;
        musicAudioSource.Play();
        while (musicAudioSource.volume < maxVolume)
        {
            musicAudioSource.volume += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public AudioClip bgClip;

	private void Start()
	{

        SetBGMusic();
    }

     public void SetBGMusic()
    {
      
            maxVolume = 0.7f;
            musicAudioSource.clip = bgClip;

        StartCoroutine(MusicAppear());

    }


     public void PlayAudioClipByKey(SoundKey key)
    {
        nonLoopedAudioSource.PlayOneShot(audioLibrary.GetAudioClipByKey(key));
    }
}
