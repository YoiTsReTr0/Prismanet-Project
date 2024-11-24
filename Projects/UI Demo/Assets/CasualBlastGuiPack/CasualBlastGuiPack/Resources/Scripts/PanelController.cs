using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CasualBlast
{
    public class PanelController : MonoBehaviour
    {
        // Reference to the Animator component
        public Animator animator;

        // Variable to track if the panel is expanded or collapsed
        private bool isExpanded = false;

        // Method to toggle the panel's state
        public void TogglePanel()
        {
            if (isExpanded)
            {
                // Trigger the collapse animation
                animator.SetTrigger("Collapse");
            }
            else
            {
                // Trigger the expand animation
                animator.SetTrigger("Expand");
            }
            // Toggle the isExpanded state
            isExpanded = !isExpanded;
        }
    }
}
