using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using Random = UnityEngine.Random;

namespace QuizGame.LevelPlay
{
    public class QuizGameManager : MonoBehaviour
    {
        public static QuizGameManager instance;

        #region Local Variables

        private QuizUIManager _uiManager;
        private QuizGameAudioManger _audioManger;

        private bool _gameOver;

        private int _userCoins = 16;
        private int _correctAnsRewardCoins = 19;

        [SerializeField] private int _totalQuestions = 5;
        private int _correctAnswers = 0;
        int _attemptedQuestions = 0;

        private Vector3 _currentQuestion = new();
        private int[] _currentChoices = new int[4];

        [SerializeField,
         Tooltip(
             "For this demo, a UI functionality to change range values is provided. In main game, this will be done through difficulty filtering. Exclusive minimal range")]
        private int FormerNumRange = 4;

        [SerializeField,
         Tooltip(
             "For this demo, a UI functionality to change range values is provided. In main game, this will be done through difficulty filtering. Inclusive outer range")]
        private int LatterNumRange = 9;

        #endregion

        #region Events/Actions

        [HideInInspector] public UnityEvent GM_OnGameStart; // Event can be used to restart the game
        [HideInInspector] public UnityEvent GM_OnAnswerCorrect;
        [HideInInspector] public UnityEvent GM_OnAnswerIncorrect;
        [HideInInspector] public UnityEvent GM_OnGameOver;

        #endregion

        #region Helper Functions

        /// <summary>
        /// Generates a new question with answer in Vector3 format and updates the local storage variables
        /// </summary>
        /// <returns></returns>
        private Vector3 GetNewQuestion()
        {
            Vector3 que = new();

            que = new(Random.Range(1, FormerNumRange), Random.Range(1, LatterNumRange), 0);
            que.z = que.x * que.y;

            return que;
        }


        /// <summary>
        /// Returns an arroy of choices in the form of integer. Also sorts them randomly.
        /// </summary>
        /// <param name="corrAns">Correct answer to get values closer to this number</param>
        /// <returns></returns>
        private int[] GetChoices(int corrAns)
        {
            // Setup incorrect choices before random sorting
            int rangeOffset = 5;
            int lowerBound = (corrAns - 5 <= 0) ? 1 : corrAns - 5;

            HashSet<int> choiceSet = new HashSet<int>();

            choiceSet.Add(corrAns);

            while (choiceSet.Count < 4)
            {
                int randomChoice = Random.Range(lowerBound, corrAns + rangeOffset + 3);
                choiceSet.Add(randomChoice);
            }

            int[] choices = new int[4];
            choiceSet.CopyTo(choices);

            // Shuffle the choices array
            for (int i = 0; i < choices.Length; i++)
            {
                int randomIndex = Random.Range(i, choices.Length);
                int temp = choices[i];
                choices[i] = choices[randomIndex];
                choices[randomIndex] = temp;
            }

            return choices;
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
            _audioManger = QuizGameAudioManger.instance;

            // Setup and sub functions to events
            GM_OnGameStart.AddListener(() =>
            {
                SetupNewQuestion(); // Generate 1st question
                _uiManager.UIM_OnGameStart?.Invoke(_correctAnsRewardCoins);
                _uiManager.UIM_SetupNextQuestion?.Invoke(_currentQuestion, _currentChoices);
            });

            GM_OnAnswerCorrect.AddListener(() =>
            {
                _audioManger.PlaySFXOneShotHigh(_audioManger.CorrAnswerClip);

                _correctAnswers++;
                _attemptedQuestions++;
                _userCoins += 19;

                if (_attemptedQuestions < _totalQuestions)
                {
                    SetupNewQuestion();
                    _uiManager.UIM_SetupNextQuestion?.Invoke(_currentQuestion, _currentChoices);
                }

                else if (_attemptedQuestions >= _totalQuestions)
                    GM_OnGameOver?.Invoke();

                _uiManager.UIM_UpdateUIForCorrectAnswer?.Invoke(_correctAnswers, _totalQuestions, _userCoins);
            });

            GM_OnAnswerIncorrect.AddListener(() =>
            {
                _audioManger.PlaySFXOneShotHigh(_audioManger.WrongAnswerClip);

                _attemptedQuestions++;

                if (_attemptedQuestions < _totalQuestions)
                {
                    SetupNewQuestion();
                    _uiManager.UIM_SetupNextQuestion?.Invoke(_currentQuestion, _currentChoices);
                }

                else if (_attemptedQuestions >= _totalQuestions)
                    GM_OnGameOver?.Invoke();
            });

            GM_OnGameOver.AddListener(() =>
            {
                _gameOver = true;
                _audioManger.StopAllAmbient();
                _uiManager.UIM_GameOver?.Invoke(_correctAnswers, _totalQuestions);
            });
        }


        /// <summary>
        /// Parent setup function for new question and choices
        /// </summary>
        private void SetupNewQuestion()
        {
            _currentQuestion = GetNewQuestion();

            _currentChoices = GetChoices((int)_currentQuestion.z);
        }


        /// <summary>
        /// Simple function to load back main menu scene
        /// </summary>
        public void LoadMainMenuScene()
        {
        }


        [ContextMenu("StartGame")]
        private void TempGameStart()
        {
            GM_OnGameStart?.Invoke();
        }
    }
}