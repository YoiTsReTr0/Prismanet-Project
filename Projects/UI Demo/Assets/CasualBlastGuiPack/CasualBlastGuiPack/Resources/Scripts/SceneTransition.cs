// Copyright (C) 2024 Nice Studio - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the Asset Store EULA is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CasualBlast
{
    public class SceneTransition : MonoBehaviour
    {
        // Name of the scene to transition to
        public string scene = "<Insert scene name>";

        // Duration of the transition
        public float duration = 0.5f;

        // Color used during the transition
        public Color color = Color.black;

        // Method to perform the scene transition
        public void PerformTransition()
        {
            // Call the LoadLevel method on the Transition class to change scenes
            Transition.LoadLevel(scene, duration, color);
        }
    }
}
