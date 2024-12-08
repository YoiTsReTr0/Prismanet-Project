using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Hexagon_GameManager : MonoBehaviour
{
    public static Hexagon_GameManager instance;

    #region Local Variables

    private Hexagon_UIManager _uiManager;

    private int _currLives;

    #endregion

    #region Editor Variables

    [SerializeField, Range(1, 5)] private int TotalLives = 3;

    [SerializeField] private int MinRange = 2;
    [SerializeField] private int MaxRange = 20;

    #endregion

    #region Unity Events

    [HideInInspector] public UnityEvent GM_OnGameStart = new();
    [HideInInspector] public UnityEvent GM_OnAnswerCorrect = new();
    [HideInInspector] public UnityEvent GM_OnAnswerIncorrect = new();
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

        GM_OnGameStart.AddListener(() => { _uiManager.UIM_OnGameStart?.Invoke(TotalLives, MinRange, MaxRange); });

        GM_OnAnswerCorrect.AddListener(() => { _uiManager.UIM_UpdateUIForCorrectAnswer?.Invoke(); });

        GM_OnAnswerIncorrect.AddListener(() =>
        {
            _currLives--;
            _uiManager.UIM_UpdateUIForIncorrectAnswer?.Invoke(_currLives);
        });


        GM_OnGameOver.AddListener(() => _uiManager.UIM_GameOver?.Invoke(_currLives, TotalLives));

        GM_OnGameStart?.Invoke();

        _currLives = TotalLives;
    }
}