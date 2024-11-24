// Copyright (C) 2024 Nice Studio - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the Asset Store EULA is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CasualBlast
{
    public class MusicManager : MonoBehaviour
    {
        // References to the music on and off images
        public GameObject musicOnImage;
        public GameObject musicOffImage;

        // Variable to store music on/off state
        private int isMusicOn;

        private void Start()
        {
            // Get the music state from PlayerPrefs
            isMusicOn = PlayerPrefs.GetInt("music_on");

            // Set the appropriate image active based on the music state
            if (isMusicOn == 1)
            {
                musicOnImage.SetActive(true);
                musicOffImage.SetActive(false);
            }
            else
            {
                musicOnImage.SetActive(false);
                musicOffImage.SetActive(true);
            }
        }

        // Method to switch the music state
        public void SwitchMusic()
        {
            // Get the current music state from PlayerPrefs
            isMusicOn = PlayerPrefs.GetInt("music_on");

            // Find the background music AudioSource
            var backgroundAudioSource = GameObject.Find("Background-Music").GetComponent<AudioSource>();

            // Toggle the music state
            if (isMusicOn == 1)
            {
                // Turn off the music
                musicOnImage.SetActive(false);
                musicOffImage.SetActive(true);
                PlayerPrefs.SetInt("music_on", 0);
                backgroundAudioSource.volume = 0;
            }
            else
            {
                // Turn on the music
                musicOnImage.SetActive(true);
                musicOffImage.SetActive(false);
                PlayerPrefs.SetInt("music_on", 1);
                backgroundAudioSource.volume = 1;
            }
        }
    }
}
