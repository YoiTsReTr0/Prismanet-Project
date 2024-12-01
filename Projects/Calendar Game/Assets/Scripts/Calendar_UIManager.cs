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
    private Vector3 _currQuestion;

    private bool _isAddition = true;
    private int _currLightsOn = 0;
    private int _quesTotalCount;
    private int _quesAttemptedCount;

    #endregion

    #region Editor Variables

    [SerializeField, Space(35)] private float AvgAnimTime = 2;


    [Header("Header Area")]
    

    #endregion

    #region Unity Events

    [Space(35), Header("Events")] public UnityEvent<bool, int> UIM_OnGameStart = new();

    public UnityEvent<Vector3> UIM_SetupNextQuestion = new();
    public UnityEvent<int> UIM_GameOver = new();
    public UnityEvent<int, Vector3> UIM_UpdateUIForCorrectAnswer = new();
    public UnityEvent<int, Vector3> UIM_UpdateUIForIncorrectAnswer = new();

    #endregion

    #region Pre-requisites

    private void SmoothChange(float currentValue, float targetValue, float duration)
    {
        DOVirtual.Float(
            currentValue,
            targetValue,
            duration,
            (value) => { ProgressBar.value = value; }
        ).OnComplete(() => { });
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

        UIM_OnGameStart.AddListener((bool isAddition, int quesCount) =>
        {
            _isAddition = isAddition;
            _quesTotalCount = quesCount;
            ScoreText.text = "0 / " + quesCount.ToString();
        });

        UIM_SetupNextQuestion.AddListener();

        UIM_UpdateUIForCorrectAnswer.AddListener((int val, Vector3 dat) =>
        {
            ScoreText.text = _quesAttemptedCount + " / " + _quesTotalCount.ToString();
            //QuesText.text = " _____ ";
            SmoothChange(ProgressBar.value, (float)val / (float)_quesTotalCount, AvgAnimTime);

            SubmitBtn.interactable = false;

            DOVirtual.DelayedCall(AvgAnimTime, () =>
            {
                SubmitBtn.interactable = true;

                CorrectAnsResultPanel.SetActive(false);
                if (_quesAttemptedCount < _quesTotalCount)
                {
                    UIM_SetupNextQuestion?.Invoke(dat);
                }
            });
        });

        UIM_UpdateUIForIncorrectAnswer.AddListener((int val, Vector3 dat) =>
        {
            ScoreText.text = _quesAttemptedCount + " / " + _quesTotalCount.ToString();
            //QuesText.text = " _____ ";

            SubmitBtn.interactable = false;
            DOVirtual.DelayedCall(AvgAnimTime, () =>
            {
                SubmitBtn.interactable = true;

                IncorrectAnsResultPanel.SetActive(false);
                if (_quesAttemptedCount < _quesTotalCount)
                {
                    UIM_SetupNextQuestion?.Invoke(dat);
                }
            });
        });

        UIM_GameOver.AddListener((int count) =>
        {
            SubmitBtn.gameObject.SetActive(false);
            DOVirtual.DelayedCall(AvgAnimTime, () => { GameOverPanel.SetActive(true); });

            GameOverResultText.text = $"Score: {count} / {_quesTotalCount}";
        });

        SubmitBtn.onClick.AddListener();
    }
}