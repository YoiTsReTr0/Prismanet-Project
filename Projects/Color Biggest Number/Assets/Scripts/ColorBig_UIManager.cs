using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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

        [SerializeField] private float AvgAnimTime = 2;

        [Header("Heading Area")] [SerializeField]
        private TextMeshProUGUI ScoreText;

        [SerializeField] private Slider ProgressBar;
        [SerializeField] private GameObject[] ProgressBarStars;

        [SerializeField] private List<GameObject> SelectColorActiveImgList = new();

        [Header("Game Area")] [SerializeField] private Transform QuestionsParent;
        [SerializeField] private ColorBig_NumObj QuestionsObj;

        [SerializeField] private Button CheckAnswerBtn;

        [SerializeField] private GameObject CorrectResultPanel;
        [SerializeField] private GameObject IncorrectResultPanel;


        [Header("Game Over Area")] [SerializeField]
        private GameObject GameOverPanel;

        [SerializeField] private Image[] AchievedStarsImages;
        [SerializeField] private TextMeshProUGUI GameOverResultText;

        [Header("Number Sprites")] [SerializeField]
        private List<Sprite> NumbersList = new();

        [Header("Misc Area")] [SerializeField] private Color AchievedStarColor;

        #endregion

        #region Local Variables

        private ColorBig_GameManager _gameManager;

        private int _currCorrectIndex;
        private int _currSelectedIndex;
        private ColorBig_NumObj _currSelectedNumObj;

        private bool _gameOver;

        private Color _selectionColor = Color.green;

        private List<ColorBig_NumObj> _questionsObjList = new();

        private int _quesTotalCount;
        private int _quesAtteptedCount;

        private Coroutine _progressBarCoroutine;
        private float _averageAnimDurations = 2f;
        private int _currStars = 0;

        #endregion

        #region Unity Events

        [Header("Events")]
        public UnityEvent<int[], int> UIM_OnGameStart = new(); // Called from Game Manager when game starts

        public UnityEvent<int[], int> UIM_SetupNextQuestion = new(); // Called from Game Manager when game starts
        public UnityEvent<int, int> UIM_GameOver = new(); // Called from Game Manager when game starts

        public UnityEvent<int, bool> UIM_UpdateUIForCorrectAnswer = new();
        public UnityEvent<bool> UIM_UpdateUIForIncorrectAnswer = new();

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

        private void SmoothChange(float currentValue, float targetValue, float duration)
        {
            DOVirtual.Float(
                currentValue,
                targetValue,
                duration,
                (value) => { ProgressBar.value = value; }
            ).OnComplete(() => { });
        }

        /// <summary>
        /// Provides obtained stars out of 3 by provided quotient
        /// </summary>
        /// <param name="quotient">Value for stars</param>
        /// <returns></returns>
        private int GetStarsCount(float quotient)
        {
            int stars = 0;

            if (quotient == 1)
                stars = 3;

            else if (quotient >= 0.6)
            {
                stars = 2;
            }

            else if (quotient >= 0.2)
            {
                stars = 1;
            }

            else if (quotient < 0.2)
                stars = 0;

            return stars;
        }

        private void RunGrowAndShrinkAnim(GameObject obj, Color newColor = default, bool useColor = false)
        {
            Vector3 OgSize1 = obj.transform.localScale;

            if (useColor)
                obj.GetComponent<Image>().color = newColor;

            obj.transform.DOScale(OgSize1 + Vector3.one, _averageAnimDurations / 16)
                .OnComplete(
                    () =>
                        obj.transform.DOScale(OgSize1, _averageAnimDurations / 16));
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

            UIM_OnGameStart.AddListener((int[] ques, int count) =>
            {
                _quesTotalCount = count;
                ScoreText.text = $"Score: 1/{_quesTotalCount} ";
                SetupQuestion(ques);
            });
            UIM_SetupNextQuestion.AddListener((ques, attemptedQues) =>
            {
                _questionsObjList.Clear();
                _currSelectedIndex = -1;

                SetupQuestion(ques, attemptedQues + 1);
            });

            UIM_UpdateUIForCorrectAnswer.AddListener((_score, isContinue) =>
            {
                CorrectAnsUpdateUI(_score);
                CorrectResultPanel.SetActive(true);

                DOVirtual.DelayedCall(_averageAnimDurations, () =>
                {
                    CorrectResultPanel.SetActive(false);

                    if (isContinue)
                        _gameManager.GM_OnSetupNextQuestion?.Invoke();

                    else
                        _gameManager.GM_OnGameOver?.Invoke();
                });
            });

            UIM_UpdateUIForIncorrectAnswer.AddListener((isContinue) =>
            {
                IncorrectResultPanel.SetActive(true);

                DOVirtual.DelayedCall(_averageAnimDurations, () =>
                {
                    IncorrectResultPanel.SetActive(false);

                    if (isContinue)
                        _gameManager.GM_OnSetupNextQuestion?.Invoke();

                    else
                        _gameManager.GM_OnGameOver?.Invoke();
                });
            });

            UIM_GameOver.AddListener((int score, int maxScore) =>
            {
                int stars = GetStarsCount((float)score / maxScore);

                GameOverSetup(stars);

                _gameOver = true;

                for (int i = 0; i < stars; i++)
                    AchievedStarsImages[i].color = AchievedStarColor;
            });

            CheckAnswerBtn.onClick.AddListener(CheckAnswer);
        }

        private void SetupQuestion(int[] array, int attemptedQues)
        {
            ScoreText.text = $"Ques: {attemptedQues}/{_quesTotalCount} ";

            SetupQuestion(array);
        }

        private void GameOverSetup(int score)
        {
            GameOverPanel.SetActive(true);
            GameOverResultText.text = score < 1 ? "Try Harder Next Time" : "Well Done";
        }

        private IEnumerator ProgressBarAnimIncrease(float initialVal, float finalVal)
        {
            void ClaimProgressBarStar(int starNo)
            {
                RunGrowAndShrinkAnim(ProgressBarStars[starNo - 1], Color.yellow, true);
                RunGrowAndShrinkAnim(ProgressBar.handleRect.gameObject);
            }


            float elapsedTime = 0f;

            while (elapsedTime < _averageAnimDurations)
            {
                elapsedTime += Time.deltaTime;

                ProgressBar.value = Mathf.Lerp(initialVal, finalVal, elapsedTime / _averageAnimDurations);

                if (_currStars < GetStarsCount(ProgressBar.value))
                {
                    _currStars++;
                    ClaimProgressBarStar(_currStars);
                }

                yield return new WaitForEndOfFrame();
            }


            yield return null;
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

                btnObj.ObjImage.sprite = NumbersList[question[i] - 1];

                int index = i;
                btnObj.ObjButton.onClick.AddListener(() =>
                {
                    //_quesAtteptedCount++;
                    /*if (index == _currCorrectIndex)
                        SetupButtonClick(true, btnObj);

                    else
                        SetupButtonClick(false, btnObj);*/

                    _currSelectedIndex = index;

                    SetupButtonClick(btnObj);
                });
            }

            return;

            void SetupButtonClick(ColorBig_NumObj btnObj)
            {
                btnObj.ObjImage.color = _selectionColor;

                if (_currSelectedNumObj == btnObj)
                    return;

                if (_currSelectedNumObj)
                    _currSelectedNumObj.ObjImage.color = Color.white;

                _currSelectedNumObj = btnObj;

                if (!CheckAnswerBtn.interactable)
                    CheckAnswerBtn.interactable = true;
            }
        }

        private void CheckAnswer()
        {
            if (_currSelectedIndex == _currCorrectIndex)
            {
                _gameManager.GM_OnAnswerCorrect?.Invoke();
            }
            else
            {
                _gameManager.GM_OnAnswerIncorrect?.Invoke();
                _currSelectedNumObj.ObjImage.color = Color.red;

                _questionsObjList[_currCorrectIndex].ObjImage.color = _selectionColor;
            }

            CheckAnswerBtn.interactable = false;
        }

        private void CorrectAnsUpdateUI(int correctAns)
        {
            _progressBarCoroutine =
                StartCoroutine(ProgressBarAnimIncrease(ProgressBar.value, (float)correctAns / _quesTotalCount));
        }

        public void TempSetColor(string hexCode)
        {
            if (ColorUtility.TryParseHtmlString(hexCode, out Color color))
            {
                _selectionColor = color;
            }

            foreach (var t in SelectColorActiveImgList)
                t.SetActive(false);
        }
    }
}