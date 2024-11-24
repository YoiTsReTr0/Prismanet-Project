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
    private int _quesCount;

    #endregion

    #region Editor Variables

    [SerializeField] private Light_LightButton[] LightButtons;

    [SerializeField] private TextMeshProUGUI QuesText;
    [SerializeField] private TextMeshProUGUI ScoreText;

    [SerializeField] private GameObject CorrectAnsResultPanel;
    [SerializeField] private GameObject IncorrectAnsResultPanel;

    #endregion

    #region Unity Events

    [Space(35), Header("Events")] public UnityEvent<bool, int> UIM_OnGameStart = new();

    public UnityEvent<Vector3> UIM_SetupNextQuestion = new();
    public UnityEvent UIM_GameOver = new();
    public UnityEvent<int> UIM_UpdateUIForCorrectAnswer = new();

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
            _quesCount = quesCount;
            ScoreText.text = "0 / " + quesCount.ToString();
        });

        UIM_SetupNextQuestion.AddListener(SetupQuestion);

        UIM_UpdateUIForCorrectAnswer.AddListener((int val) =>
        {
            ScoreText.text = val + " / " + _quesCount.ToString();
            QuesText.text = " _____ ";
        });

        foreach (var t in LightButtons)
        {
            SetupBtnClick(t);
        }
    }

    private void SetupQuestion(Vector3 dat)
    {
        _currQuestion = dat;
        _currLightsOn = (int)dat.x;

        QuesText.color = Color.white;
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

    public void CheckAnswer()
    {
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

            DOVirtual.DelayedCall(2, () =>
            {
                _gameManager.GM_OnAnswerCorrect?.Invoke();
                CorrectAnsResultPanel.SetActive(false);
            });
        }

        else
        {
            IncorrectAnsResultPanel.SetActive(true);
            //QuesText.color = Color.red;

            string sign = _isAddition ? "+" : "-";
            QuesText.text =
                $"{_currQuestion.x.ToString()} {sign} {_currQuestion.y.ToString()} = {_currQuestion.z.ToString()} <u><color=red>not {_currLightsOn}</u>";

            DOVirtual.DelayedCall(2, () =>
            {
                _gameManager.GM_OnAnswerIncorrect?.Invoke();
                IncorrectAnsResultPanel.SetActive(false);
            });
        }
    }
}