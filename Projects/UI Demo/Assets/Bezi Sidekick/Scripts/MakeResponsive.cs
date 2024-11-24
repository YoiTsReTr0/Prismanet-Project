using UnityEngine;
using UnityEngine.UI;

public class MakeResponsive : MonoBehaviour
{
    void Start()
    {
        // Add CanvasScaler to the Canvas
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f; // Balances between width and height
        }
        
        // Iterate through all child elements and ensure proper anchor settings
        RectTransform[] rectTransforms = GetComponentsInChildren<RectTransform>(true);
        foreach (var rectTransform in rectTransforms)
        {
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
        }
    }
}