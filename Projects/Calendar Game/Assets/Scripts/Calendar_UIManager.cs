using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Calendar_UIManager : MonoBehaviour
{
    public static Calendar_UIManager instance;

    #region Local Variables

    private Calendar_GameManager _gameManager;

    private Stack<Calendar_QuesData> _currQuesStack;
    private Calendar_QuesData _currQuesData;
    private Calendar_QuesUI _currQuesUI;

    private Coroutine _progressBarCoroutine;
    private float _averageAnimDurations = 2f;
    private int _currStars = 0;

    private CurrentQuesState _currQuesState;

    private string _correctSetString = "Yes, the sum is ";
    private string _incorrectSetString = "Out of Lives, the sum is ";

    #endregion

    #region Editor Variables

    #region General

    [SerializeField] private List<Calendar_QuesData> SetDataList = new();
    [SerializeField] private List<Calendar_QuesUI> QuestionUIList = new();

    #endregion

    #region Header Area

    [Header("Header Area")] [SerializeField]
    private Image[] LivesImages;

    [SerializeField] private TextMeshProUGUI QuesCountText;

    #endregion

    #region Main Game Area

    [Header("Main Game Area")] [SerializeField]
    private Slider ProgressBar;

    [SerializeField] private GameObject[] ProgressBarStars;

    [SerializeField] private GameObject QuesSurroundImage;

    [SerializeField] private GameObject CorrectResultPanel;
    [SerializeField] private GameObject IncorrectResultPanel;

    [SerializeField] private GameObject AnswerSetResultPanel;
    [SerializeField] private TextMeshProUGUI AnswerSetResultPanelText;

    [SerializeField] private Button SubmitBtn;

    #endregion

    #region Game Over Area

    [Header("Game Over Area")] [SerializeField]
    private GameObject GameOverPanel;

    [SerializeField] private Image[] AchievedStarsImages;

    #endregion

    #region Misc Area

    [Header("Misc Area")] [SerializeField] private Color AchievedStarColor;

    #endregion

    #endregion

    #region Unity Events

    [Space(35), Header("Events")] public UnityEvent<int, int> UIM_OnGameStart = new();

    public UnityEvent UIM_SetupNextQuestion = new();
    public UnityEvent<int, int> UIM_GameOver = new();
    public UnityEvent UIM_UpdateUIForCorrectAnswer = new();
    public UnityEvent<int> UIM_UpdateUIForIncorrectAnswer = new();
    public UnityEvent<Vector3, int, bool> UIM_UpdateUIForCorrectFullAnswerSet = new();
    public UnityEvent<int, int, int, bool> UIM_UpdateUIForIncorrectFullAnswerSet = new();

    #endregion

    #region Pre-requisites

    /// <summary>
    /// Very efficient method to shuffle T
    /// </summary>
    /// <param name="list">List of type T</param>
    /// <typeparam name="T">Can be anything</typeparam>
    private void ShuffleList<T>(List<T> list)
    {
        // Fisher-Yates shuffle algorithm
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    /// <summary>
    /// Very efficient method to shuffle T
    /// </summary>
    /// <param name="array"></param>
    /// <typeparam name="T"></typeparam>
    private void ShuffleArray<T>(T[] array)
    {
        // Fisher-Yates shuffle algorithm
        for (int i = array.Length - 1; i > 0; i--)
        {
            int rand = UnityEngine.Random.Range(0, i + 1); // Use Unity's Random class
            T temp = array[i];
            array[i] = array[rand];
            array[rand] = temp;
        }
    }

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

    /// <summary>
    /// Sets active the question box, called when question is updated
    /// </summary>
    private void SetActiveQuestionBoxPerState()
    {
        foreach (var set in QuestionUIList)
        {
            set.HeadingText.transform.parent.gameObject.SetActive(false);
        }

        switch (_currQuesState)
        {
            case CurrentQuesState.FindStage:
                QuestionUIList[0].HeadingText.transform.parent.gameObject.SetActive(true);
                QuestionUIList[0].InputField.text = "";
                break;
            case CurrentQuesState.AddStage:
                QuestionUIList[1].HeadingText.transform.parent.gameObject.SetActive(true);
                QuestionUIList[1].InputField.text = "";
                break;
            case CurrentQuesState.MultiplyStage:
                QuestionUIList[2].HeadingText.transform.parent.gameObject.SetActive(true);
                QuestionUIList[2].InputField.text = "";
                break;
        }
    }

    /// <summary>
    /// Get Question UI box data per the current state
    /// </summary>
    /// <returns>Returns a type of Calendar_QuesUI</returns>
    private Calendar_QuesUI GetQuestionBoxPerState()
    {
        switch (_currQuesState)
        {
            case CurrentQuesState.FindStage:
                return QuestionUIList[0];
            case CurrentQuesState.AddStage:
                return QuestionUIList[1];
            case CurrentQuesState.MultiplyStage:
                return QuestionUIList[2];
        }

        return QuestionUIList[0];
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
        _gameManager = Calendar_GameManager.instance;

        SubmitBtn.onClick.AddListener(CheckAnswer);

        UIM_OnGameStart.AddListener((int quesCount, int LivesPerQues) =>
        {
            GameSetup(quesCount);
            for (int i = 0; i < LivesPerQues; i++)
            {
                LivesImages[i].gameObject.SetActive(true);
            }
        });

        UIM_SetupNextQuestion.AddListener(() =>
        {
            SubmitBtn.interactable = true;
            SetupNewQuestion();
        });

        UIM_UpdateUIForCorrectAnswer.AddListener(() =>
        {
            CorrectResultPanel.SetActive(true);
            SubmitBtn.interactable = false;
            DOVirtual.DelayedCall(_averageAnimDurations * 0.9f, () =>
            {
                SubmitBtn.interactable = true;
                CorrectResultPanel.SetActive(false);
                UpdateQuestion();
            });
        });

        UIM_UpdateUIForIncorrectAnswer.AddListener((int count) =>
        {
            IncorrectResultPanel.SetActive(true);
            SubmitBtn.interactable = false;
            DOVirtual.DelayedCall(_averageAnimDurations * 0.9f, () =>
            {
                SubmitBtn.interactable = true;
                IncorrectResultPanel.SetActive(false);
                LivesImages[count].color = Color.white;
                _currQuesUI.InputField.text = "";

                if (count <= 0)
                    _gameManager.GM_OnFullAnswerSetIncorrect?.Invoke();
            });
        });

        UIM_UpdateUIForCorrectFullAnswerSet.AddListener(
            (Vector3 progress, int livesCount, bool continueGame) =>
            {
                SubmitBtn.interactable = false;

                AnswerSetResultPanel.SetActive(true);
                AnswerSetResultPanelText.text = _correctSetString + ((_currQuesData.SmallestNum + 8) * 9).ToString();

                DOVirtual.DelayedCall(_averageAnimDurations * 1.35f, () =>
                {
                    _progressBarCoroutine =
                        StartCoroutine(ProgressBarAnimIncrease(ProgressBar.value,
                            (float)progress.x / progress.z));

                    AnswerSetResultPanel.SetActive(false);

                    for (int i = 0; i < livesCount; i++)
                    {
                        if (ColorUtility.TryParseHtmlString("#DA3D3D", out Color newColor))
                        {
                            LivesImages[i].color = newColor;
                        }
                    }


                    /*DOVirtual.DelayedCall(_averageAnimDurations * 0.9f, () =>
                    {*/
                    if (continueGame)
                    {
                        QuesCountText.text = $"Ques: {progress.y}/{progress.z}";
                        UIM_SetupNextQuestion?.Invoke();
                    }

                    else
                    {
                        _gameManager.GM_OnGameOver?.Invoke();
                    }
                    //});
                });
            });

        UIM_UpdateUIForIncorrectFullAnswerSet.AddListener(
            (int attemptedAnsCount, int questionsCount, int livesCount, bool continueGame) =>
            {
                SubmitBtn.interactable = false;

                AnswerSetResultPanel.SetActive(true);
                AnswerSetResultPanelText.text = _incorrectSetString + ((_currQuesData.SmallestNum + 8) * 9).ToString();

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
                        QuesCountText.text = $"Ques: {attemptedAnsCount}/{questionsCount}";

                        UIM_SetupNextQuestion?.Invoke();
                    }

                    else
                    {
                        _gameManager.GM_OnGameOver?.Invoke();
                    }
                });
            });

        UIM_GameOver.AddListener((int score, int maxScore) =>
        {
            GameOverPanel.SetActive(true);
            for (int i = 0; i < GetStarsCount((float)score / maxScore); i++)
                AchievedStarsImages[i].color = AchievedStarColor;
        });
    }

    private void GameSetup(int quesCount)
    {
        ShuffleList(SetDataList);
        _currQuesStack = new(SetDataList);

        QuesCountText.text = "Ques: 0/" + quesCount;
    }


    private void SetupNewQuestion()
    {
        _currQuesData = _currQuesStack.Pop();

        QuesSurroundImage.transform.SetParent(_currQuesData.ContainerTransform);
        QuesSurroundImage.transform.localPosition = Vector3.zero;

        _currQuesUI = GetQuestionBoxPerState();

        _currQuesState = CurrentQuesState.FindStage;
        SetActiveQuestionBoxPerState();
    }

    /// <summary>
    /// Check the answer by reading the value in the current active input field.
    /// If (correct and) current stage is multiply stage then OnFullAnswerSetCorrect event will be called else other events
    /// </summary>
    public void CheckAnswer()
    {
        Calendar_QuesUI currSet = GetQuestionBoxPerState();
        int currNo = 0;
        switch (_currQuesState)
        {
            case CurrentQuesState.FindStage:
                currNo = _currQuesData.SmallestNum;
                break;

            case CurrentQuesState.AddStage:
                currNo = _currQuesData.SmallestNum + 8;
                break;

            case CurrentQuesState.MultiplyStage:
                currNo = (_currQuesData.SmallestNum + 8) * 9;
                break;
        }

        if (currSet.InputField.text == currNo.ToString())
        {
            if (_currQuesState == CurrentQuesState.MultiplyStage)
                _gameManager.GM_OnFullAnswerSetCorrect?.Invoke();

            else
                _gameManager.GM_OnAnswerCorrect?.Invoke();
        }

        else
        {
            _gameManager.GM_OnAnswerIncorrect?.Invoke();
        }
    }

    private void UpdateQuestion()
    {
        _currQuesUI = GetQuestionBoxPerState();
        _currQuesUI.HeadingText.transform.parent.gameObject.SetActive(false);

        _currQuesState++;
        SetActiveQuestionBoxPerState();

        if (_currQuesState == CurrentQuesState.AddStage)
        {
            _currQuesUI = GetQuestionBoxPerState();
            _currQuesUI.HeadingText.text = "Add 8 to " + _currQuesData.SmallestNum + " =";
        }
        else
        {
            _currQuesUI = GetQuestionBoxPerState();
            _currQuesUI.HeadingText.text = "Multiply " + (_currQuesData.SmallestNum + 8) + " by 9 =";
        }
    }


    #region UI Anims

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
}