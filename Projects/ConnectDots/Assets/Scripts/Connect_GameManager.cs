using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MainGame.ConnectDots
{
    public class Connect_GameManager : MonoBehaviour
    {
        public static Connect_GameManager instance;
        private Connect_UIManager _uiManager;

        #region Local Variables

        private bool _gameEnd = false;
        private int _answersCount;

        private int _currLivesCount;
        [SerializeField] private int _totalLivesCount;

        #endregion

        #region Editor Variables

        [SerializeField] private Connect_DataSO LevelData;

        #endregion

        #region Unity Events

        [HideInInspector] public UnityEvent GM_OnGameStart = new();
        [HideInInspector] public UnityEvent GM_OnCorrectConnect = new();
        [HideInInspector] public UnityEvent GM_OnIncorrectConnect = new();
        [HideInInspector] public UnityEvent<bool> GM_OnGameEnd = new();

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
            _uiManager = Connect_UIManager.instance;

            _currLivesCount = _totalLivesCount = LevelData.LivesCount;


            GM_OnGameStart.AddListener(() =>
            {
                _uiManager.UIM_OnGameStart?.Invoke(LevelData.GameCriteriaText, LevelData.AnswersList,
                    _totalLivesCount);
            });

            GM_OnCorrectConnect.AddListener(() =>
            {
                _answersCount++;
                _uiManager.UIM_OnCorrectConnect?.Invoke(_answersCount, LevelData.AnswersList.Count);
            });

            GM_OnIncorrectConnect.AddListener(() =>
            {
                _currLivesCount--;
                _uiManager.UIM_OnWrongSelection?.Invoke(_currLivesCount);
                if (_currLivesCount <= 0)
                    GM_OnGameEnd?.Invoke(false);
            });

            GM_OnGameEnd.AddListener((bool win) =>
            {
                _gameEnd = true;
                _uiManager.UIM_OnGameEnd?.Invoke(_answersCount, LevelData.AnswersList.Count, win);
                Debug.Log("Game End");
            });

            Invoke("tempStartDelay", 1);
        }

        private void tempStartDelay()
        {
            GM_OnGameStart.Invoke();
        }
    }
}