using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace ColorBigGame.Main
{
    public class ColorBig_UIManager : MonoBehaviour
    {
        public static ColorBig_UIManager instance;

        #region Editor Variables

        [Header("Heading Area")] [SerializeField]
        private TextMeshProUGUI ScoreText;

        [Header("Game Area")] [SerializeField] private Transform QuestionsParent;
        [SerializeField] private ColorBig_NumObj QuestionsObj;

        [Header("Game Over Area")] [SerializeField]
        private GameObject GameOverPanel;

        [SerializeField] private TextMeshProUGUI GameOverResultText;

        #endregion

        #region Local Variables

        private ColorBig_GameManager _gameManager;

        private int _currCorrectIndex;

        private bool _gameOver;

        private Color _selectionColor = Color.green;

        private List<ColorBig_NumObj> _questionsObjList = new();

        #endregion

        #region Unity Events

        [Header("Events")]
        public UnityEvent<int[]> UIM_OnGameStart = new(); // Called from Game Manager when game starts

        public UnityEvent<int[]> UIM_SetupNextQuestion = new(); // Called from Game Manager when game starts
        public UnityEvent<int, int> UIM_GameOver = new(); // Called from Game Manager when game starts

        public UnityEvent<int> UIM_UpdateUIForCorrectAnswer = new();

        #endregion

        #region Helper Functions

        private void ShuffleArray(int[] array)
        {
            // Fisher-Yates shuffle algorithm
            for (int i = array.Length - 1; i > 0; i--)
            {
                int rand = Random.Range(0, i + 1);
                int temp = array[i];
                array[i] = array[rand];
                array[rand] = temp;
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
            _gameManager = ColorBig_GameManager.instance;

            UIM_OnGameStart.AddListener(ques => SetupQuestion(ques));
            UIM_SetupNextQuestion.AddListener(ques => StartCoroutine(DelayedSetupQuestion(ques)));

            UIM_UpdateUIForCorrectAnswer.AddListener(_score => CorrectAnsUpdateUI(_score));

            UIM_GameOver.AddListener((int score, int maxScore) =>
            {
                StartCoroutine(DelayedGameOver(score, maxScore));
                _gameOver = true;
            });
        }

        private IEnumerator DelayedSetupQuestion(int[] array)
        {
            yield return new WaitForSeconds(3.5f);

            SetupQuestion(array);
        }

        private IEnumerator DelayedGameOver(int score, int maxScore)
        {
            yield return new WaitForSeconds(3.5f);

            InitGameOver(score, maxScore);
        }

        private void SetupQuestion(int[] question)
        {
            for (int i = 0; i < QuestionsParent.childCount; i++)
            {
                Destroy(QuestionsParent.GetChild(i).gameObject);
            }

            int ans = question[^1];

            ShuffleArray(question);

            _currCorrectIndex = Array.IndexOf(question, ans);


            for (int i = 0; i < question.Length; i++)
            {
                ColorBig_NumObj btnObj = Instantiate(QuestionsObj, QuestionsParent, false);

                _questionsObjList.Add(btnObj);

                btnObj.ObjText.text = question[i].ToString();

                int index = i;
                btnObj.ObjButton.onClick.AddListener(() =>
                {
                    if (index == _currCorrectIndex)
                        SetupButtonClick(true, btnObj);

                    else
                        SetupButtonClick(false, btnObj);
                });
            }

            return;

            void SetupButtonClick(bool correct, ColorBig_NumObj btnObj)
            {
                foreach (ColorBig_NumObj obj in _questionsObjList)
                    obj.ObjButton.interactable = false;

                if (correct)
                {
                    _gameManager.GM_OnAnswerCorrect?.Invoke();
                    btnObj.ObjText.color = _selectionColor;
                }
                else
                {
                    _gameManager.GM_OnAnswerIncorrect?.Invoke();
                    btnObj.ObjText.color = Color.red;

                    _questionsObjList[_currCorrectIndex].ObjText.color = _selectionColor;
                }

                _questionsObjList.Clear();

                if (!_gameOver) // Debug
                {
                    // Anims area 
                }
            }
        }

        private void CorrectAnsUpdateUI(int score)
        {
            ScoreText.text = score.ToString();
        }

        private void InitGameOver(int score, int maxScore)
        {
            GameOverPanel.SetActive(true);
            GameOverResultText.text = $"{score}/{maxScore}";
        }
    }
}