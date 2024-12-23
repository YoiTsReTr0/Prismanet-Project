using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MainGame.MatchingGame
{
    public class QuizUIManager : MonoBehaviour
    {
        public static QuizUIManager instance;

        #region Local Variables

        private QuizGameManager _gameManager;

        private bool _gameOver;
        private bool _optionSelected;
        private int _currSelectionNo;
        private int _correctAnsRewardCoins = 0;

        private List<OptionsObject> _leftOptionButtonList = new();
        private List<OptionsObject> _rightOptionButtonList = new();
        private Button _currSelectedOptionBtn;

        private Coroutine _progressBarCoroutine;
        private float _averageAnimDurations = 2f;
        private int _currStars = 0;

        #endregion

        #region Editor Variables

        [Header("Heading Area")] [SerializeField]
        private TextMeshProUGUI QuesCountText;

        [SerializeField] private Image[] LivesImages;

        [Header("Matching Area")] [SerializeField]
        private Transform LeftOptionsParent;

        [SerializeField] private Transform RightOptionsParent;
        [SerializeField] private OptionsObject MatchOptionObj;

        [SerializeField] private Slider ProgressBar;
        [SerializeField] private GameObject[] ProgressBarStars;

        [SerializeField] private GameObject GameProtectorScreen;

        [Space(20), SerializeField] private GameObject AnswerSetResultPanel;
        [SerializeField] private TextMeshProUGUI AnswerSetResultPanelText;

        [Header("Game Over Area")] [SerializeField]
        private GameObject GameOverPanel;

        [SerializeField] private TextMeshProUGUI GameOverText;
        [SerializeField] private Image[] AchievedStarsImages;

        [Header("Misc Area")] [SerializeField, Tooltip("Minimum dimensions of match object, ref 4 objects")]
        private Vector2 MinMatchObjSize;

        [SerializeField, Tooltip("Maximum dimensions of match object, ref 10 objects")]
        private Vector2 MaxMatchObjSize;

        [SerializeField] private Color AchievedStarColor;

        #endregion

        #region Unity Events

        [Header("Events")] public UnityEvent<int> UIM_OnGameStart = new();
        public UnityEvent<MatchingGameDataSO, int, int> UIM_SetupNextQuestion = new();

        public UnityEvent<bool> UIM_UpdateUIForCorrectAnswer = new();
        public UnityEvent<int> UIM_UpdateUIForIncorrectAnswer = new();
        public UnityEvent<Vector3, bool> UIM_UpdateUIForCorrectFullAnswerSet = new();
        public UnityEvent<int, int, bool> UIM_UpdateUIForIncorrectFullAnswerSet = new();
        public UnityEvent<int, int> UIM_GameOver = new();
        public UnityEvent UIM_OutsideClick = new();

        //public UnityEvent UIM_SetupNextQuestion = new(); // Called from Game Manager when game starts

        #endregion

        #region Helper Functions (depricated as of now. future usable)

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


            UIM_OnGameStart.AddListener(
                (int quesCount) => { QuesCountText.text = $"Ques: 0/{quesCount}"; });

            UIM_SetupNextQuestion.AddListener((MatchingGameDataSO gameData, int attemptedQues, int quesCount) =>
            {
                SetupMatchingOptions(gameData.LeftOptionImages, gameData.RightOptionImages, gameData.CellSize,
                    gameData.Spacing);

                QuesCountText.text = $"Ques: {attemptedQues + 1}/{quesCount}";

                for (int i = 0; i < LivesImages.Length; i++)
                {
                    LivesImages[i].gameObject.SetActive(false);

                    if (i < gameData.LivesCount)
                    {
                        LivesImages[i].gameObject.SetActive(true);

                        if (ColorUtility.TryParseHtmlString("#DA3D3D", out Color newColor))
                        {
                            LivesImages[i].color = newColor;
                        }
                    }
                }
            });

            UIM_UpdateUIForCorrectAnswer.AddListener((bool complete) =>
            {
                if (complete)
                    _gameManager.GM_OnFullAnswerSetCorrect?.Invoke();
            });

            UIM_UpdateUIForIncorrectAnswer.AddListener((int count) =>
            {
                LivesImages[count].color = Color.white;

                if (count <= 0)
                    _gameManager.GM_OnFullAnswerSetIncorrect?.Invoke();
            });


            UIM_UpdateUIForCorrectFullAnswerSet.AddListener((Vector3 data, bool continueGame) =>
            {
                GameProtectorScreen.SetActive(true);

                _progressBarCoroutine =
                    StartCoroutine(ProgressBarAnimIncrease(ProgressBar.value,
                        (float)data.x / data.z));

                DOVirtual.DelayedCall(_averageAnimDurations * 0.9f, () =>
                {
                    AnswerSetResultPanel.SetActive(true);
                    AnswerSetResultPanelText.text = "Perfection!!";

                    DOVirtual.DelayedCall(_averageAnimDurations * 1.35f, () =>
                    {
                        AnswerSetResultPanel.SetActive(false);

                        if (!continueGame)
                            _gameManager.GM_OnGameOver?.Invoke();

                        else
                        {
                            GameProtectorScreen.SetActive(false);
                            _gameManager.GM_SetupNewQuestion?.Invoke();
                        }
                    });
                });
            });


            UIM_UpdateUIForIncorrectFullAnswerSet.AddListener(
                (int attemptedAnsCount, int questionsCount, bool continueGame) =>
                {
                    GameProtectorScreen.SetActive(true);

                    AnswerSetResultPanel.SetActive(true);
                    AnswerSetResultPanelText.text = "Out Of Lives";

                    DOVirtual.DelayedCall(_averageAnimDurations * 1.35f, () =>
                    {
                        AnswerSetResultPanel.SetActive(false);

                        if (continueGame)
                        {
                            GameProtectorScreen.SetActive(false);

                            _gameManager.GM_SetupNewQuestion?.Invoke();
                        }


                        else
                            _gameManager.GM_OnGameOver?.Invoke();
                    });
                });


            UIM_GameOver.AddListener((int score, int maxScore) =>
            {
                GameOverPanel.SetActive(true);

                int finalStars = GetStarsCount((float)score / maxScore);

                if (finalStars == 0)
                    GameOverText.text = "Try Harder Next Time";


                for (int i = 0; i < finalStars; i++)
                    AchievedStarsImages[i].color = AchievedStarColor;
            });

            UIM_OutsideClick.AddListener(DeselectCurrentOption);
        }


        /// <summary>
        /// Called when all questions are answered, sets up the score
        /// </summary>
        /// <param name="corrAns">Count of correct answers</param>
        /// <param name="totalQues">Total count of questions in the level</param>
        private void GameOverSetup(int corrAns, int totalQues)
        {
            /*_gameOver = true;
            DOVirtual.DelayedCall(_averageAnimDurations + 1, () => { GameOverPanel.SetActive(true); });

            for (int i = 0; i < GetStarsCount((float)corrAns, totalQues); i++)
                AchievedStarsImages[i].color = AchievedStarColor;*/
        }


        /// <summary>
        /// Event based, deselects the current active option on clicking the environment
        /// </summary>
        private void DeselectCurrentOption()
        {
            if (!_optionSelected)
                return;

            if (_currSelectedOptionBtn != null)
                OptionBtnClickAnim(_currSelectedOptionBtn, 2);

            ManageInteractableOnClick(_currSelectedOptionBtn.GetComponent<OptionsObject>(), false, false);

            _currSelectedOptionBtn = null;
            _optionSelected = false;
        }


        #region UI Animations Area

        /// <summary>
        /// Animation on clicking an option from matching
        /// </summary>
        /// <param name="btn">Button to change scale</param>
        /// <param name="sizeFactor">Sizes: 2 = normal, 1 = shrink, 3 = enlarge</param>
        private void OptionBtnClickAnim(Button btn, int sizeFactor = 2)
        {
            Vector3 size = new();

            if (sizeFactor == 3)
                size = new(1.3f, 1.3f, 1);

            else if (sizeFactor == 2)
                size = new(1f, 1f, 1);

            else if (sizeFactor == 1)
                size = new(0.5f, 0.5f, 1);

            btn.gameObject.transform.DOScale(size, _averageAnimDurations * 0.75f);
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

        #endregion


        #region Game mechanisms

        /// <summary>
        /// Initial setup for matching game
        /// </summary>
        /// <param name="leftList">List of sprites to be displayed on left side</param>
        /// <param name="rightList">List of sprites to be displayed on right side</param>
        private void SetupMatchingOptions(List<Sprite> leftList, List<Sprite> rightList, Vector2 cellSize,
            Vector2 spacing)
        {
            LeftOptionsParent.GetComponent<GridLayoutGroup>().cellSize = cellSize;
            LeftOptionsParent.GetComponent<GridLayoutGroup>().spacing = spacing;

            RightOptionsParent.GetComponent<GridLayoutGroup>().cellSize = cellSize;
            RightOptionsParent.GetComponent<GridLayoutGroup>().spacing = spacing;

            int helperVal;

            _leftOptionButtonList.Clear();
            _rightOptionButtonList.Clear();

            // Destroy old options
            for (int i = 0; i < LeftOptionsParent.childCount; i++)
            {
                Destroy(LeftOptionsParent.GetChild(i).gameObject);
                Destroy(RightOptionsParent.GetChild(i).gameObject);
            }

            for (int i = 0; i < leftList.Count; i++)
            {
                helperVal = i;

                OptionsObject btnLeft = Instantiate(MatchOptionObj, LeftOptionsParent, false);
                OptionsObject btnRight = Instantiate(MatchOptionObj, RightOptionsParent, false);

                ButtonEventsSetup(btnLeft, helperVal);
                ButtonEventsSetup(btnRight, helperVal);

                _leftOptionButtonList.Add(btnLeft);
                _rightOptionButtonList.Add(btnRight);

                btnLeft.GetComponent<Image>().sprite = leftList[i];
                btnRight.GetComponent<Image>().sprite = rightList[i];

                ManageMatchObjSize(leftList.Count);
                ManageMatchObjSize(rightList.Count);
            }

            ShuffleOptions(LeftOptionsParent);
            ShuffleOptions(RightOptionsParent);

            return;

            void RemoveCompsOnChoiceCorrect(OptionsObject obj)
            {
                obj.MatchedImage.SetActive(true);
                Destroy(obj.ObjButton);
                Destroy(obj);

                _currSelectedOptionBtn.GetComponent<OptionsObject>().MatchedImage.SetActive(true);
                Destroy(_currSelectedOptionBtn.GetComponent<OptionsObject>());
                Destroy(_currSelectedOptionBtn);
            }

            void ButtonEventsSetup(OptionsObject currBtn, int assignedNo)
            {
                currBtn.ObjButton.onClick.AddListener(() =>
                {
                    if (!_optionSelected)
                    {
                        _currSelectionNo = assignedNo;
                        _optionSelected = true;

                        _currSelectedOptionBtn = currBtn.ObjButton;

                        ManageInteractableOnClick(currBtn, true, false);

                        OptionBtnClickAnim(currBtn.ObjButton, 3);
                    }

                    // Re run disables the selection
                    else
                    {
                        _optionSelected = false;


                        // correct choice
                        if (assignedNo == _currSelectionNo && currBtn.ObjButton != _currSelectedOptionBtn)
                        {
                            _gameManager.GM_OnAnswerCorrect?.Invoke();

                            ManageInteractableOnClick(currBtn, false, true);
                            RemoveCompsOnChoiceCorrect(currBtn);

                            OptionBtnClickAnim(currBtn.ObjButton, 1);
                            OptionBtnClickAnim(_currSelectedOptionBtn, 1);

                            _currSelectedOptionBtn = null;
                            return;
                        }

                        // Incorrect choice
                        else if (assignedNo != _currSelectionNo)
                        {
                            _gameManager.GM_OnAnswerIncorrect?.Invoke();
                        }

                        OptionBtnClickAnim(currBtn.ObjButton, 2);
                        OptionBtnClickAnim(_currSelectedOptionBtn, 2);

                        ManageInteractableOnClick(currBtn, false, false);
                        _currSelectedOptionBtn = null;
                    }
                });
            }
        }


        /// <summary>
        /// Will only manage intractability of buttons, considers the old button and the parameter button too.
        /// </summary>
        /// <param name="currBtn">Button to manage intractability</param>
        /// <param name="disable">if true, disables all other buttons in the list of selected option</param>
        /// <param name="removeBtns">if true, removes the button provided in the parameter</param>
        private void ManageInteractableOnClick(OptionsObject currBtn, bool disable, bool removeBtns)
        {
            if (disable)
            {
                if (_leftOptionButtonList.Contains(currBtn))
                {
                    foreach (var btn in _leftOptionButtonList)
                    {
                        btn.ObjButton.interactable = (btn == currBtn);
                    }
                }

                else if (_rightOptionButtonList.Contains(currBtn))
                {
                    foreach (var btn in _rightOptionButtonList)
                    {
                        btn.ObjButton.interactable = (btn == currBtn);
                    }
                }
            }

            else
            {
                if (removeBtns)
                {
                    OptionsObject tempObj = _currSelectedOptionBtn != null
                        ? _currSelectedOptionBtn.GetComponent<OptionsObject>()
                        : null;

                    for (int i = 0; i < 2; i++)
                    {
                        if (tempObj != null)
                        {
                            if (_leftOptionButtonList.Contains(tempObj))
                                _leftOptionButtonList.RemoveAt(_leftOptionButtonList.IndexOf(tempObj));

                            else if (_rightOptionButtonList.Contains(tempObj))
                                _rightOptionButtonList.RemoveAt(_rightOptionButtonList.IndexOf(tempObj));
                        }

                        tempObj = currBtn;
                    }
                }

                foreach (var btn in _leftOptionButtonList.Concat(_rightOptionButtonList))
                {
                    btn.ObjButton.interactable = true;
                }
            }
        }


        /// <summary>
        /// Called at game start, shuffles lists and sets up in the parent.
        /// </summary>
        /// <param name="OptionsParent">Provide parent to shuffle children</param>
        private void ShuffleOptions(Transform OptionsParent)
        {
            for (int i = 0; i < OptionsParent.childCount; i++)
            {
                GameObject currentChild = OptionsParent.GetChild(i).gameObject;

                int randomIndex = Random.Range(i, OptionsParent.childCount);

                GameObject randomChild = OptionsParent.GetChild(randomIndex).gameObject;

                currentChild.transform.SetSiblingIndex(randomIndex);
                randomChild.transform.SetSiblingIndex(i);
            }
        }


        /// <summary>
        /// Maintains the size of Match option objects dynamically
        /// </summary>
        /// <param name="obj">Match options object</param>
        /// <param name="sizeFactor">Size of the list generally ranging from 4-10</param>
        private void ManageMatchObjSize(int sizeFactor)
        {
            /*int constCount = 0;

            if (sizeFactor == 3 || sizeFactor == 4 || sizeFactor >= 7)
                constCount = 4;
            else
                constCount = 3;

            LeftOptionsParent.GetComponent<GridLayoutGroup>().constraintCount = constCount;


            RightOptionsParent.GetComponent<GridLayoutGroup>().constraintCount = constCount;*/
        }

        #endregion
    }
}