using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MainGame.TurnsAndPatterns
{
    public class TnP_GameManager : MonoBehaviour
    {
        public static TnP_GameManager instance;

        #region Local Variables

        private TnP_UIManager _uiManager;

        private int _currLivesCount;
        private int _totalLivesCount;

        #endregion

        #region Editor Variables

        [SerializeField] private PatternDataSO GameData;

        #endregion

        #region Unity Events

        [HideInInspector] public UnityEvent GM_OnGameStart = new();
        [HideInInspector] public UnityEvent GM_OnAnswerCorrect = new();
        [HideInInspector] public UnityEvent GM_OnAnswerIncorrect = new();
        [HideInInspector] public UnityEvent<bool> GM_OnGameOver = new();

        #endregion

        #region Pre-Requisites

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
            _uiManager = TnP_UIManager.instance;

            GM_OnGameStart.AddListener(() =>
            {
                _uiManager.UIM_OnGameStart?.Invoke(GameData);
                _currLivesCount = _totalLivesCount = GameData.LivesCount;
            });

            GM_OnAnswerCorrect.AddListener(() => { _uiManager.UIM_UpdateUIForCorrectAnswer?.Invoke(); });

            GM_OnAnswerIncorrect.AddListener(() =>
            {
                _currLivesCount--;
                _uiManager.UIM_UpdateUIForIncorrectAnswer?.Invoke(_currLivesCount);

                if (_currLivesCount <= 0)
                    GM_OnGameOver?.Invoke(false);
            });
            GM_OnGameOver.AddListener((bool win) =>
            {
                _uiManager.UIM_GameOver?.Invoke(win, _currLivesCount, _totalLivesCount);
            });

            GM_OnGameStart?.Invoke();
        }
    }
}