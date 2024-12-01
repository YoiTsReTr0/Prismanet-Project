using UnityEngine;
using UnityEngine.Events;

public class Calendar_GameManager : MonoBehaviour
{
    public static Calendar_GameManager instance;

    #region Local Variables

    private Calendar_UIManager _uiManager;

    [SerializeField] private int _questionsCount = 5;

    [SerializeField,
     Tooltip("Inclusive range, Not significant as the design for this game states fixed bulbs count and placement.")]
    private int _lowerRange = 1;

    [SerializeField,
     Tooltip("Exclusive range, Not significant as the design for this game states fixed bulbs count and placement.")]
    private int _upperRange = 6;

    [SerializeField] private bool _isAddition = true;

    private int _corrAnsCount;
    private int _attemptedAnsCount;

    #endregion

    #region Unity Events

    [HideInInspector] public UnityEvent GM_OnGameStart = new();
    [HideInInspector] public UnityEvent GM_OnAnswerCorrect = new();
    [HideInInspector] public UnityEvent GM_OnAnswerIncorrect = new();
    [HideInInspector] public UnityEvent GM_OnGameOver = new();

    #endregion

    #region Helper Functions

    private Vector3 GetQuestion()
    {
        Vector3 dat = new();

        dat.x = Random.Range(_lowerRange, _upperRange);
        dat.y = Random.Range(_lowerRange, _upperRange);

        if (!_isAddition)
        {
            if (dat.x < dat.y)
            {
                float temp;
                temp = dat.x;
                dat.x = dat.y;
                dat.y = temp;
            }

            dat.z = dat.x - dat.y;
        }
        else
            dat.z = dat.x + dat.y;

        return dat;
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
        _uiManager = Calendar_UIManager.instance;

        GM_OnGameStart.AddListener(() =>
        {
            _uiManager.UIM_OnGameStart?.Invoke(_isAddition, _questionsCount);
            _uiManager.UIM_SetupNextQuestion?.Invoke(GetQuestion());
        });

        GM_OnAnswerCorrect.AddListener(() =>
        {
            _attemptedAnsCount++;
            _corrAnsCount++;

            _uiManager.UIM_UpdateUIForCorrectAnswer?.Invoke(_corrAnsCount, GetQuestion());

            if (_attemptedAnsCount < _questionsCount)
                //_uiManager.UIM_SetupNextQuestion?.Invoke(GetQuestion());
                return;
            else
            {
                _uiManager.UIM_GameOver?.Invoke(_corrAnsCount);
                GM_OnGameOver?.Invoke();
            }
        });

        GM_OnAnswerIncorrect.AddListener(() =>
        {
            _attemptedAnsCount++;

            _uiManager.UIM_UpdateUIForIncorrectAnswer?.Invoke(_corrAnsCount, GetQuestion());

            if (_attemptedAnsCount < _questionsCount)
                //_uiManager.UIM_SetupNextQuestion?.Invoke(GetQuestion());
                return;
            else
            {
                _uiManager.UIM_GameOver?.Invoke(_corrAnsCount);
                GM_OnGameOver?.Invoke();
            }
        });

        GM_OnGameOver.AddListener(() => Debug.Log("GameOver"));

        GM_OnGameStart?.Invoke();
    }
}