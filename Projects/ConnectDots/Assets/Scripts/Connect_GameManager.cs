using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MainGame.ConnectDots
{
    public class Connect_GameManager : MonoBehaviour
    {
        public static Connect_GameManager instance;
        private Connect_UIManager _uiManager;

        #region Local Variables

        private bool _gameEnd = false;

        private int _currLivesCount;
        private int _totalLivesCount;

        private int _corrAnsCount;
        private int _attemptedAnsCount;

        private Connect_DataSO _currLevelData;
        private Queue<Connect_DataSO> _levelsDataQueue = new();

        #endregion

        #region Editor Variables

        [SerializeField] private List<Connect_DataSO> LevelDataList;
        [SerializeField] private int QuesCount = 10;

        #endregion

        #region Unity Events

        [HideInInspector] public UnityEvent GM_OnGameStart = new();
        [HideInInspector] public UnityEvent GM_SetupNewQuestion = new();

        [HideInInspector] public UnityEvent GM_OnAnswerCorrect = new();
        [HideInInspector] public UnityEvent GM_OnAnswerIncorrect = new();

        [HideInInspector] public UnityEvent GM_OnFullAnswerSetCorrect = new();
        [HideInInspector] public UnityEvent GM_OnFullAnswerSetIncorrect = new();

        [HideInInspector] public UnityEvent GM_OnGameOver = new();

        #endregion

        #region Helper Func

        private void InitLevelData()
        {
            for (int i = 0; i < QuesCount; i++)
            {
                Connect_DataSO newData;

                do
                {
                    newData = LevelDataList[Random.Range(0, LevelDataList.Count)];
                } while (_currLevelData != null && _currLevelData == newData);

                _currLevelData = newData;
                _levelsDataQueue.Enqueue(newData);
            }
        }

        private void UpdateCurrLevelData()
        {
            _currLevelData = _levelsDataQueue.Dequeue();
            _totalLivesCount = _currLivesCount = _currLevelData.LivesCount;
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
            _uiManager = Connect_UIManager.instance;

            GM_OnGameStart.AddListener(() =>
            {
                InitLevelData();

                _uiManager.UIM_OnGameStart?.Invoke(QuesCount);

                GM_SetupNewQuestion?.Invoke();
            });

            GM_SetupNewQuestion.AddListener(() =>
            {
                UpdateCurrLevelData();


                _uiManager.UIM_SetupNextQuestion?.Invoke(_currLevelData, _attemptedAnsCount, QuesCount);
            });


            GM_OnFullAnswerSetCorrect.AddListener(() =>
            {
                _attemptedAnsCount++;
                _corrAnsCount++;

                bool continueGame = _attemptedAnsCount < QuesCount;

                _uiManager.UIM_UpdateUIForCorrectFullAnswerSet?.Invoke(
                    new(_corrAnsCount, _attemptedAnsCount, QuesCount),
                    continueGame);
            });

            GM_OnFullAnswerSetIncorrect.AddListener(() =>
            {
                _attemptedAnsCount++;
                bool continueGame = _attemptedAnsCount < QuesCount;

                _currLivesCount = continueGame ? _totalLivesCount : _currLivesCount;
                _uiManager.UIM_UpdateUIForIncorrectFullAnswerSet?.Invoke(_attemptedAnsCount, QuesCount,
                    continueGame);
            });

            GM_OnAnswerCorrect.AddListener(() => { _uiManager.UIM_UpdateUIForCorrectAnswer?.Invoke(); });

            GM_OnAnswerIncorrect.AddListener(() =>
            {
                _currLivesCount--;
                _uiManager.UIM_UpdateUIForIncorrectAnswer?.Invoke(_currLivesCount);
            });


            GM_OnGameOver.AddListener(() =>
            {
                /*if (QuestionCount == 1)
                    _uiManager.UIM_GameOver?.Invoke(_currLivesCount, _totalLivesCount);

                else*/
                {
                    _uiManager.UIM_GameOver?.Invoke(_corrAnsCount, QuesCount);
                }
            });

            Invoke("tempStartDelay", 1f);
        }

        private void tempStartDelay()
        {
            GM_OnGameStart.Invoke();
        }
    }
}