using System.Collections;
using System.Collections.Generic;
using TheraBytes.BetterUi;
using UnityEngine;

public class ToggleAccessibilityMode : MonoBehaviour 
{
    // called via Toggle event
    public void SetAccessibilityMode(bool isModeEnabled)
    {
        if(isModeEnabled)
        {
            ResolutionMonitor.AddScreenTag("Accessibility_Mode");
        }
        else
        {
            ResolutionMonitor.RemoveScreenTag("Accessibility_Mode");
        }
    }
}
