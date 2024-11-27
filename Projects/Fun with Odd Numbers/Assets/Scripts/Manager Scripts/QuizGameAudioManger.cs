using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace QuizGame.LevelPlay
{
    public class QuizGameAudioManger : MonoBehaviour
    {
        public static QuizGameAudioManger instance;

        [SerializeField] private AudioSource[] _bgAudioSources;
        [SerializeField] private AudioSource _sfxSourceHigh;
        [SerializeField] private AudioSource _sfxSourceLow;

        [Header("Audio Clips")] public AudioClip BGMusicLoop;
        public AudioClip CoinRewardClip;
        public AudioClip CorrAnswerClip;
        public AudioClip WrongAnswerClip;
        public AudioClip StarCollectClip;
        public AudioClip FishChoicesAppearClip;
        public AudioClip GameOverClip;

        private void Awake()
        {
            if (instance == null)
                instance = this;

            else
                Destroy(gameObject);
        }

        private void Start()
        {
            _bgAudioSources[0].clip = BGMusicLoop;

            for (int i = 0; i < _bgAudioSources.Length; i++)
            {
                _bgAudioSources[i]?.Play();
            }
        }

        public void PlaySFXOneShotHigh(AudioClip clip)
        {
            _sfxSourceHigh?.PlayOneShot(clip);
            Debug.Log($"Sound clip {clip.name.ToString()} player");
        }
        
        public void PlaySFXOneShotLow(AudioClip clip)
        {
            _sfxSourceLow?.PlayOneShot(clip);
            Debug.Log($"Sound clip {clip.name.ToString()} player");
        }

        public void StopAllAmbient()
        {
            for (int i = 0; i < _bgAudioSources.Length; i++)
            {
                if (i > 0)
                    _bgAudioSources[i]?.DOFade(0, 1.75f);
            }
        }
    }
}