using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Hexagon_UIManager : MonoBehaviour
{
    public static Hexagon_UIManager instance;

    #region Local Variables

    private Hexagon_GameManager _gameManager;

    private Coroutine _progressBarCoroutine;
    private float _averageAnimDurations = 2f;
    private int _currStars = 0;

    private bool _isSingleQues = false;

    private int _currHiddenCorrAnsCount;
    private int _hiddenAnsCount = 6;

    private int _hexLowerRange;
    private int _hexHigherRange;

    #endregion

    #region Editor Variables

    #region Header Area

    [Header("Header Area")] [SerializeField]
    private Image[] LivesImages;

    [SerializeField] private TextMeshProUGUI QuesCountText;

    #endregion

    #region Main Game Area

    [Header("Main Game Area")] [SerializeField]
    private Hexagon_HexagonObject[] CircleObjects;

    [SerializeField] private Hexagon_HexagonObject[] SquareObjects;

    [Space(20), SerializeField] private GameObject CorrectResultPanel;
    [SerializeField] private GameObject IncorrectResultPanel;

    [SerializeField] private Slider ProgressBar;
    [SerializeField] private GameObject[] ProgressBarStars;
    [SerializeField] private Button SubmitBtn;


    [Space(20), SerializeField] private GameObject AnswerSetResultPanel;
    [SerializeField] private TextMeshProUGUI AnswerSetResultPanelText;

    #endregion

    #region Game Over Area

    [Header("Game Over Area")] [SerializeField]
    private GameObject GameOverPanel;

    [SerializeField] private TextMeshProUGUI GameOverText;

    [SerializeField] private Image[] AchievedStarsImages;

    #endregion

    #region Misc Area

    [Header("Misc Area")] [SerializeField] private Color AchievedStarColor;

    #endregion

    #endregion

    #region Unity Events

    public UnityEvent<Vector3, int, int, bool> UIM_OnGameStart = new();
    public UnityEvent<int> UIM_SetupNextQuestion = new();

    public UnityEvent<Vector3, bool> UIM_UpdateUIForCorrectAnswer = new();
    public UnityEvent<int> UIM_UpdateUIForIncorrectAnswer = new();
    public UnityEvent<int, int, int, bool> UIM_UpdateUIForIncorrectFullAnswerSet = new();
    public UnityEvent<int, int> UIM_GameOver = new();

    #endregion

    #region Pre-requisites

    private bool GetAnswersResult()
    {
        if (!_isSingleQues)
        {
            for (int i = 0; i < CircleObjects.Length; i++)
            {
                if (CircleObjects[i].PreDefinedNumText.text != CircleObjects[i].InputField.text)
                {
                    return false;
                }


                if (SquareObjects[i].PreDefinedNumText.text != SquareObjects[i].InputField.text)
                {
                    return false;
                }
            }
        }
        else
        {
            _currHiddenCorrAnsCount = 0;

            for (int i = 0; i < CircleObjects.Length; i++)
            {
                if (!CircleObjects[i].PreDefinedNumText.gameObject.activeSelf &&
                    CircleObjects[i].PreDefinedNumText.text == CircleObjects[i].InputField.text)
                {
                    _currHiddenCorrAnsCount++;
                }


                if (!SquareObjects[i].PreDefinedNumText.gameObject.activeSelf &&
                    SquareObjects[i].PreDefinedNumText.text == SquareObjects[i].InputField.text)
                {
                    _currHiddenCorrAnsCount++;
                }
            }

            if (_hiddenAnsCount > _currHiddenCorrAnsCount)
                return false;
        }

        return true;
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

    private void SetAllHexInputFieldsInteractable(bool interactable)
    {
        for (int i = 0; i < CircleObjects.Length; i++)
        {
            CircleObjects[i].InputField.interactable = interactable;
            SquareObjects[i].InputField.interactable = interactable;
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
        _gameManager = Hexagon_GameManager.instance;


        UIM_OnGameStart.AddListener(
            (Vector3 dataSet1, int hiddenAnsCount, int quesCount, bool isSingleQues) =>
            {
                _isSingleQues = isSingleQues;
                _hiddenAnsCount = hiddenAnsCount;

                _hexLowerRange = (int)dataSet1.y;
                _hexHigherRange = (int)dataSet1.z;

                QuesCountText.gameObject.SetActive(!isSingleQues);
                QuesCountText.text = $"Ques: 0/{quesCount}";


                for (int i = 0; i < dataSet1.x; i++)
                {
                    LivesImages[i].gameObject.SetActive(true);
                }
            });

        UIM_SetupNextQuestion.AddListener((int livesCount) =>
        {
            for (int i = 0; i < livesCount; i++)
            {
                if (ColorUtility.TryParseHtmlString("#DA3D3D", out Color newColor))
                {
                    LivesImages[i].color = newColor;
                }
            }

            SetAllHexInputFieldsInteractable(true);

            SubmitBtn.interactable = true;

            HexagonQuestionSetup(_hexLowerRange, _hexHigherRange);
        });


        UIM_UpdateUIForCorrectAnswer.AddListener((Vector3 progress, bool continueGame) =>
        {
            CorrectResultPanel.SetActive(true);
            SubmitBtn.interactable = false;

            QuesCountText.text = $"Ques: {progress.y}/{progress.z}";

            _progressBarCoroutine =
                StartCoroutine(ProgressBarAnimIncrease(ProgressBar.value,
                    (float)progress.x / progress.z));

            DOVirtual.DelayedCall(_averageAnimDurations * 0.9f, () =>
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

                        _gameManager.GM_SetupNewQuestion?.Invoke();
                    }
                });
            });
        });

        UIM_UpdateUIForIncorrectAnswer.AddListener((int count) =>
        {
            IncorrectResultPanel.SetActive(true);
            SubmitBtn.interactable = false;

            DOVirtual.DelayedCall(_averageAnimDurations * 0.9f, () =>
            {
                IncorrectResultPanel.SetActive(false);
                LivesImages[count].color = Color.white;
                SubmitBtn.interactable = true;

                if (count <= 0)
                {
                    SetAllHexInputFieldsInteractable(false);

                    if (_isSingleQues)
                    {
                        SubmitBtn.interactable = false;


                        _progressBarCoroutine =
                            StartCoroutine(ProgressBarAnimIncrease(ProgressBar.value,
                                (float)_currHiddenCorrAnsCount / _hiddenAnsCount));

                        /*DOVirtual.DelayedCall(_averageAnimDurations * 1.35f,
                            () => { _gameManager.GM_OnGameOver?.Invoke(); });*/
                    }

                    _gameManager.GM_OnFullAnswerSetIncorrect?.Invoke();
                    /*else
                    {
                    }*/
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
                        _gameManager.GM_SetupNewQuestion?.Invoke();


                    else
                        _gameManager.GM_OnGameOver?.Invoke();


                    QuesCountText.text = $"Ques: {attemptedAnsCount}/{questionsCount}";
                });
            });


        UIM_GameOver.AddListener((int score, int maxScore) =>
        {
            SetAllHexInputFieldsInteractable(false);

            if (_isSingleQues && score <= 0)
                GameOverText.text = "Out Of Lives";

            score = _isSingleQues ? _currHiddenCorrAnsCount : score;
            maxScore = _isSingleQues ? _hiddenAnsCount : maxScore;


            GameOverPanel.SetActive(true);

            int finalStars = GetStarsCount((float)score / maxScore);

            if (!_isSingleQues && finalStars == 0)
                GameOverText.text = "Try Harder Next Time";


            for (int i = 0; i < finalStars; i++)
                AchievedStarsImages[i].color = AchievedStarColor;
        });

        SubmitBtn.onClick.AddListener(CheckAnswer);
    }

    private void HexagonQuestionSetup(int lowerRange, int highRange)
    {
        GeneratePuzzle(lowerRange, highRange);

        return;

        void GeneratePuzzle(int lowerRange, int highRange)
        {
            for (int i = 0; i < CircleObjects.Length; i++)
            {
                CircleObjects[i].FinalNumber = Random.Range(lowerRange, highRange);
                CircleObjects[i].PreDefinedNumText.text = CircleObjects[i].FinalNumber.ToString();
                CircleObjects[i].InputField.text = CircleObjects[i].FinalNumber.ToString();
            }

            for (int i = 0; i < CircleObjects.Length; i++)
            {
                SquareObjects[i].FinalNumber = CircleObjects[i].FinalNumber * CircleObjects[(i + 1) % 6].FinalNumber;
                SquareObjects[i].PreDefinedNumText.text = SquareObjects[i].FinalNumber.ToString();
                SquareObjects[i].InputField.text = SquareObjects[i].FinalNumber.ToString();
            }

            List<int> hideIndices = new List<int>();
            while (hideIndices.Count < _hiddenAnsCount)
            {
                int randomIndex = Random.Range(0, 6);
                if (!hideIndices.Contains(randomIndex))
                    hideIndices.Add(randomIndex);
            }

            // Hide random Circle or Square values
            foreach (int index in hideIndices)
            {
                if (Random.value > 0.5f)
                {
                    CircleObjects[index].PreDefinedNumText.gameObject.SetActive(false);
                    CircleObjects[index].InputField.gameObject.SetActive(true);
                    CircleObjects[index].InputField.text = "";
                }
                else
                {
                    SquareObjects[index].PreDefinedNumText.gameObject.SetActive(false);
                    SquareObjects[index].InputField.gameObject.SetActive(true);
                    SquareObjects[index].InputField.text = "";
                }
            }

            // Step 4: Validate the puzzle
            if (!ValidatePuzzle())
            {
                Debug.LogWarning("Generated puzzle is unsolvable. Retrying...");
                GeneratePuzzle(lowerRange, highRange);
                return;
            }

            // Step 5: Display the puzzle (for testing)
            Debug.Log("Puzzle Generated!");
        }


        bool ValidatePuzzle()
        {
            // Ensure at least two consecutive circles are visible
            int visibleCircles = 0;
            for (int i = 0; i < 6; i++)
            {
                if (CircleObjects[i].FinalNumber != -1) visibleCircles++;
                if (visibleCircles >= 2) return true; // Puzzle is solvable
            }

            return false; // Not enough visible circles
        }
    }

    private void CheckAnswer()
    {
        if (!GetAnswersResult())
            _gameManager.GM_OnAnswerIncorrect?.Invoke();

        else
            _gameManager.GM_OnAnswerCorrect?.Invoke();
    }


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
}