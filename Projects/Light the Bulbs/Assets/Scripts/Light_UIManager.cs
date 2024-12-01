using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Light_UIManager : MonoBehaviour
{
    public static Light_UIManager instance;

    #region Local Variables

    private Light_GameManager _gameManager;
    private Vector3 _currQuestion;

    private bool _isAddition = true;
    private int _currLightsOn = 0;
    private int _quesTotalCount;
    private int _quesAttemptedCount;

    private int _currStars = 0;

    #endregion

    #region Editor Variables

    [SerializeField, Space(35)] private float AvgAnimTime = 2;

    [SerializeField] private Light_LightButton[] LightButtons;

    [SerializeField] private TextMeshProUGUI QuesText;
    [SerializeField] private TextMeshProUGUI ScoreText;

    [SerializeField] private GameObject CorrectAnsResultPanel;
    [SerializeField] private GameObject IncorrectAnsResultPanel;
    [SerializeField] private Color DefQuesTextColor;
    [SerializeField] private Slider ProgressBar;
    [SerializeField] private GameObject[] ProgressBarStars;
    [SerializeField] private Button SubmitBtn;

    [Space(35)] [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private TextMeshProUGUI GameOverResultText;

    [SerializeField] private Image[] AchievedStarsImages;

    [Header("Misc Area")] [SerializeField] private Color AchievedStarColor;

    #endregion

    #region Unity Events

    [Space(35), Header("Events")] public UnityEvent<bool, int> UIM_OnGameStart = new();

    public UnityEvent<Vector3> UIM_SetupNextQuestion = new();
    public UnityEvent<int> UIM_GameOver = new();
    public UnityEvent<int, Vector3> UIM_UpdateUIForCorrectAnswer = new();
    public UnityEvent<int, Vector3> UIM_UpdateUIForIncorrectAnswer = new();

    #endregion

    #region Pre-requisites

    private void RunGrowAndShrinkAnim(GameObject obj, Color newColor = default, bool useColor = false)
    {
        Vector3 OgSize1 = obj.transform.localScale;

        if (useColor)
            obj.GetComponent<Image>().color = newColor;

        obj.transform.DOScale(OgSize1 + Vector3.one, AvgAnimTime / 16)
            .OnComplete(
                () =>
                    obj.transform.DOScale(OgSize1, AvgAnimTime / 16));
    }

    void ClaimProgressBarStar(int starNo)
    {
        RunGrowAndShrinkAnim(ProgressBarStars[starNo - 1], Color.yellow, true);
        RunGrowAndShrinkAnim(ProgressBar.handleRect.gameObject);
    }

    private void SmoothChange(float currentValue, float targetValue, float duration)
    {
        DOVirtual.Float(
            currentValue,
            targetValue,
            duration,
            (value) =>
            {
                ProgressBar.value = value;
                if (_currStars < GetStarsCount(ProgressBar.value))
                {
                    _currStars++;
                    ClaimProgressBarStar(_currStars);
                }
            }
        ).OnComplete(() => { });
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
        _gameManager = Light_GameManager.instance;

        UIM_OnGameStart.AddListener((bool isAddition, int quesCount) =>
        {
            _isAddition = isAddition;
            _quesTotalCount = quesCount;
            ScoreText.text = "0 / " + quesCount.ToString();
        });

        UIM_SetupNextQuestion.AddListener(SetupQuestion);

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

            //GameOverResultText.text = $"Score: {count} / {_quesTotalCount}";

            for (int i = 0; i < GetStarsCount((float)count / _quesTotalCount); i++)
                AchievedStarsImages[i].color = AchievedStarColor;
        });

        SubmitBtn.onClick.AddListener(CheckAnswer);

        foreach (var t in LightButtons)
        {
            SetupBtnClick(t);
        }
    }

    private void SetupQuestion(Vector3 dat)
    {
        _currQuestion = dat;
        _currLightsOn = (int)dat.x;

        QuesText.color = DefQuesTextColor;
        string sign = _isAddition ? "+" : "-";
        QuesText.text = $"{dat.x} {sign} {dat.y} = ";

        for (int i = 0; i < LightButtons.Length; i++)
        {
            ManualLightControl(LightButtons[i], false);
            LightButtons[i].LightBtn.interactable = true;

            if (i < dat.x)
            {
                ManualLightControl(LightButtons[i], true);
                LightButtons[i].LightBtn.interactable =
                    false; // Remove this code to make already lights on to be clickable
            }
        }
    }

    private void SetupBtnClick(Light_LightButton btn)
    {
        btn.LightBtn.onClick.AddListener(() =>
        {
            btn.LightOn = !btn.LightOn;

            if (btn.LightOn)
                _currLightsOn++;
            else
                _currLightsOn--;

            btn.LightOnImage.gameObject.SetActive(btn.LightOn);
            btn.LightOffImage.gameObject.SetActive(!btn.LightOn);
        });
    }

    private void ManualLightControl(Light_LightButton btn, bool lightOn)
    {
        btn.LightOnImage.gameObject.SetActive(lightOn);
        btn.LightOffImage.gameObject.SetActive(!lightOn);
        btn.LightOn = lightOn;
    }

    private void CheckAnswer()
    {
        _quesAttemptedCount++;

        for (int i = 0; i < LightButtons.Length; i++)
        {
            LightButtons[i].LightBtn.interactable = false;
        }

        if (_currLightsOn == _currQuestion.z)
        {
            CorrectAnsResultPanel.SetActive(true);
            QuesText.color = Color.green;

            string sign = _isAddition ? "+" : "-";
            QuesText.text =
                $"{_currQuestion.x.ToString()} {sign} {_currQuestion.y.ToString()} = {_currQuestion.z.ToString()}";

            _gameManager.GM_OnAnswerCorrect?.Invoke();
        }

        else
        {
            IncorrectAnsResultPanel.SetActive(true);
            //QuesText.color = Color.red;

            string sign = _isAddition ? "+" : "-";
            QuesText.text =
                $"{_currQuestion.x.ToString()} {sign} {_currQuestion.y.ToString()} = {_currQuestion.z.ToString()} <u><color=red>not {_currLightsOn}</u>";


            _gameManager.GM_OnAnswerIncorrect?.Invoke();
        }
    }
}