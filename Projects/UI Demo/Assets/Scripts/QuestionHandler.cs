using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "QuestionData", menuName = "ScriptableObjects/QuestionData")]
public class QuestionHandler : ScriptableObject
{
    public List<Questions> questions;
    
}


[Serializable]
public class Questions
{
    public string question;
    public Sprite correctAnswer;
    public List<Sprite> options ;
}