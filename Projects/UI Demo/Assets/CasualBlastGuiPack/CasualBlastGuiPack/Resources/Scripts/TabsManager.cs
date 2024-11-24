// Copyright (C) 2024 Nice Studio - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the Asset Store EULA is available at http://unity3d.com/company/legal/as_terms.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CasualBlast
{
    public class TabsManager : MonoBehaviour
    {
        // Array of background images for tab buttons
        public Image[] TabButtonsBackgrounds;

        // Array of text components for tab buttons
        public TextMeshProUGUI[] TabTexts;

        // Colors for active and inactive tabs
        public Color ActiveTabColor, InactiveTabColor;

        // Colors for active and inactive tab texts
        public Color ActiveTextColor, InactiveTextColor;

        // Method to switch to a specified tab
        public void SwitchToTab(int id)
        {
            // Set all tab button backgrounds to inactive color
            foreach (var tab in TabButtonsBackgrounds)
            {
                tab.color = InactiveTabColor;
            }

            // Set all tab texts to inactive color
            foreach (var tabText in TabTexts)
            {
                tabText.color = InactiveTextColor;
            }

            // Set the selected tab button background to active color
            TabButtonsBackgrounds[id].color = ActiveTabColor;

            // Set the selected tab text to active color
            TabTexts[id].color = ActiveTextColor;
        }
    }
}
