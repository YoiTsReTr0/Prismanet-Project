using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private List<Map_StateObject> _absentStates = new();
    private List<Map_StateObject> _wrongSelectedStates = new();
    private List<Map_IndexObject> _indexObjList = new();

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
    [SerializeField] private Transform IndexParent;

    [SerializeField] private Button SubmitBtn;
    [SerializeField] private GameObject PlayPauseBtnParent;

    #endregion

    #region Game Over Area

    [Header("Game Over Area")] [SerializeField]
    private GameObject GameOverPanel;

    [SerializeField] private Image[] AchievedStarsImages;

    #endregion

    #region Misc Area

    [Header("Misc Area")] [SerializeField] private Color AchievedStarColor;
    [SerializeField] private Color StateDeselectedColor;
    [SerializeField] private Color MapColorOnSelect;
    [SerializeField] private Color MapColorOnShowAns;
    [SerializeField] private Color MapTextColorOnSelect;
    [SerializeField] private Color MapTextColorOnShowAns;

    [Space(35), SerializeField] private GameObject StatesParent;
    [SerializeField] private GameObject FlagsParent;
    [Space(35), SerializeField] private Map_IndexObject IndexObjPrefab;

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

    /*private bool AllStatePresent()
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
    }*/

    private bool AllStatePresent()
    {
        _absentStates.Clear();
        _wrongSelectedStates.Clear();

        // Step 1: Find Absent States
        foreach (var answerState in _answerStates.AnswerStatesList)
        {
            if (!_selectedStates.Contains(answerState))
            {
                _absentStates.Add(answerState);
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
                _absentStates.Add(expandableSet.Set[0]); // Add the first state as a representative
            }
        }

        foreach (var selectedState in _selectedStates)
        {
            bool isInAnswerStates = _answerStates.AnswerStatesList.Contains(selectedState);

            bool isInExpandableStates = _answerStates.ExpandableStatesSetList
                .Any(expandableSet => expandableSet.Set.Contains(selectedState));

            if (!isInAnswerStates && !isInExpandableStates)
            {
                _wrongSelectedStates.Add(selectedState);
            }
        }

        return _absentStates.Count + _wrongSelectedStates.Count < 1;
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
            SubmitBtn.interactable = true;

            SetupNewQuestion(ques);
            QuesCountText.text = $"Ques: {attemptedQues + 1}/{_quesCount}";
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
            IncorrectResultPanel.SetActive(true);
            SubmitBtn.interactable = false;
            DOVirtual.DelayedCall(_averageAnimDurations * 0.9f, () =>
            {
                PlayPauseBtnParent.SetActive(true);

                IncorrectResultPanel.SetActive(false);

                AddMissingStatesOnFail();

                DOVirtual.DelayedCall(_averageAnimDurations * 2f, () =>
                {
                    PlayPauseBtnParent.SetActive(false);

                    ResetSelectedStatesOnAnswer();

                    if (attemptedQues >= _quesCount)
                        _gameManager.GM_OnGameOver?.Invoke();

                    else
                    {
                        UIM_SetupNextQuestion?.Invoke(ques, attemptedQues);
                    }
                });
            });
        });

        UIM_SelectStateObject.AddListener((Map_StateObject obj) =>
        {
            if (_flagTween != null)
                return;

            if (!_selectedStates.Contains(obj))
            {
                _selectedStates.Add(obj);

                SetFlagColor(obj, _selectedStates.Count, true);

                StateObjectSelectionAction(true, obj);

                SetupIndex(obj, _selectedStates.Count, true);
            }

            else
            {
                _selectedStates.Remove(obj);

                StateObjectSelectionAction(false, obj);
                SetupIndex(obj, int.Parse(obj.FlagText.text), false);

                ReassignFlagNumbers();
            }
        });

        UIM_GameOver.AddListener((int score, int maxScore) =>
        {
            GameOverPanel.SetActive(true);
            for (int i = 0; i < GetStarsCount((float)score / maxScore); i++)
                AchievedStarsImages[i].color = AchievedStarColor;

            //QuesCountText.text = $"Ques: {maxScore}/{_quesCount}";
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

    #region Game Core Mechanics

    private void GameSetup(int quesCount)
    {
        _quesCount = quesCount;
        QuesCountText.text = "Ques: 1/" + quesCount;

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

        for (int i = 0; i < _indexObjList.Count; i++)
        {
            Destroy(_indexObjList[i].gameObject);
        }

        _indexObjList.Clear();
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
            obj.FlagImage.gameObject.SetActive(true);

            float yVal = 0;
            Vector3 position = new(0, 0, 0);

            position.y = yVal + 120;
            obj.FlagImage.transform.GetChild(0).GetComponent<RectTransform>().localPosition = position;

            _flagTween = obj.FlagImage.transform.GetChild(0).GetComponent<RectTransform>()
                .DOLocalMoveY(yVal, _averageAnimDurations * 0.29f)
                .SetEase(Ease.InOutQuad).OnComplete(() => { _flagTween = null; });


            obj.GetComponent<Image>().color = Color.white;
        }

        else
        {
            obj.GetComponent<Image>().color = StateDeselectedColor;

            float yVal = obj.FlagImage.transform.GetChild(0).GetComponent<RectTransform>().localPosition.y;


            _flagTween = obj.FlagImage.transform.GetChild(0).GetComponent<RectTransform>()
                .DOLocalMoveY(yVal + 120, _averageAnimDurations * 0.29f)
                .SetEase(Ease.InOutQuad).OnComplete(() =>
                {
                    Vector3 position = obj.FlagImage.transform.GetChild(0).GetComponent<RectTransform>()
                        .localPosition;

                    position.y = yVal;
                    obj.FlagImage.transform.GetChild(0).GetComponent<RectTransform>().localPosition = position;
                    _flagTween = null;
                    obj.FlagImage.gameObject.SetActive(false);
                });
        }
    }

    private void AddMissingStatesOnFail()
    {
        for (int i = 0; i < _absentStates.Count; i++)
        {
            _selectedStates.Add(_absentStates[i]);

            SetFlagColor(_absentStates[i], _selectedStates.Count, false);
            StateObjectSelectionAction(true, _absentStates[i]);
            SetupIndex(_absentStates[i], _selectedStates.Count, true);
        }

        for (int i = 0; i < _wrongSelectedStates.Count; i++)
        {
            _selectedStates.Remove(_wrongSelectedStates[i]);

            SetFlagColor(_wrongSelectedStates[i], int.Parse(_wrongSelectedStates[i].FlagText.text), false);

            StateObjectSelectionAction(false, _wrongSelectedStates[i]);

            SetupIndex(_wrongSelectedStates[i],
                int.Parse(_wrongSelectedStates[i].FlagText.text), false);
        }

        ReassignFlagNumbers();
    }

    #endregion

    #region Helper Mechanics

    private void SetFlagColor(Map_StateObject obj, int flagNum, bool selecting = true)
    {
        Image img = obj.FlagImage.transform.GetChild(0).GetComponent<Image>();
        TextMeshProUGUI text = obj.FlagText;

        if (selecting)
        {
            img.color = MapColorOnSelect;
            text.text = flagNum.ToString();
            text.color = MapTextColorOnSelect;
        }

        else
        {
            img.color = MapColorOnShowAns;
            text.text = flagNum.ToString();
            text.color = MapTextColorOnShowAns;
        }
    }

    private void SetupIndex(Map_StateObject obj, int num, bool adding)
    {
        if (adding)
        {
            Map_IndexObject indexObj = Instantiate(IndexObjPrefab, IndexParent, false);
            _indexObjList.Add(indexObj);

            indexObj.StateData = obj;
            indexObj.StateName.text = obj.StateName;
            indexObj.IndexNumText.text = num.ToString();
        }

        else
        {
            Map_IndexObject indexObjToRemove = _indexObjList.Find(index => index.StateData == obj);

            if (indexObjToRemove != null)
            {
                _indexObjList.Remove(indexObjToRemove);
                Destroy(indexObjToRemove.gameObject);
            }
        }
    }

    private void SetupIndex(Map_StateObject obj, int num)
    {
        Map_IndexObject indexObj = _indexObjList.Find(index => index.StateData == obj);
        indexObj.IndexNumText.text = num.ToString();
    }

    private void ReassignFlagNumbers()
    {
        for (int i = 0; i < _selectedStates.Count; i++)
        {
            _selectedStates[i].FlagText.text = (i + 1).ToString();
            SetupIndex(_selectedStates[i], i + 1);
        }
    }


    public void AllDOTweenPausePlay(bool pause)
    {
        if (pause)
            DOTween.PauseAll();

        else
            DOTween.PlayAll();
    }

    #endregion

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

    #region Debug Area

    /*[ContextMenu("Fill Flags")]
    private void FillImageDataInStateObjects()
    {
        for (int i = 0; i < StatesParent.transform.childCount; i++)
        {
            StatesParent.transform.GetChild(i).GetComponent<Map_StateObject>().FlagImage =
                FlagsParent.transform.GetChild(i).gameObject;

            UnityEditor.EditorUtility.SetDirty(StatesParent.transform.GetChild(i).GetComponent<Map_StateObject>());
        }

        UnityEditor.EditorUtility.SetDirty(gameObject);
    }*/

    #endregion
}