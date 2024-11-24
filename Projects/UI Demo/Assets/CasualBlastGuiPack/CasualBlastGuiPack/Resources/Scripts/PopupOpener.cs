// Copyright (C) 2024 Nice Studio - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the Asset Store EULA is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;

namespace CasualBlast
{
    public class PopupOpener : MonoBehaviour
    {
        // Reference to the popup prefab
        public GameObject popupPrefab;

        // Reference to the canvas
        protected Canvas m_canvas;

        protected void Start()
        {
            // Find and get the Canvas component
            m_canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            Assert.IsNotNull(m_canvas, "Canvas is not found.");
        }

        // Method to open the popup
        public virtual void OpenPopup()
        {
            // Ensure the popup prefab is assigned
            Assert.IsNotNull(popupPrefab, "Popup prefab is not assigned.");

            // Instantiate the popup prefab
            var popup = Instantiate(popupPrefab) as GameObject;
            Assert.IsNotNull(popup, "Failed to instantiate popup.");

            // Activate the popup
            popup.SetActive(true);

            // Set the popup's scale and parent
            popup.transform.localScale = Vector3.one;
            popup.transform.SetParent(m_canvas.transform, false);

            // Call the Open method on the Popup component
            popup.GetComponent<Popup>().Open();
        }
    }
}
