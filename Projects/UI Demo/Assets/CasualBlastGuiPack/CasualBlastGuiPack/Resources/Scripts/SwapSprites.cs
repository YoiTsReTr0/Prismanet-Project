// Copyright (C) 2024 Nice Studio - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the Asset Store EULA is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CasualBlast
{
    public class SwapSprites : MonoBehaviour
    {
        // References to the sprites for enabled and disabled states
        public Sprite enabledSprite;
        public Sprite disabledSprite;

        // Variable to track the current state
        private bool isSwapped = true;

        // Reference to the Image component
        private Image image;

        public void Awake()
        {
            // Get the Image component
            image = GetComponent<Image>();
        }

        // Method to swap the sprite
        public void SwapSprite()
        {
            if (isSwapped)
            {
                // Set the sprite to disabled and update the state
                isSwapped = false;
                image.sprite = disabledSprite;
            }
            else
            {
                // Set the sprite to enabled and update the state
                isSwapped = true;
                image.sprite = enabledSprite;
            }
        }
    }
}
