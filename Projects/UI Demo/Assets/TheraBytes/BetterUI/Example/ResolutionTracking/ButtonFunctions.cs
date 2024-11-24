using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi.Examples.ResolutionTracking
{
    public class ButtonFunctions : MonoBehaviour
    {
        [SerializeField] Image toggleImage;
        [SerializeField] Sprite makeFullscreenSprite;
        [SerializeField] Sprite makeWindowedSprite;

        // called through event on selectable
        // This is called on a custom event "Pointer Down" because only this way it can toggle full screen immediately (in WebGL)
        // If a toggle would be used instead the change would trigger after another click (where it the game is focused again).
        // it works on Pointer Down because the window will be focused on pointer up again.
        public void ToggleFullscreen()
        {
#if UNITY_EDITOR
            Debug.LogFormat("Fullscreen cannot be toggled inside Unity (in builds only).");
#endif
            bool changeToFullScreen = !Screen.fullScreen;
            Screen.fullScreen = changeToFullScreen;

            toggleImage.sprite = (changeToFullScreen)
                ? makeWindowedSprite
                : makeFullscreenSprite;
        }
    }
}
