using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Hexagon_GameManager : MonoBehaviour
{
    public static Hexagon_GameManager instance;

    #region Local Variables

    private Hexagon_UIManager _uiManager;

    private int _currLives;
    private int _corrAnsCount;
    private int _attemptedAnsCount;

    #endregion

    #region Editor Variables

    [SerializeField, Range(1, 5)] private int TotalLives = 3;
    [SerializeField] private int QuestionCount;
    [SerializeField] private int HiddenAnsCount = 6;

    [SerializeField] private int MinRange = 2;
    [SerializeField] private int MaxRange = 20;

    #endregion

    #region Unity Events

    [HideInInspector] public UnityEvent GM_OnGameStart = new();
    [HideInInspector] public UnityEvent GM_SetupNewQuestion = new();
    [HideInInspector] public UnityEvent GM_OnAnswerCorrect = new();
    [HideInInspector] public UnityEvent GM_OnAnswerIncorrect = new();
    [HideInInspector] public UnityEvent GM_OnFullAnswerSetIncorrect = new();
    [HideInInspector] public UnityEvent GM_OnGameOver = new();

    #endregion

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        _uiManager = Hexagon_UIManager.instance;

        GM_OnGameStart.AddListener(() =>
        {
            _uiManager.UIM_OnGameStart?.Invoke(new Vector3(TotalLives, MinRange, MaxRange), HiddenAnsCount,
                QuestionCount, QuestionCount == 1);

            GM_SetupNewQuestion?.Invoke();
        });

        GM_SetupNewQuestion.AddListener(() => { _uiManager.UIM_SetupNextQuestion?.Invoke(_currLives); });


        GM_OnAnswerCorrect.AddListener(() =>
        {
            _attemptedAnsCount++;
            _corrAnsCount++;
            _currLives = TotalLives;

            bool continueGame = _attemptedAnsCount < QuestionCount;

            _uiManager.UIM_UpdateUIForCorrectAnswer?.Invoke(
                new(_corrAnsCount, _attemptedAnsCount, QuestionCount),
                continueGame);
        });

        GM_OnFullAnswerSetIncorrect.AddListener(() =>
        {
            _attemptedAnsCount++;
            bool continueGame = _attemptedAnsCount < QuestionCount;

            _currLives = continueGame ? TotalLives : _currLives;
            _uiManager.UIM_UpdateUIForIncorrectFullAnswerSet?.Invoke(_attemptedAnsCount, QuestionCount, _currLives,
                continueGame);
        });

        GM_OnAnswerIncorrect.AddListener(() =>
        {
            _currLives--;
            _uiManager.UIM_UpdateUIForIncorrectAnswer?.Invoke(_currLives);
        });


        GM_OnGameOver.AddListener(() =>
        {
            if (QuestionCount == 1)
                _uiManager.UIM_GameOver?.Invoke(_currLives, TotalLives);

            else
            {
                _uiManager.UIM_GameOver?.Invoke(_corrAnsCount, QuestionCount);
            }
        });


        GM_OnGameStart?.Invoke();

        _currLives = TotalLives;
    }
}