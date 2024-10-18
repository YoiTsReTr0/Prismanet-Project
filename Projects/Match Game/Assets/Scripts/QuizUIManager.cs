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

        #endregion

        #region Editor Variables

        [Header("Heading Area")] [SerializeField]
        private TextMeshProUGUI CoinsText;

        [SerializeField] private GameObject CoinsImage;

        [Header("Matching Area")] [SerializeField]
        private Transform LeftOptionsParent;

        [SerializeField] private Transform RightOptionsParent;
        [SerializeField] private OptionsObject MatchOptionObj;

        [Header("Game Over Area")] [SerializeField]
        private GameObject GameOverPanel;


        [Header("Misc Area")] [SerializeField, Tooltip("Minimum dimensions of match object, ref 4 objects")]
        private Vector2 MinMatchObjSize;

        [SerializeField, Tooltip("Maximum dimensions of match object, ref 10 objects")]
        private Vector2 MaxMatchObjSize;

        #endregion

        #region Unity Events

        [Header("Events")] public UnityEvent<List<Sprite>, List<Sprite>>
            UIM_OnGameStart = new(); // Called from Game Manager when game starts

        public UnityEvent UIM_SetupNextQuestion = new(); // Called from Game Manager when game starts
        public UnityEvent UIM_GameOver = new(); // Called from Game Manager when game starts
        public UnityEvent UIM_OutsideClick = new();

        public UnityEvent
            UIM_UpdateUIForCorrectAnswer = new(); // Called from Game Manager when game starts

        #endregion

        #region Helper Functions (depricated as of now. future usable)

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

            UIM_OnGameStart.AddListener((List<Sprite> leftList, List<Sprite> rightList) =>
            {
                SetupMatchingOptions(leftList, rightList);
                //GameOverPanel.SetActive(false);
            });

            //UIM_GameOver.AddListener(GameOverSetup);

            UIM_UpdateUIForCorrectAnswer.AddListener(UpdateUIForCorrectAnswer);

            UIM_OutsideClick.AddListener(DeselectCurrentOption);
        }


        /// <summary>
        /// Update the progress bar with each question answered correctly
        /// </summary>
        /// <param name="correctAns">Correct answer in the integer form simply</param>
        /// <param name="totalQues">Total count of questions for the level</param>
        private void UpdateUIForCorrectAnswer()
        {
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

            btn.gameObject.transform.DOScale(size, 1.5f);
        }

        #endregion


        #region Game mechanisms

        /// <summary>
        /// Initial setup for matching game
        /// </summary>
        /// <param name="leftList">List of sprites to be displayed on left side</param>
        /// <param name="rightList">List of sprites to be displayed on right side</param>
        private void SetupMatchingOptions(List<Sprite> leftList, List<Sprite> rightList)
        {
            int helperVal;


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
                            _gameManager.GM_OnOptionCorrect?.Invoke();

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
                            _gameManager.GM_OnOptionIncorrect?.Invoke();
                        }

                        OptionBtnClickAnim(currBtn.ObjButton, 2);
                        OptionBtnClickAnim(_currSelectedOptionBtn, 2);

                        ManageInteractableOnClick(currBtn, false, false);
                        _currSelectedOptionBtn = null;
                    }
                });
            }


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
            int constCount = 0;

            if (sizeFactor == 3 || sizeFactor == 4 || sizeFactor >= 7)
                constCount = 4;
            else
                constCount = 3;

            LeftOptionsParent.GetComponent<GridLayoutGroup>().constraintCount = constCount;


            RightOptionsParent.GetComponent<GridLayoutGroup>().constraintCount = constCount;
        }

        #endregion
    }
}