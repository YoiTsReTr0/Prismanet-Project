using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MainGame.ConnectDots
{
    public class Connect_GameManager : MonoBehaviour
    {
        public static Connect_GameManager instance;
        private Connect_UIManager _uiManager;

        #region Local Variables

        private bool _gameEnd = false;
        private int _answersCount;

        #endregion

        #region Editor Variables

        [SerializeField] private Connect_DataSO GameData;

        #endregion

        #region Unity Events

        [HideInInspector] public UnityEvent GM_OnGameStart = new();
        [HideInInspector] public UnityEvent GM_OnCorrectConnect = new();
        [HideInInspector] public UnityEvent GM_OnIncorrectConnect = new();
        [HideInInspector] public UnityEvent GM_OnGameEnd = new();

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
                _uiManager.UIM_OnGameStart?.Invoke(GameData.GameCriteriaText, GameData.AnswersList);
            });

            GM_OnCorrectConnect.AddListener(() =>
            {
                _uiManager.UIM_OnCorrectConnect?.Invoke();
                _answersCount++;

                /* updated to end when the animation reaches the last answer. Pending code addition, add where required
                 if (_answersCount == GameData.AnswersList.Count)
                {
                    GM_OnGameEnd?.Invoke();
                }*/
            });

            GM_OnGameEnd.AddListener(() =>
            {
                _gameEnd = true;
                _uiManager.UIM_OnGameEnd?.Invoke();
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