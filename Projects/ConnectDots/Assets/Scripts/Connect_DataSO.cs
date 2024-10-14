using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewConnectGameData", menuName = "Create Minigame Data/Connect Game")]
public class Connect_DataSO : ScriptableObject
{
    public string GameCriteriaText;
    public List<int> AnswersList;
}