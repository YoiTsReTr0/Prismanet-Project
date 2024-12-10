using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Map_UIManager : MonoBehaviour
{
    public static Map_UIManager instance;

    #region Local Variables

    private Map_GameManager _gameManager;


    private Coroutine _progressBarCoroutine;
    private float _averageAnimDurations = 2f;
    private int _currStars = 0;
    private int _quesCount = 0;

    private QuestionStateObjectSet _answerStates = new();
    private List<Map_StateObject> _selectedStates = new();
    private Tween _flagTween;

    #endregion

    #region Editor Variables

    #region General

    #endregion

    #region Header Area

    [Header("Header Area")]
    // Use if required
    //[SerializeField] private Image[] LivesImages;
    [SerializeField]
    private TextMeshProUGUI QuesCountText;

    #endregion

    #region Main Game Area

    [Header("Main Game Area")] [SerializeField]
    private Slider ProgressBar;

    [SerializeField] private GameObject[] ProgressBarStars;
    [SerializeField] private TextMeshProUGUI FromStateText;
    [SerializeField] private TextMeshProUGUI ToStateText;


    [SerializeField] private GameObject CorrectResultPanel;
    [SerializeField] private GameObject IncorrectResultPanel;


    [SerializeField] private Button SubmitBtn;

    #endregion

    #region Game Over Area

    [Header("Game Over Area")] [SerializeField]
    private GameObject GameOverPanel;

    [SerializeField] private Image[] AchievedStarsImages;

    #endregion

    #region Misc Area

    [Header("Misc Area")] [SerializeField] private Color AchievedStarColor;
    [SerializeField] private Color StateDeselectedColor;

    #endregion

    #endregion

    #region Unity Events

    [Space(35), Header("Events")] public UnityEvent<int> UIM_OnGameStart = new();

    public UnityEvent<QuestionStateObjectSet, int> UIM_SetupNextQuestion = new();
    public UnityEvent<int, int> UIM_GameOver = new();
    public UnityEvent<QuestionStateObjectSet, int, int> UIM_UpdateUIForCorrectAnswer = new();
    public UnityEvent<QuestionStateObjectSet, int> UIM_UpdateUIForIncorrectAnswer = new();
    public UnityEvent<Map_StateObject> UIM_SelectStateObject = new();

    #endregion

    #region Pre-requisites

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

    private bool AllStatePresent()
    {
        foreach (var answerState in _answerStates.AnswerStatesList)
        {
            if (!_selectedStates.Contains(answerState))
            {
                return false;
            }
        }

        foreach (var expandableSet in _answerStates.ExpandableStatesSetList)
        {
            bool hasAtLeastOne = false;
            foreach (var state in expandableSet.Set)
            {
                if (_selectedStates.Contains(state))
                {
                    hasAtLeastOne = true;
                    break;
                }
            }

            if (!hasAtLeastOne)
            {
                return false;
            }
        }

        return true;
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
        _gameManager = Map_GameManager.instance;


        UIM_OnGameStart.AddListener((int quesCount) =>
        {
            Debug.Log("Game Start");
            GameSetup(quesCount);
        });

        UIM_SetupNextQuestion.AddListener((QuestionStateObjectSet ques, int attemptedQues) =>
        {
            SetupNewQuestion(ques);
            QuesCountText.text = $"Ques: {attemptedQues}/{_quesCount}";
        });

        UIM_UpdateUIForCorrectAnswer.AddListener((QuestionStateObjectSet ques, int attemptedQues, int correctAns) =>
        {
            ResetSelectedStatesOnAnswer();
            CorrectResultPanel.SetActive(true);
            SubmitBtn.interactable = false;
            _progressBarCoroutine =
                StartCoroutine(ProgressBarAnimIncrease(ProgressBar.value, (float)correctAns / _quesCount));

            DOVirtual.DelayedCall(_averageAnimDurations * 0.9f, () =>
            {
                SubmitBtn.interactable = true;
                CorrectResultPanel.SetActive(false);

                if (attemptedQues >= _quesCount)
                    _gameManager.GM_OnGameOver?.Invoke();

                else
                {
                    UIM_SetupNextQuestion?.Invoke(ques, attemptedQues);
                }
            });
        });

        UIM_UpdateUIForIncorrectAnswer.AddListener((QuestionStateObjectSet ques, int attemptedQues) =>
        {
            ResetSelectedStatesOnAnswer();
            IncorrectResultPanel.SetActive(true);
            SubmitBtn.interactable = false;
            DOVirtual.DelayedCall(_averageAnimDurations * 0.9f, () =>
            {
                SubmitBtn.interactable = true;
                IncorrectResultPanel.SetActive(false);
                //LivesImages[currLives].color = Color.white;

                if (attemptedQues >= _quesCount)
                    _gameManager.GM_OnGameOver?.Invoke();

                else
                {
                    UIM_SetupNextQuestion?.Invoke(ques, attemptedQues);
                }
            });
        });

        UIM_SelectStateObject.AddListener((Map_StateObject obj) =>
        {
            if (_flagTween != null)
                return;

            if (!_selectedStates.Contains(obj))
            {
                _selectedStates.Add(obj);

                StateObjectSelectionAction(true, obj);
            }

            else
            {
                _selectedStates.Remove(obj);

                StateObjectSelectionAction(false, obj);
            }
        });

        UIM_GameOver.AddListener((int score, int maxScore) =>
        {
            GameOverPanel.SetActive(true);
            for (int i = 0; i < GetStarsCount((float)score / maxScore); i++)
                AchievedStarsImages[i].color = AchievedStarColor;
        });

        SubmitBtn.onClick.AddListener(CheckAnswer);

        return;

        void ResetSelectedStatesOnAnswer()
        {
            foreach (var state in _selectedStates)
            {
                StateObjectSelectionAction(false, state);
            }

            _selectedStates.Clear(); // extra layer
        }
    }

    private void GameSetup(int quesCount)
    {
        _quesCount = quesCount;
        QuesCountText.text = "Ques: 0/" + quesCount;

        /*for (int i = 0; i < LivesPerQues; i++)
        {
            LivesImages[i].gameObject.SetActive(true);
        }*/
    }


    private void SetupNewQuestion(QuestionStateObjectSet ques)
    {
        _answerStates = ques;

        FromStateText.text = ques.AnswerStatesList[0].StateName;
        ToStateText.text = ques.AnswerStatesList[^1].StateName;
    }

    /// <summary>
    /// Check the answer by reading the value in the current active input field.
    /// If (correct and) current stage is multiply stage then OnFullAnswerSetCorrect event will be called else other events
    /// </summary>
    private void CheckAnswer()
    {
        if (AllStatePresent())
        {
            _gameManager.GM_OnAnswerCorrect?.Invoke();
        }

        else
        {
            _gameManager.GM_OnAnswerIncorrect?.Invoke();
        }
    }

    private void StateObjectSelectionAction(bool selected, Map_StateObject obj)
    {
        if (selected)
        {
            obj.SelectedImage.gameObject.SetActive(true);

            float yVal = obj.SelectedImage.rectTransform.localPosition.y;
            Vector3 position = obj.SelectedImage.rectTransform.localPosition;

            position.y = yVal + 120;
            obj.SelectedImage.rectTransform.localPosition = position;

            _flagTween = obj.SelectedImage.rectTransform
                .DOLocalMoveY(yVal, _averageAnimDurations * 0.29f)
                .SetEase(Ease.InOutQuad).OnComplete(() => { _flagTween = null; });


            obj.GetComponent<Image>().color = Color.white;
        }

        else
        {
            obj.GetComponent<Image>().color = StateDeselectedColor;

            float yVal = obj.SelectedImage.rectTransform.localPosition.y;


            _flagTween = obj.SelectedImage.rectTransform
                .DOLocalMoveY(yVal + 120, _averageAnimDurations * 0.29f)
                .SetEase(Ease.InOutQuad).OnComplete(() =>
                {
                    Vector3 position = obj.SelectedImage.rectTransform.localPosition;

                    position.y = yVal;
                    obj.SelectedImage.rectTransform.localPosition = position;
                    _flagTween = null;
                    obj.SelectedImage.gameObject.SetActive(false);
                });
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