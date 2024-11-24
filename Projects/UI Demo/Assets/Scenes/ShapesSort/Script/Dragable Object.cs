using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableShape : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ShapeType shapeType;

    private RectTransform rectTransform;
    private Canvas canvas;
    private GroupPanel currentGroupPanel;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetAsLastSibling();
        currentGroupPanel = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CheckGroupPanel();
    }

    private void CheckGroupPanel()
    {
        foreach (var groupPanel in FindObjectsOfType<GroupPanel>())
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(groupPanel.GetComponent<RectTransform>(), rectTransform.position))
            {
                currentGroupPanel = groupPanel;
                return;
            }
        }
        currentGroupPanel = null;
    }

    public bool IsInCorrectGroup()
    {
        return currentGroupPanel != null && currentGroupPanel.CanAcceptShape(shapeType);
    }
}

public enum ShapeType
{
    Square,
    Circle
}