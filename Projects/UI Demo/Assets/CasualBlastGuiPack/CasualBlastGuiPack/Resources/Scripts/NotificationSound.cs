// Copyright (C) 2024 Nice Studio - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the Asset Store EULA is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace CasualBlast
{
    public class NotificationSound : MonoBehaviour
    {
        // Singleton instance of NotificationSound
        public static NotificationSound Instance;

        // AudioSource component
        private AudioSource m_audioSource;

        private void Start()
        {
            // Check if an instance of NotificationSound already exists
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
                // Set the notification volume based on PlayerPrefs
                m_audioSource.volume = PlayerPrefs.GetInt("notification_on");
            }
        }
    }
}
