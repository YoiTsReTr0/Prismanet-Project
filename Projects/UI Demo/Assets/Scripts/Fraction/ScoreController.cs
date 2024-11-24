using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreController : MonoBehaviour
{

    [SerializeField] private FractionManager manager;

    [SerializeField] private TextMeshProUGUI skillText;
    [SerializeField] private TextMeshProUGUI correctText;
    [SerializeField] private TextMeshProUGUI wrongText;


    private void Start()
    {

        CheckForReviewText();
        SetAnswersText();
    }

    private void SetAnswersText()
    {
        correctText.text = manager.correctAnswers.ToString();
        wrongText.text = (manager.questionHandler.questions.Count - manager.correctAnswers).ToString();
    }

    private void CheckForReviewText()
    {
        if(manager.correctAnswers <= manager.questionHandler.questions.Count/2)
        {
            skillText.text = "Keep Learning";
        }
        else if((manager.correctAnswers >= manager.questionHandler.questions.Count/2 ) &&(manager.correctAnswers <= manager.questionHandler.questions.Count - 3))
        {
            skillText.text = "Good Job";
        }
        else
        {
            skillText.text = "Excellent";
        }
    }

    public void TryAgain()
    {
        SceneManager.LoadScene("GameplayFraction");
    }
}
