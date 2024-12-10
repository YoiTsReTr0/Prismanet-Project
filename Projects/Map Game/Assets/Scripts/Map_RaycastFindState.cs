using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Map_RaycastFindState : MonoBehaviour
{
    [SerializeField] private int MaxChecks = 7;
    [SerializeField] private float MaxDuration = 5f;
    private GraphicRaycaster _raycaster;
    private EventSystem _eventSystem;

    private Map_UIManager _uiManager;

    void Start()
    {
        _raycaster = FindObjectOfType<GraphicRaycaster>();
        _eventSystem = FindObjectOfType<EventSystem>();

        _uiManager = Map_UIManager.instance;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastAtMouse();
        }
    }

    private void RaycastAtMouse()
    {
        PointerEventData pointerEventData = new PointerEventData(_eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        _raycaster.Raycast(pointerEventData, results);

        int checks = 0;
        float startTime = Time.time;

        foreach (RaycastResult result in results)
        {
            if (checks >= MaxChecks || (Time.time - startTime) >= MaxDuration)
            {
                Debug.Log("Raycast check limit reached or timed out.");
                return;
            }

            GameObject hitObject = result.gameObject;

            if (hitObject.tag != "StateRaycastAble")
            {
                Debug.Log($"Raycast hit object {hitObject.name} with a non-HAHA tag. Stopping further checks.");
                return;
            }

            Image image = hitObject.GetComponent<Image>();
            if (image != null && IsVisiblePart(image, pointerEventData))
            {
                Debug.Log($"Clicked on visible part of {hitObject.name}");
                _uiManager.UIM_SelectStateObject?.Invoke(hitObject.GetComponent<Map_StateObject>());
                return;
            }
            else
            {
                Debug.Log($"Transparent part clicked on {hitObject.name}");
            }

            checks++;
        }

        Debug.Log("No valid clickable image found.");
    }

    private bool IsVisiblePart(Image image, PointerEventData eventData)
    {
        if (image.sprite == null) return false;

        RectTransform rectTransform = image.rectTransform;
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position,
            eventData.pressEventCamera, out localPoint);

        Rect rect = rectTransform.rect;
        Vector2 normalizedPoint =
            new Vector2((localPoint.x - rect.x) / rect.width, (localPoint.y - rect.y) / rect.height);

        Texture2D texture = image.sprite.texture;

        if (!texture.isReadable)
        {
            Debug.LogWarning($"Texture on {image.gameObject.name} is not readable!");
            return false;
        }

        Vector2 pixelCoord = new Vector2(normalizedPoint.x * texture.width, normalizedPoint.y * texture.height);
        Color pixelColor = texture.GetPixel((int)pixelCoord.x, (int)pixelCoord.y);

        return pixelColor.a > 0.1f;
    }
}