using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MainGame.TurnsAndPatterns
{
    public class TnP_UIManager : MonoBehaviour
    {
        public static TnP_UIManager instance;

        #region Local Variables

        private TnP_GameManager _gameManager;

        private PatternDataSO _gameData;

        private List<RotateImgObj> _rotateImgObjList = new();
        private RotateImgObj _currSelObj;

        private Coroutine _progressBarCoroutine;
        private float _averageAnimDurations = 2f;
        private int _currStars = 0;

        private bool _isSingleQues = false;

        private int _currCorrAnsCount;

        #endregion

        #region Editor Variables

        [SerializeField] private float AvgAnimTime = 2;

        [Header("Header Area")] [SerializeField]
        private Image[] LivesImages;

        [SerializeField] private TextMeshProUGUI QuesCountText;


        [Header("Gameplay Area")] [SerializeField]
        private GameObject ObjSpawnParent;

        [SerializeField] private PresetObj PresetObj;
        [SerializeField] private RotateImgObj RotateImgObj;
        [SerializeField] private Button SubmitBtn;

        [SerializeField, Space(15)] private Slider ProgressBar;
        [SerializeField] private GameObject[] ProgressBarStars;

        [Space(35)] [SerializeField] private GameObject CorrectResultPanel;
        [SerializeField] private GameObject IncorrectResultPanel;
        [SerializeField] private GameObject AnswerSetResultPanel;
        [SerializeField] private TextMeshProUGUI AnswerSetResultPanelText;

        [Header("Game Over Area")] [SerializeField]
        private GameObject GameOverPanel;

        [SerializeField] private TextMeshProUGUI GameOverText;
        [SerializeField] private Image[] AchievedStarsImages;


        [Header("Misc Area")] [SerializeField] private Color AchievedStarColor;

        #endregion

        #region Unity Events

        [Space(35), Header("Events")] public UnityEvent UIM_OnGameStart = new();
        public UnityEvent<Vector2, PatternDataSO> UIM_SetupNextQuestion;
        public UnityEvent<Vector3, int, bool> UIM_UpdateUIForCorrectAnswer = new();
        public UnityEvent<int> UIM_UpdateUIForIncorrectAnswer = new();
        public UnityEvent<int, int, int, bool> UIM_UpdateUIForIncorrectFullAnswerSet = new();
        public UnityEvent<int, int> UIM_GameOver = new();

        #endregion

        #region Pre-Requisites

        private ImageDirection NumToImgDirEnum(int no)
        {
            int currNo = no % 8;

            return (ImageDirection)currNo;
        }

        private float ImgDirEnumToRotation(ImageDirection dir)
        {
            float zRotation = 0;

            switch (dir)
            {
                case ImageDirection.North:
                    zRotation = 0;
                    break;
                case ImageDirection.NorthEast:
                    zRotation = 45;
                    break;
                case ImageDirection.East:
                    zRotation = 90;
                    break;
                case ImageDirection.SouthEast:
                    zRotation = 135;
                    break;
                case ImageDirection.South:
                    zRotation = 180;
                    break;
                case ImageDirection.SouthWest:
                    zRotation = 225;
                    break;
                case ImageDirection.West:
                    zRotation = 270;
                    break;
                case ImageDirection.NorthWest:
                    zRotation = 315;
                    break;
            }

            return zRotation;
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
            _gameManager = TnP_GameManager.instance;

            UIM_OnGameStart.AddListener(() => { _gameManager.GM_SetupNewQuestion?.Invoke(); });

            UIM_SetupNextQuestion.AddListener((Vector2 quesCountData, PatternDataSO data) =>
            {
                for (int i = 0; i < LivesImages.Length; i++)
                {
                    LivesImages[i].gameObject.SetActive(false);

                    if (i < data.LivesCount)
                        LivesImages[i].gameObject.SetActive(true);
                }


                for (int i = 0; i < data.LivesCount; i++)
                {
                    if (ColorUtility.TryParseHtmlString("#DA3D3D", out Color newColor))
                    {
                        LivesImages[i].color = newColor;
                    }
                }

                SubmitBtn.interactable = true;

                QuesCountText.text = $"Ques: {quesCountData.x + 1}/{quesCountData.y}";

                _gameData = data;

                SetupGame();
            });

            UIM_UpdateUIForCorrectAnswer.AddListener((Vector3 progress, int livesCount, bool continueGame) =>
            {
                CorrectResultPanel.SetActive(true);
                SubmitBtn.interactable = false;


                _progressBarCoroutine =
                    StartCoroutine(ProgressBarAnimIncrease(ProgressBar.value,
                        (float)progress.x / progress.z));

                DOVirtual.DelayedCall(AvgAnimTime, () =>
                {
                    CorrectResultPanel.SetActive(false);

                    AnswerSetResultPanel.SetActive(true);
                    AnswerSetResultPanelText.text = "Perfection!!";

                    DOVirtual.DelayedCall(_averageAnimDurations * 1.35f, () =>
                    {
                        AnswerSetResultPanel.SetActive(false);

                        if (_isSingleQues || !continueGame)
                            _gameManager.GM_OnGameOver?.Invoke();

                        else
                        {
                            SubmitBtn.interactable = true;
                            QuesCountText.text = $"Ques: {progress.y + 1}/{progress.z}";

                            _gameManager.GM_SetupNewQuestion?.Invoke();
                        }
                    });
                });
            });

            UIM_UpdateUIForIncorrectAnswer.AddListener((int count) =>
            {
                IncorrectResultPanel.SetActive(true);
                SubmitBtn.interactable = false;


                DOVirtual.DelayedCall(AvgAnimTime * 0.9f, () =>
                {
                    IncorrectResultPanel.SetActive(false);
                    LivesImages[count].color = Color.white;
                    SubmitBtn.interactable = true;

                    if (count <= 0)
                    {
                        if (_isSingleQues)
                        {
                            SubmitBtn.interactable = false;

                            _progressBarCoroutine =
                                StartCoroutine(ProgressBarAnimIncrease(ProgressBar.value,
                                    (float)_currCorrAnsCount / _gameData.TotalBoxesCount -
                                    _gameData.PositionsList.Count));
                        }

                        _gameManager.GM_OnFullAnswerSetIncorrect?.Invoke();
                    }
                });
            });

            UIM_UpdateUIForIncorrectFullAnswerSet.AddListener(
                (int attemptedAnsCount, int questionsCount, int livesCount, bool continueGame) =>
                {
                    SubmitBtn.interactable = false;

                    AnswerSetResultPanel.SetActive(true);
                    AnswerSetResultPanelText.text = "Out Of Lives";

                    DOVirtual.DelayedCall(_averageAnimDurations * 1.35f, () =>
                    {
                        AnswerSetResultPanel.SetActive(false);

                        for (int i = 0; i < livesCount; i++)
                        {
                            if (ColorUtility.TryParseHtmlString("#DA3D3D", out Color newColor))
                            {
                                LivesImages[i].color = newColor;
                            }
                        }


                        if (continueGame)
                        {
                            QuesCountText.text = $"Ques: {attemptedAnsCount + 1}/{questionsCount}";
                            _gameManager.GM_SetupNewQuestion?.Invoke();
                        }


                        else
                            _gameManager.GM_OnGameOver?.Invoke();
                    });
                });


            UIM_GameOver.AddListener((int score, int maxScore) =>
            {
                if (_isSingleQues && score <= 0)
                    GameOverText.text = "Out Of Lives";

                score = _isSingleQues ? _currCorrAnsCount : score;
                maxScore = _isSingleQues ? _gameData.TotalBoxesCount - _gameData.PositionsList.Count : maxScore;


                GameOverPanel.SetActive(true);

                int finalStars = GetStarsCount((float)score / maxScore);

                if (!_isSingleQues && finalStars == 0)
                    GameOverText.text = "Try Harder Next Time";


                for (int i = 0; i < finalStars; i++)
                    AchievedStarsImages[i].color = AchievedStarColor;
            });
        }

        private void SetupGame()
        {
            _rotateImgObjList.Clear();

            for (int i = 0; i < ObjSpawnParent.transform.childCount; i++)
            {
                Destroy(ObjSpawnParent.transform.GetChild(i).gameObject);
            }

            int posNo = 0;
            for (int i = 0; i < _gameData.TotalBoxesCount; i++)
            {
                if (i == _gameData.PositionsList[posNo])
                {
                    PresetObj obj = Instantiate(PresetObj, ObjSpawnParent.transform, false);

                    obj.RotateImage.sprite = _gameData.RotateableSprite;
                    obj.Direction = NumToImgDirEnum(i);
                    obj.RotateImage.transform.rotation = Quaternion.Euler(0, 0, ImgDirEnumToRotation(obj.Direction));

                    posNo++;
                    posNo = Mathf.Clamp(posNo, 0, _gameData.PositionsList.Count - 1);
                }

                else
                {
                    RotateImgObj obj = Instantiate(RotateImgObj, ObjSpawnParent.transform, false);
                    _rotateImgObjList.Add(obj);

                    obj.RotateImage.sprite = _gameData.RotateableSprite;
                    obj.CurrDirection =
                        ImageDirection.North; // Always North in the beginning, controls will control new directions
                    obj.FinalDirection = NumToImgDirEnum(i);

                    obj.SelectionBtn.onClick.AddListener(() => { SetupBtnClickToSelect(obj); });
                }
            }

            return;

            void SetupBtnClickToSelect(RotateImgObj obj)
            {
                foreach (var v in _rotateImgObjList)
                {
                    v.SelectedSurroundImage.gameObject.SetActive(false);
                    v.SelectionBtn.interactable = true;
                }

                obj.SelectedSurroundImage.gameObject.SetActive(true);
                obj.SelectionBtn.interactable = false;

                _currSelObj = obj;
            }
        }

        public void RotateObj(int zRot)
        {
            if (_currSelObj == null)
            {
                Debug.Log("None Selected");
                return;
            }

            _currSelObj.RotateImage.transform.rotation =
                Quaternion.Euler(0, 0, _currSelObj.RotateImage.transform.rotation.eulerAngles.z + zRot);

            int rotNo = 0;

            int tempValue = zRot / 45 + (int)_currSelObj.CurrDirection;

            if (tempValue >= 0)
            {
                rotNo = tempValue - 8 * (tempValue / 8);
                Debug.Log(tempValue + "   " + rotNo);
            }
            else
            {
                rotNo = 8 + tempValue;
            }

            /*(zRot / 45 + (int)_currSelObj.CurrDirection) >= 0
                ? (zRot / 45 + (int)_currSelObj.CurrDirection)
                : 8 + (zRot / 45 + (int)_currSelObj.CurrDirection);*/

            _currSelObj.CurrDirection = (ImageDirection)rotNo;
        }

        public void CheckFinalAnswer()
        {
            foreach (var v in _rotateImgObjList)
            {
                v.SelectedSurroundImage.gameObject.SetActive(false);
                v.SelectionBtn.interactable = false;
            }

            _currCorrAnsCount = 0;
            int incorrectAnsCount = 0;

            foreach (var vObj in _rotateImgObjList)
            {
                if (vObj.CurrDirection != vObj.FinalDirection)
                {
                    incorrectAnsCount++;
                }
            }

            _currCorrAnsCount = _gameData.TotalBoxesCount - _gameData.PositionsList.Count - incorrectAnsCount;

            if (incorrectAnsCount > 0)
            {
                _gameManager.GM_OnAnswerIncorrect?.Invoke();
                _currSelObj = null;


                DOVirtual.DelayedCall(AvgAnimTime, () =>
                {
                    foreach (var v in _rotateImgObjList)
                    {
                        v.SelectionBtn.interactable = true;
                    }
                });

                return;
            }

            _gameManager.GM_OnAnswerCorrect?.Invoke();
        }

        #region UI Anim

        private IEnumerator ProgressBarAnimIncrease(float initialVal, float finalVal)
        {
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

            void RunGrowAndShrinkAnim(GameObject obj, Color newColor = default, bool useColor = false)
            {
                Vector3 OgSize1 = obj.transform.localScale;

                if (useColor)
                    obj.GetComponent<Image>().color = newColor;

                obj.transform.DOScale(OgSize1 + Vector3.one, _averageAnimDurations / 16)
                    .OnComplete(
                        () =>
                            obj.transform.DOScale(OgSize1, _averageAnimDurations / 16));
            }

            void ClaimProgressBarStar(int starNo)
            {
                RunGrowAndShrinkAnim(ProgressBarStars[starNo - 1], Color.yellow, true);
                RunGrowAndShrinkAnim(ProgressBar.handleRect.gameObject);
            }
        }

        #endregion
    }
}