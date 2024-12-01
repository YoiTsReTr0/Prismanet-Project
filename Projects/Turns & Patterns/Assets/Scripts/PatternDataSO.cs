using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Pattern Game Data", menuName = "Create Pattern Data")]
public class PatternDataSO : ScriptableObject
{
    [Range(1, 5)] public int LivesCount = 3;
    public Sprite RotateableSprite;

    [Header("Question Constraints")] [Tooltip("Total count of boxes including fixed and rotatable boxes")]
    public int TotalBoxesCount;

    [Tooltip("Question - where will the fixed images will be placed. Must ALWAYS be less than 'TotalBoxesCount'")]
    public List<int> PositionsList;

    /*[Tooltip("If false then rotation criteria will be 45 degree sets")]
    public bool Is90DegreeGame;*/
}