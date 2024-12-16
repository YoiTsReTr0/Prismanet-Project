using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Map_StateObject : MonoBehaviour
{
    private bool isSelected;


    public string StateName;
    public GameObject FlagImage;
    public TextMeshProUGUI FlagText;

    public bool IsSelected
    {
        get => isSelected;

        set
        {
            isSelected = value;
            FlagImage.GetComponent<Animator>().SetBool("isSelected", value);
        }
    }

    void Start()
    {
        Image image = GetComponent<Image>();
        if (image != null)
        {
            image.alphaHitTestMinimumThreshold = 0.1f; // Set alpha threshold
        }

        FlagText = FlagImage.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
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