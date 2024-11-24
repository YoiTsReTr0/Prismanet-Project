// Copyright (C) 2024 Nice Studio - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the Asset Store EULA is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace CasualBlast {

    public class Popup : MonoBehaviour
    {
        private GameObject background;
        public bool isBackgroundOpen = true;

        public void Open()
        {
            if(isBackgroundOpen)
            {
                // Create a new GameObject for the background
                background = new GameObject("PopupBackground");
                background.transform.SetParent(transform.parent, false); // Set parent to the Canvas

                // Add Image component to the background GameObject
                Image backgroundImage = background.AddComponent<Image>();
                backgroundImage.color = new Color(0, 0, 0, 0.8f); // Set color to semi-transparent black

                // Set the background to cover the entire screen
                RectTransform rectTransform = background.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.offsetMin = new Vector2(0, 0);
                rectTransform.offsetMax = new Vector2(0, 0);

                // Move the background to be behind the popup
                background.transform.SetSiblingIndex(transform.GetSiblingIndex());
            }

            gameObject.SetActive(true); // Show the popup
        }

        public void Close()
        {
            var animator = GetComponent<Animator>();
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Open"))
                animator.Play("Close");

            StartCoroutine(RunPopupDestroy());
        }

        // Automatically destroys the popup 0.5 seconds after closing it.
        // The destruction is performed asynchronously via a coroutine. 
        private IEnumerator RunPopupDestroy()
        {
            yield return new WaitForSeconds(0.5f);
            Destroy(background); // Destroy the background GameObject
            Destroy(gameObject); // Destroy the popup GameObject
        }
    }
}