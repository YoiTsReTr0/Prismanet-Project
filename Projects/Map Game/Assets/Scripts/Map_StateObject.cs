using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Map_StateObject : MonoBehaviour
{
    private bool isSelected;


    public string StateName;
    public Image SelectedImage;

    public bool IsSelected
    {
        get => isSelected;

        set
        {
            isSelected = value;
            SelectedImage.GetComponent<Animator>().SetBool("isSelected", value);
        }
    }

    void Start()
    {
        Image image = GetComponent<Image>();
        if (image != null)
        {
            image.alphaHitTestMinimumThreshold = 0.1f; // Set alpha threshold
        }
    }
}

[Serializable]
public class QuestionStateObjectSet
{
    public List<Map_StateObject> AnswerStatesList = new();
    public List<Map_ExpandableStateSet> ExpandableStatesSetList = new();
}

[Serializable]
public class Map_ExpandableStateSet
{
    public List<Map_StateObject> Set = new();
}