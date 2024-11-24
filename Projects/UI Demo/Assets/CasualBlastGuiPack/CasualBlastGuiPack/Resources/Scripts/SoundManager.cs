// Copyright (C) 2024 Nice Studio - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the Asset Store EULA is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CasualBlast
{
    public class SoundManager : MonoBehaviour
    {
        // References to the sound on and off images
        public GameObject soundOnImage;
        public GameObject soundOffImage;

        // Variable to store sound on/off state
        private int isSoundOn;

        private void Start()
        {
            // Get the sound state from PlayerPrefs
            isSoundOn = PlayerPrefs.GetInt("sound_on");

            // Set the appropriate image active based on the sound state
            if (isSoundOn == 1)
            {
                soundOnImage.SetActive(true);
                soundOffImage.SetActive(false);
            }
            else
            {
                soundOnImage.SetActive(false);
                soundOffImage.SetActive(true);
            }
        }

        // Method to switch the sound state
        public void SwitchSound()
        {
            // Get the current sound state from PlayerPrefs
            isSoundOn = PlayerPrefs.GetInt("sound_on");

            // Toggle the sound state
            if (isSoundOn == 1)
            {
                // Turn off the sound
                soundOnImage.SetActive(false);
                soundOffImage.SetActive(true);
                PlayerPrefs.SetInt("sound_on", 0);
                AudioListener.volume = 0;
            }
            else
            {
                // Turn on the sound
                soundOnImage.SetActive(true);
                soundOffImage.SetActive(false);
                PlayerPrefs.SetInt("sound_on", 1);
                AudioListener.volume = 1;
            }
        }
    }
}
