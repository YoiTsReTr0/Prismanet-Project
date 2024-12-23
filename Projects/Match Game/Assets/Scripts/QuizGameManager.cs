using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

namespace MainGame.MatchingGame
{
    public class QuizGameManager : MonoBehaviour
    {
        public static QuizGameManager instance;

        #region Editor Variables

        [SerializeField] private List<MatchingGameDataSO> LevelDataList;

        [SerializeField] private int QuestionCount = 10;

        #endregion

        #region Local Variables

        private QuizUIManager _uiManager;

        private bool _gameOver;

        private int _userCoins = 16;

        private int _totalMatches = 5;
        private int _corrMatches = 0;

        private int _currLivesCount;
        private int _totalLivesCount;

        private int _corrAnsCount;
        private int _attemptedAnsCount;

        private MatchingGameDataSO _currGameData;

        #endregion

        #region Events/Actions

        [HideInInspector] public UnityEvent GM_OnGameStart = new();
        [HideInInspector] public UnityEvent GM_SetupNewQuestion = new();

        [HideInInspector] public UnityEvent GM_OnAnswerCorrect = new();
        [HideInInspector] public UnityEvent GM_OnAnswerIncorrect = new();

        [HideInInspector] public UnityEvent GM_OnFullAnswerSetCorrect = new();
        [HideInInspector] public UnityEvent GM_OnFullAnswerSetIncorrect = new();

        [HideInInspector] public UnityEvent GM_OnGameOver = new();

        #endregion

        #region Helper Functions

        private void UpdateGameData()
        {
            MatchingGameDataSO newData = LevelDataList[Random.Range(0, LevelDataList.Count)];

            while (_currGameData == newData)
            {
                newData = LevelDataList[Random.Range(0, LevelDataList.Count)];
            }

            _currGameData = newData;
            _totalMatches = _currGameData.LeftOptionImages.Count;
            _totalLivesCount = _currLivesCount = _currGameData.LivesCount;
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
            _uiManager = QuizUIManager.instance;
            GM_OnGameStart.AddListener(() =>
            {
                _uiManager.UIM_OnGameStart?.Invoke(QuestionCount);

                GM_SetupNewQuestion?.Invoke();
            });

            GM_SetupNewQuestion.AddListener(() =>
            {
                UpdateGameData();

                _corrMatches = 0;

                _uiManager.UIM_SetupNextQuestion?.Invoke(_currGameData, _attemptedAnsCount, QuestionCount);
            });


            GM_OnFullAnswerSetCorrect.AddListener(() =>
            {
                _attemptedAnsCount++;
                _corrAnsCount++;

                bool continueGame = _attemptedAnsCount < QuestionCount;

                _uiManager.UIM_UpdateUIForCorrectFullAnswerSet?.Invoke(
                    new(_corrAnsCount, _attemptedAnsCount, QuestionCount),
                    continueGame);
            });

            GM_OnFullAnswerSetIncorrect.AddListener(() =>
            {
                _attemptedAnsCount++;
                bool continueGame = _attemptedAnsCount < QuestionCount;

                _currLivesCount = continueGame ? _totalLivesCount : _currLivesCount;
                _uiManager.UIM_UpdateUIForIncorrectFullAnswerSet?.Invoke(_attemptedAnsCount, QuestionCount,
                    continueGame);
            });

            GM_OnAnswerCorrect.AddListener(() =>
            {
                _corrMatches++;

                _uiManager.UIM_UpdateUIForCorrectAnswer?.Invoke(_corrMatches == _totalMatches);
            });

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
                    _uiManager.UIM_GameOver?.Invoke(_corrAnsCount, QuestionCount);
                }
            });

            GM_OnGameStart?.Invoke();
        }

        #region Debug Area

        #endregion
    }
}