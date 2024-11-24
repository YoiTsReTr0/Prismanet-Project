using UnityEngine;

public class GroupPanel : MonoBehaviour
{
    public ShapeType acceptedShapeType;

    public bool CanAcceptShape(ShapeType shapeType)
    {
        return shapeType == acceptedShapeType;
    }
}