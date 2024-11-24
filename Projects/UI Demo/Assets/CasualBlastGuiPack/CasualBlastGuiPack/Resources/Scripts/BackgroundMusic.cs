// Copyright (C) 2024 Nice Studio - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the Asset Store EULA is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;
using UnityEngine;

namespace CasualBlast
{
    public class BackgroundMusic : MonoBehaviour
    {
        // Singleton instance of BackgroundMusic
        public static BackgroundMusic Instance;

        // AudioSource component
        private AudioSource m_audioSource;

        private void Start()
        {
            // Check if an instance of BackgroundMusic already exists
            if (Instance != null)
            {
                // If so, destroy this duplicate instance
                DestroyImmediate(gameObject);
            }
            else
            {
                // If not, set this as the instance and don't destroy it on load
                DontDestroyOnLoad(gameObject);
                Instance = this;
                // Get the AudioSource component
                m_audioSource = GetComponent<AudioSource>();
                // Ignore the listener volume
                m_audioSource.ignoreListenerVolume = true;
                // Set the music volume based on PlayerPrefs
                m_audioSource.volume = PlayerPrefs.GetInt("music_on");
                // Set the sound volume based on PlayerPrefs
                AudioListener.volume = PlayerPrefs.GetInt("sound_on");
            }
        }

        // Fade in the audio
        public void FadeIn()
        {
            // Check if music is enabled in PlayerPrefs
            if (PlayerPrefs.GetInt("music_on") == 1)
            {
                // Start the fade-in coroutine
                StartCoroutine(FadeAudio(1.0f, Fade.In));
            }
        }

        // Fade out the audio
        public void FadeOut()
        {
            // Check if music is enabled in PlayerPrefs
            if (PlayerPrefs.GetInt("music_on") == 1)
            {
                // Start the fade-out coroutine
                StartCoroutine(FadeAudio(1.0f, Fade.Out));
            }
        }

        // Enum to define fade types
        private enum Fade
        {
            In,
            Out
        }

        // Coroutine to handle audio fading
        private IEnumerator FadeAudio(float time, Fade fadeType)
        {
            // Determine start and end values based on fade type
            var start = fadeType == Fade.In ? 0.0f : 1.0f;
            var end = fadeType == Fade.In ? 1.0f : 0.0f;
            var i = 0.0f;
            var step = 1.0f / time;

            // Gradually change the volume
            while (i <= 1.0f)
            {
                i += step * Time.deltaTime;
                m_audioSource.volume = Mathf.Lerp(start, end, i);
                // Wait for the next frame
                yield return new WaitForSeconds(step * Time.deltaTime);
            }
        }
    }
}
