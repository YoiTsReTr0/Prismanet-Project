using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;


namespace ColorBigGame.Main
{
    public class ColorBig_GameManager : MonoBehaviour
    {
        public static ColorBig_GameManager instance;

        #region Editor Variables

        [SerializeField] private int NumbersInQuestion = 4;
        [SerializeField] private int NumOfQuestions = 4;

        #endregion

        #region Local Variables

        private ColorBig_UIManager _UIManager;


        private int _attemptedAnswers;
        private int _correctAnswers;
        private int _score;

        private bool _gameOver;

        #endregion


        #region Unity Events

        [HideInInspector] public UnityEvent GM_OnGameStart;
        [HideInInspector] public UnityEvent GM_OnAnswerCorrect;
        [HideInInspector] public UnityEvent GM_OnSetupNextQuestion;
        [HideInInspector] public UnityEvent GM_OnAnswerIncorrect;
        [HideInInspector] public UnityEvent GM_OnGameOver;

        #endregion

        #region Helper Functions

        /// <summary>
        /// Get a question in the form of integer array in ascending order
        /// </summary>
        /// <param name="low">Lower range</param>
        /// <param name="high">Higher range</param>
        /// <returns>Int[]</returns>
        private int[] GetQuestion(int low, int high)
        {
            if (high - low < NumbersInQuestion)
            {
                throw new InvalidOperationException("Range is too small for the number of unique questions requested.");
            }

            HashSet<int> nums = new();

            int[] result = new int[NumbersInQuestion];

            for (int i = 0; i < NumbersInQuestion; i++)
            {
                int number;

                do number = Random.Range(low, high);
                while (!nums.Add(number));

                result[i] = number;
            }

            Array.Sort(result);
            return result;
        }

        // same can be created for a float

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
            _UIManager = ColorBig_UIManager.instance;

            GM_OnGameStart.AddListener(() =>
            {
                _UIManager.UIM_OnGameStart?.Invoke(GetQuestion(1, 10), NumOfQuestions);
                //_UIManager.UIM_SetupNextQuestion?.Invoke();
            });

            GM_OnAnswerCorrect.AddListener(() =>
            {
                _attemptedAnswers++;
                _correctAnswers++;
                _score++;


                _UIManager.UIM_UpdateUIForCorrectAnswer?.Invoke(_score, _attemptedAnswers < NumOfQuestions);
            });

            GM_OnSetupNextQuestion.AddListener(() =>
            {
                _UIManager.UIM_SetupNextQuestion?.Invoke(GetQuestion(1, 10), _attemptedAnswers);
            });

            GM_OnAnswerIncorrect.AddListener(() =>
            {
                _attemptedAnswers++;


                _UIManager.UIM_UpdateUIForIncorrectAnswer?.Invoke(_attemptedAnswers < NumOfQuestions);
            });


            GM_OnGameOver.AddListener(() =>
            {
                _gameOver = true;
                _UIManager.UIM_GameOver?.Invoke(_score, NumOfQuestions); // modify accordingly
            });

            GM_OnGameStart?.Invoke();
        }
    }
}