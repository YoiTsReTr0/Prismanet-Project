using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MainGame.TurnsAndPatterns
{
    public class TnP_GameManager : MonoBehaviour
    {
        public static TnP_GameManager instance;

        #region Local Variables

        private TnP_UIManager _uiManager;

        private int _currLivesCount;
        private int _totalLivesCount;

        private int _corrAnsCount;
        private int _attemptedAnsCount;

        private PatternDataSO _currGameData;

        #endregion

        #region Editor Variables

        [SerializeField] private List<PatternDataSO> GameDataList;
        [SerializeField] private int QuesCount;

        #endregion

        #region Unity Events

        [HideInInspector] public UnityEvent GM_OnGameStart = new();
        [HideInInspector] public UnityEvent GM_SetupNewQuestion = new();
        [HideInInspector] public UnityEvent GM_OnAnswerCorrect = new();
        [HideInInspector] public UnityEvent GM_OnAnswerIncorrect = new();
        [HideInInspector] public UnityEvent GM_OnFullAnswerSetIncorrect = new();
        [HideInInspector] public UnityEvent GM_OnGameOver = new();

        #endregion

        #region Pre-Requisites

        private void GetNewGameData()
        {
            while (true)
            {
                int x = Random.Range(0, GameDataList.Count);
                if (_currGameData != GameDataList[x]) 
                {
                    _currGameData = GameDataList[x];
                    break;
                }
            }
        }

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

            GM_OnGameStart.AddListener(() => { _uiManager.UIM_OnGameStart?.Invoke(); });

            GM_SetupNewQuestion.AddListener(() =>
            {
                GetNewGameData();

                _totalLivesCount = _currGameData.LivesCount;
                _currLivesCount = _totalLivesCount;
                _uiManager.UIM_SetupNextQuestion?.Invoke(new(_attemptedAnsCount, QuesCount),
                    _currGameData);
            });


            GM_OnAnswerCorrect.AddListener(() =>
            {
                _corrAnsCount++;
                _attemptedAnsCount++;
                _currLivesCount = _totalLivesCount;

                bool continueGame = _attemptedAnsCount < QuesCount;

                _uiManager.UIM_UpdateUIForCorrectAnswer?.Invoke(
                    new(_corrAnsCount, _attemptedAnsCount, QuesCount), _currLivesCount,
                    continueGame);
            });

            GM_OnAnswerIncorrect.AddListener(() =>
            {
                _currLivesCount--;
                _uiManager.UIM_UpdateUIForIncorrectAnswer?.Invoke(_currLivesCount);
            });

            GM_OnFullAnswerSetIncorrect.AddListener(() =>
            {
                _attemptedAnsCount++;
                bool continueGame = _attemptedAnsCount < QuesCount;

                _currLivesCount = continueGame ? _totalLivesCount : _currLivesCount;
                _uiManager.UIM_UpdateUIForIncorrectFullAnswerSet?.Invoke(_attemptedAnsCount, QuesCount, _currLivesCount,
                    continueGame);
            });

            GM_OnGameOver.AddListener(() =>
            {
                if (QuesCount == 1)
                    _uiManager.UIM_GameOver?.Invoke(_currLivesCount, _totalLivesCount);

                else
                {
                    _uiManager.UIM_GameOver?.Invoke(_corrAnsCount, QuesCount);
                }
            });

            GM_OnGameStart?.Invoke();
        }
    }
}