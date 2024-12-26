using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace QuizGame.LevelPlay
{
    public class QuizUIManager : MonoBehaviour
    {
        public static QuizUIManager instance;

        #region Local Variables

        private QuizGameManager _gameManager;
        private QuizGameAudioManger _audioManger;

        bool _gameOver;

        private Coroutine _progressBarCoroutine;
        private float _averageAnimDurations = 2f;
        private int _currStars = 0;
        private int _correctAnsRewardCoins = 0;
        private int _quesCount = 0;

        private int _currCorrectAns;
        private int _currChosenChoice;
        private Button _currChosenChoiceButton;

        #endregion

        #region Editor Variables

        [Header("Heading Area")] [SerializeField]
        private TextMeshProUGUI CoinsText;

        [SerializeField] private TextMeshProUGUI HeadingQuesCountText;

        [SerializeField] private GameObject CoinsImage;


        [Header("Questions Area")] [SerializeField]
        private TextMeshProUGUI QuestionText;


        [SerializeField] private RectTransform QuestionsArea;

        [SerializeField] private Slider ProgressBar;
        [SerializeField] private GameObject[] ProgressBarStars;

        [SerializeField] private Button[] ChoicesButtons;

        [SerializeField] private GameObject ChoicesProtectorImage;
        [SerializeField] private GameObject CorrectResultPanel;
        [SerializeField] private GameObject IncorrectResultPanel;
        [SerializeField] private Button CheckAnswerBtn;

        [Header("Game Over Area")] [SerializeField]
        private GameObject GameOverPanel;

        [SerializeField] private TextMeshProUGUI GameOverText;

        [SerializeField] private Image[] AchievedStarsImages;

        [Header("Misc Area")] [SerializeField] private Color AchievedStarColor;
        [SerializeField] private Color ChoiceSelectedColor;
        [SerializeField] private Color ChoiceDefaultColor;

        #endregion

        #region Unity Events

        [Header("Events")]
        public UnityEvent<int, int, int> UIM_OnGameStart = new(); // Called from Game Manager when game starts

        public UnityEvent<Vector2, int[], int>
            UIM_SetupNextQuestion = new(); // Called from Game Manager when game starts

        public UnityEvent<int, int> UIM_GameOver = new(); // Called from Game Manager when game starts

        public UnityEvent<int, int, int, int>
            UIM_UpdateUIForCorrectAnswer = new(); // Called from Game Manager when game starts

        #endregion

        #region Helper Functions

        /// <summary>
        /// Provides obtained stars out of 3 by division of parameters
        /// </summary>
        /// <param name="val1">Numerator</param>
        /// <param name="val2">Denominator</param>
        /// <returns></returns>
        private int GetStarsCount(float val1, int val2)
        {
            int stars = 0;
            float quotient = (float)val1 / val2;

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

        public string GenerateOddSequence(int n)
        {
            if (n <= 0) return string.Empty;

            List<int> oddNumbers = new List<int>();
            for (int i = 0; i < n; i++)
            {
                oddNumbers.Add(2 * i + 1);
            }

            return string.Join(" + ", oddNumbers);
        }

        private void ManageChoiceBtnColorOnClick()
        {
            for (int i = 0; i < ChoicesButtons.Length; i++)
            {
                if (_currChosenChoiceButton != null && ChoicesButtons[i] == _currChosenChoiceButton)
                    _currChosenChoiceButton.GetComponent<Image>().color = ChoiceSelectedColor;

                else
                    ChoicesButtons[i].GetComponent<Image>().color = ChoiceDefaultColor;
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
            _gameManager = QuizGameManager.instance;
            _audioManger = QuizGameAudioManger.instance;

            UIM_OnGameStart.AddListener((int rewardCoins, int attemptedQues, int totalQues) =>
            {
                GameOverPanel.SetActive(false);
                ProgressBar.value = 0;
                _correctAnsRewardCoins = rewardCoins;

                //HeadingQuesCountText.text = $"Ques: 1/{totalQues}";
                _quesCount = totalQues;
            });

            UIM_SetupNextQuestion.AddListener(SetupQuestion);

            UIM_GameOver.AddListener(GameOverSetup);

            UIM_UpdateUIForCorrectAnswer.AddListener(UpdateUIForCorrectAnswer);

            CheckAnswerBtn.onClick.AddListener(HandleAnswerSelection);
        }


        /// <summary>
        /// Setup call to update the question text, choices and portraying images.
        /// </summary>
        /// <param name="ques">Question with answer in the Vector3 format</param>
        /// <param name="choices">Array of choices in the form of integer</param>
        private void SetupQuestion(Vector2 ques, int[] choices, int attemptedQues)
        {
            CheckAnswerBtn.interactable = false;

            _currChosenChoiceButton = null;
            ManageChoiceBtnColorOnClick();

            string quesString = GenerateOddSequence((int)ques.x);
            QuestionText.text = $"<color=#007aff>{quesString} </color>";

            HeadingQuesCountText.text = $"Ques: {attemptedQues + 1}/{_quesCount}";
            _currCorrectAns = (int)ques.y;

            // Setup buttons

            for (int i = 0; i < ChoicesButtons.Length; i++)
            {
                int hChoice = choices[i]; // helper for lambda expression
                Button currBtn = ChoicesButtons[i]; // helper for lambda expression

                ChoicesButtons[i].onClick.RemoveAllListeners();
                ChoicesButtons[i].onClick.AddListener(() =>
                {
                    CheckAnswerBtn.interactable = true;

                    _currChosenChoice = hChoice;
                    _currChosenChoiceButton = currBtn;

                    ManageChoiceBtnColorOnClick();
                });

                ChoicesButtons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = choices[i].ToString();
            }
        }

        private void HandleAnswerSelection()
        {
            if (_currChosenChoice == _currCorrectAns)
            {
                _gameManager.GM_OnAnswerCorrect?.Invoke();

                ChoiceResultPanelAnim(true);

                AddCoinsOnCorrAnsAnim();
            }

            else
            {
                _gameManager.GM_OnAnswerIncorrect?.Invoke();

                ChoiceResultPanelAnim(false);
            }

            if (!_gameOver)
            {
                QuestionsAreaAnim();

                ChoicesAreaAnim();
            }
            else
                QuestionsArea.gameObject.SetActive(false);
        }

        /// <summary>
        /// Update the progress bar with each question answered correctly
        /// </summary>
        /// <param name="correctAns">Correct answer in the integer form simply</param>
        /// <param name="totalQues">Total count of questions for the level</param>
        private void UpdateUIForCorrectAnswer(int correctAns, int attemptedQues, int totalQues, int userCoins)
        {
            if (_progressBarCoroutine != null)
                StopCoroutine(_progressBarCoroutine);

            _progressBarCoroutine =
                StartCoroutine(ProgressBarAnimIncrease(ProgressBar.value, (float)correctAns / totalQues));

            CoinsText.text = userCoins.ToString();
        }


        /// <summary>
        /// Called when all questions are answered, sets up the score
        /// </summary>
        /// <param name="corrAns">Count of correct answers</param>
        /// <param name="totalQues">Total count of questions in the level</param>
        private void GameOverSetup(int correctAns, int totalQues)
        {
            _gameOver = true;
            DOVirtual.DelayedCall(_averageAnimDurations + 1, () =>
            {
                GameOverPanel.SetActive(true);
                _audioManger.PlaySFXOneShotHigh(_audioManger.GameOverClip);
            });

            int stars = GetStarsCount((float)correctAns, totalQues);

            GameOverText.text = stars < 1 ? "Try Harder next Time" : "Well Done";

            for (int i = 0; i < stars; i++)
                AchievedStarsImages[i].color = AchievedStarColor;
        }


        /// <summary>
        /// Coroutine for animated progress bar update
        /// </summary>
        /// <param name="initialVal">Value of slider before update</param>
        /// <param name="finalVal">Updated or final value for the slider</param>
        /// <returns></returns>
        private IEnumerator ProgressBarAnimIncrease(float initialVal, float finalVal)
        {
            void ClaimProgressBarStar(int starNo)
            {
                _audioManger.PlaySFXOneShotHigh(_audioManger.StarCollectClip);

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


        #region UI Animations Area

        private void QuestionsAreaAnim()
        {
            float CurrYPosi = QuestionsArea.anchoredPosition.y;

            QuestionsArea.anchoredPosition = new(QuestionsArea.anchoredPosition.x, Screen.height * 0.14f);
            QuestionsArea.DOAnchorPosY(CurrYPosi, _averageAnimDurations).SetEase(Ease.InOutSine)
                .SetDelay(_averageAnimDurations / 2);

            QuestionsArea.GetComponent<CanvasGroup>().alpha = 0;
            QuestionsArea.GetComponent<CanvasGroup>().DOFade(1, _averageAnimDurations / 4)
                .SetDelay(_averageAnimDurations / 2);
        }

        private void ChoicesAreaAnim()
        {
            RectTransform objRect = ChoicesButtons[0].transform.parent.parent.GetComponent<RectTransform>();
            float CurrYPosi = objRect.anchoredPosition.y;

            ChoicesProtectorImage.SetActive(true);

            objRect.anchoredPosition = new(QuestionsArea.anchoredPosition.x, -Screen.height * 0.7f);
            objRect.DOAnchorPosY(CurrYPosi, _averageAnimDurations).SetEase(Ease.InOutSine)
                .SetDelay(_averageAnimDurations / 2)
                .OnComplete(() => { ChoicesProtectorImage.SetActive(false); });

            DOVirtual.DelayedCall(_averageAnimDurations * 1.1f,
                () => _audioManger.PlaySFXOneShotLow(_audioManger.FishChoicesAppearClip));
        }

        private void ChoiceResultPanelAnim(bool correct)
        {
            GameObject panel = correct ? CorrectResultPanel : IncorrectResultPanel;

            panel.SetActive(true);

            DOVirtual.DelayedCall(_averageAnimDurations, () => { panel.SetActive(false); });


            /*RectTransform objRect = ChoiceResultPanel.GetComponent<RectTransform>();
            float CurrYPosi = objRect.anchoredPosition.y;
            float FinalPosi = -Screen.height * 0.7f;

            objRect.anchoredPosition = new(QuestionsArea.anchoredPosition.x, FinalPosi);

            objRect.DOAnchorPosY(CurrYPosi, _averageAnimDurations / 8).SetEase(Ease.InOutSine);

            objRect.DOAnchorPosY(FinalPosi, _averageAnimDurations / 8)
                .SetEase(Ease.InOutSine)
                .SetDelay(_averageAnimDurations * 0.8f)
                .OnComplete(() =>
                {
                    objRect.anchoredPosition = new Vector2(QuestionsArea.anchoredPosition.x, CurrYPosi);
                    ChoiceResultPanel.SetActive(false);
                });*/
        }

        private void AddCoinsOnCorrAnsAnim()
        {
            DOVirtual.DelayedCall(1,
                () =>
                {
                    RunGrowAndShrinkAnim(CoinsImage);
                    _audioManger.PlaySFXOneShotHigh(_audioManger.CoinRewardClip);
                }); // Add delay as required

            // use _correctAnsRewardCoins for further dramatic UI anim
        }

        #endregion
    }
}