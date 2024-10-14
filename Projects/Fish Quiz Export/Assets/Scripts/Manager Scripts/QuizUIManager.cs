using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
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
        private float _averageAnimDurations = 2.5f;
        private int _currStars = 0;
        private int _correctAnsRewardCoins = 0;

        #endregion

        #region Editor Variables

        [Header("Heading Area")] [SerializeField]
        private TextMeshProUGUI CoinsText;

        [SerializeField] private GameObject CoinsImage;


        [Header("Questions Area")] [SerializeField]
        private TextMeshProUGUI QuestionText;

        [SerializeField] private Image QuesImagePrefab;
        [SerializeField] private Sprite[] QuesImagesList;

        [SerializeField,
         Tooltip(
             "This demo version supports only constant images size, with full GD the size manipulation can be added")]
        private GridLayoutGroup QuestionImagesParent;

        [SerializeField] private RectTransform QuestionsArea;

        [SerializeField] private Slider ProgressBar;
        [SerializeField] private GameObject[] ProgressBarStars;

        [SerializeField] private Button[] ChoicesButtons;
        [SerializeField] private GameObject ChoicesProtectorImage;
        [SerializeField] private GameObject ChoiceResultPanel;
        [SerializeField] private TextMeshProUGUI ChoiceResultText;

        [Header("Game Over Area")] [SerializeField]
        private GameObject GameOverPanel;

        [SerializeField] private Image[] AchievedStarsImages;

        [Header("Misc Area")] [SerializeField] private Color AchievedStarColor;

        #endregion

        #region Unity Events

        [Header("Events")] public UnityEvent<int> UIM_OnGameStart = new(); // Called from Game Manager when game starts

        public UnityEvent<Vector3, int[]> UIM_SetupNextQuestion = new(); // Called from Game Manager when game starts
        public UnityEvent<int, int> UIM_GameOver = new(); // Called from Game Manager when game starts

        public UnityEvent<int, int, int>
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

            UIM_OnGameStart.AddListener((int rewardCoins) =>
            {
                GameOverPanel.SetActive(false);
                ProgressBar.value = 0;
                _correctAnsRewardCoins = rewardCoins;
            });

            UIM_SetupNextQuestion.AddListener(SetupQuestion);

            UIM_GameOver.AddListener(GameOverSetup);

            UIM_UpdateUIForCorrectAnswer.AddListener(UpdateUIForCorrectAnswer);
        }


        /// <summary>
        /// Setup call to update the question text, choices and portraying images.
        /// </summary>
        /// <param name="ques">Question with answer in the Vector3 format</param>
        /// <param name="choices">Array of choices in the form of integer</param>
        private void SetupQuestion(Vector3 ques, int[] choices)
        {
            // Write the question
            QuestionText.text = $"{ques.x}  <font=\"LiberationSans SDF\"><b>x</b></font>  {ques.y}";

            // Setup buttons

            for (int i = 0; i < ChoicesButtons.Length; i++)
            {
                int hChoice = choices[i]; // helper for lambda expression
                int hAns = (int)ques.z; // helper for lambda expression

                ChoicesButtons[i].onClick.RemoveAllListeners();
                ChoicesButtons[i].onClick.AddListener(() =>
                {
                    if (hChoice == hAns)
                    {
                        _gameManager.GM_OnAnswerCorrect?.Invoke();

                        ChoiceResultText.text = "Perfect";

                        AddCoinsOnCorrAnsAnim();
                    }

                    else
                    {
                        _gameManager.GM_OnAnswerIncorrect?.Invoke();

                        ChoiceResultText.text = "Correct answer was " + hAns.ToString();
                    }

                    if (!_gameOver)
                    {
                        QuestionsAreaAnim();

                        ChoicesAreaAnim();
                    }
                    else
                        QuestionsArea.gameObject.SetActive(false);

                    ChoiceResultPanelAnim();
                });

                ChoicesButtons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = choices[i].ToString();
            }


            // Destroy old images
            for (int i = 0; i < QuestionImagesParent.transform.childCount; i++)
            {
                Destroy(QuestionImagesParent.transform.GetChild(i).gameObject);
            }

            QuestionImagesParent.constraintCount = (int)ques.x;

            // Setup new images
            int spriteNum = Random.Range(0, QuesImagesList.Length);
            for (int i = 0; i < ques.x * ques.y; i++)
            {
                Image img = Instantiate(QuesImagePrefab, QuestionImagesParent.transform, false);
                img.sprite = QuesImagesList[spriteNum];
            }
        }


        /// <summary>
        /// Update the progress bar with each question answered correctly
        /// </summary>
        /// <param name="correctAns">Correct answer in the integer form simply</param>
        /// <param name="totalQues">Total count of questions for the level</param>
        private void UpdateUIForCorrectAnswer(int correctAns, int totalQues, int userCoins)
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
        private void GameOverSetup(int corrAns, int totalQues)
        {
            _gameOver = true;
            DOVirtual.DelayedCall(_averageAnimDurations + 1, () =>
            {
                GameOverPanel.SetActive(true);
                _audioManger.PlaySFXOneShotHigh(_audioManger.GameOverClip);
            });

            for (int i = 0; i < GetStarsCount((float)corrAns, totalQues); i++)
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

        private void ChoiceResultPanelAnim()
        {
            ChoiceResultPanel.SetActive(true);

            RectTransform objRect = ChoiceResultPanel.GetComponent<RectTransform>();
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
                });
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