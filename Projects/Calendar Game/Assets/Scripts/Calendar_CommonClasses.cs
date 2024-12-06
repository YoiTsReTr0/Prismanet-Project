using System;
using TMPro;
using UnityEngine;

[Serializable]
public class Calendar_QuesData
{
    public int SmallestNum;
    public Transform ContainerTransform;
}

[Serializable]
public class Calendar_QuesUI
{
    public TextMeshProUGUI HeadingText;
    public TMP_InputField InputField;
}

public enum CurrentQuesState
{
    FindStage,
    AddStage,
    MultiplyStage
}