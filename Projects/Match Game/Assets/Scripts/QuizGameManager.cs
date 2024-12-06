using UnityEngine.Events;
using UnityEngine;

namespace MainGame.MatchingGame
{
    public class QuizGameManager : MonoBehaviour
    {
        public static QuizGameManager instance;

        #region Editor Variables

        [SerializeField] private MatchingGameDataSO LevelData;

        #endregion

        #region Local Variables

        private QuizUIManager _uiManager;

        private bool _gameOver;

        private int _userCoins = 16;

        [SerializeField] private int _totalMatches = 5;
        private int _correctAnswers = 0;

        private int _currLivesCount;
        [SerializeField] private int _totalLivesCount;

        #endregion

        #region Events/Actions

        [HideInInspector] public UnityEvent GM_OnGameStart; // Event can be used to restart the game
        [HideInInspector] public UnityEvent GM_OnOptionCorrect;
        [HideInInspector] public UnityEvent GM_OnOptionIncorrect;
        [HideInInspector] public UnityEvent<bool> GM_OnGameOver;

        #endregion

        #region Helper Functions

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

            _totalMatches = LevelData.LeftOptionImages.Count;
            _currLivesCount = _totalLivesCount = LevelData.LivesCount;

            // Setup and sub functions to events
            GM_OnGameStart.AddListener(() =>
            {
                _uiManager.UIM_OnGameStart?.Invoke(LevelData.LeftOptionImages,
                    LevelData.LeftOptionImages, _totalLivesCount);
                //_uiManager.UIM_SetupNextQuestion?.Invoke();
            });

            GM_OnOptionCorrect.AddListener(() =>
            {
                _correctAnswers++;
                _userCoins += LevelData.RewardCoinsPerAnswerCount;

                if (_correctAnswers < _totalMatches)
                {
                    //_uiManager.UIM_SetupNextQuestion?.Invoke();
                }

                else if (_correctAnswers >= _totalMatches)
                    GM_OnGameOver?.Invoke(true);

                _uiManager.UIM_UpdateUIForCorrectAnswer?.Invoke(_correctAnswers, _totalMatches);
            });

            GM_OnOptionIncorrect.AddListener(() =>
            {
                _currLivesCount--;
                _uiManager.UIM_OnWrongSelection?.Invoke(_currLivesCount);

                if (_currLivesCount <= 0)
                    GM_OnGameOver?.Invoke(false);
            });

            GM_OnGameOver.AddListener((bool win) =>
            {
                _gameOver = true;
                _uiManager.UIM_GameOver?.Invoke(_correctAnswers, _totalMatches, win);

                Debug.Log("Game Over");
            });

            GM_OnGameStart?.Invoke();
        }

        #region Debug Area

        #endregion
    }
}