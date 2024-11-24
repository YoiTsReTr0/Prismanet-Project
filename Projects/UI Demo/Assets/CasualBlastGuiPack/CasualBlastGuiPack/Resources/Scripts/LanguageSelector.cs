// Copyright (C) 2024 Nice Studio - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the Asset Store EULA is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CasualBlast
{
    public class LanguageSelector : MonoBehaviour
    {
        // Array of buttons for language selection
        public Button[] languageButtons;
        // Panel for the pop-up
        public GameObject popupPanel;
        // OK and Cancel buttons on the pop-up
        public Button okButton;
        public Button cancelButton;
        // Default selected language button index
        public int defaultSelectedIndex = 0;

        // Current and temporary selected indices
        private int selectedIndex = -1;
        private int tempIndex = -1;

        void Start()
        {
            // Add click listeners to each language button
            for (int i = 0; i < languageButtons.Length; i++)
            {
                int index = i;  // Capture the current index in a local variable
                languageButtons[i].onClick.AddListener(() => OnLanguageButtonClick(index));
            }

            // Initially hide all borders and checks
            foreach (var button in languageButtons)
            {
                Transform border = button.transform.Find("Border");
                Transform check = button.transform.Find("Check");
                if (border != null) border.gameObject.SetActive(false);
                if (check != null) check.gameObject.SetActive(false);
            }

            // Add listeners to OK and Cancel buttons
            okButton.onClick.AddListener(OnOkButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);

            // Initially hide the pop-up panel
            popupPanel.SetActive(false);

            // Set the default selected button
            SetDefaultSelection();
        }

        // Set the default selected language button
        void SetDefaultSelection()
        {
            if (defaultSelectedIndex >= 0 && defaultSelectedIndex < languageButtons.Length)
            {
                Transform border = languageButtons[defaultSelectedIndex].transform.Find("Border");
                Transform check = languageButtons[defaultSelectedIndex].transform.Find("Check");
                if (border != null) border.gameObject.SetActive(true);
                if (check != null) check.gameObject.SetActive(true);
                selectedIndex = defaultSelectedIndex;
            }
        }

        // Handle language button click
        void OnLanguageButtonClick(int index)
        {
            tempIndex = index;
            popupPanel.SetActive(true);
        }

        // Handle OK button click
        void OnOkButtonClick()
        {
            if (selectedIndex >= 0)
            {
                // Remove the border and check from the previously selected button
                Transform prevBorder = languageButtons[selectedIndex].transform.Find("Border");
                Transform prevCheck = languageButtons[selectedIndex].transform.Find("Check");
                if (prevBorder != null) prevBorder.gameObject.SetActive(false);
                if (prevCheck != null) prevCheck.gameObject.SetActive(false);
            }

            // Add the border and check to the newly selected button
            Transform currBorder = languageButtons[tempIndex].transform.Find("Border");
            Transform currCheck = languageButtons[tempIndex].transform.Find("Check");
            if (currBorder != null) currBorder.gameObject.SetActive(true);
            if (currCheck != null) currCheck.gameObject.SetActive(true);

            selectedIndex = tempIndex;
            popupPanel.SetActive(false);
        }

        // Handle Cancel button click
        void OnCancelButtonClick()
        {
            tempIndex = -1;
            popupPanel.SetActive(false);
        }
    }
}
