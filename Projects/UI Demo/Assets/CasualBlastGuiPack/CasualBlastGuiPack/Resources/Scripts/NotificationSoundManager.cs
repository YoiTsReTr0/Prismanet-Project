// Copyright (C) 2024 Nice Studio - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the Asset Store EULA is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CasualBlast
{
    public class NotificationSoundManager : MonoBehaviour
    {
        // References to the notification sound on and off images
        public GameObject notificationSoundOnImage;
        public GameObject notificationSoundOffImage;

        // Variable to store notification sound on/off state
        private int isSoundOn;

        private void Start()
        {
            // Get the notification sound state from PlayerPrefs
            isSoundOn = PlayerPrefs.GetInt("notification_on");

            // Set the appropriate image active based on the notification sound state
            if (isSoundOn == 1)
            {
                notificationSoundOnImage.SetActive(true);
                notificationSoundOffImage.SetActive(false);
            }
            else
            {
                notificationSoundOnImage.SetActive(false);
                notificationSoundOffImage.SetActive(true);
            }
        }

        // Method to switch the notification sound state
        public void SwitchSound()
        {
            // Get the current notification sound state from PlayerPrefs
            isSoundOn = PlayerPrefs.GetInt("notification_on");

            // Toggle the notification sound state
            if (isSoundOn == 1)
            {
                // Turn off the notification sound
                notificationSoundOnImage.SetActive(false);
                notificationSoundOffImage.SetActive(true);
                PlayerPrefs.SetInt("notification_on", 0);
                AudioListener.volume = 0;
            }
            else
            {
                // Turn on the notification sound
                notificationSoundOnImage.SetActive(true);
                notificationSoundOffImage.SetActive(false);
                PlayerPrefs.SetInt("notification_on", 1);
                AudioListener.volume = 1;
            }
        }
    }
}
