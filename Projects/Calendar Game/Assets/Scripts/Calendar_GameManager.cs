using UnityEngine;
using UnityEngine.Events;

public class Calendar_GameManager : MonoBehaviour
{
    public static Calendar_GameManager instance;

    #region Local Variables

    private Calendar_UIManager _uiManager;

    private int _corrAnsCount;
    private int _attemptedAnsCount;
    private int _currLives;

    #endregion

    #region Editor Fields

    [SerializeField] private int QuestionsCount = 5;
    [SerializeField, Range(1, 5)] private int LivesPerQues = 3;

    #endregion

    #region Unity Events

    [HideInInspector] public UnityEvent GM_OnGameStart = new();
    [HideInInspector] public UnityEvent GM_OnAnswerCorrect = new();
    [HideInInspector] public UnityEvent GM_OnAnswerIncorrect = new();
    [HideInInspector] public UnityEvent GM_OnFullAnswerSetCorrect = new();
    [HideInInspector] public UnityEvent GM_OnFullAnswerSetIncorrect = new();
    [HideInInspector] public UnityEvent GM_OnGameOver = new();

    #endregion

    #region Helper Functions

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
            _uiManager.UIM_OnGameStart?.Invoke(QuestionsCount, LivesPerQues);
            _uiManager.UIM_SetupNextQuestion?.Invoke();
        });

        GM_OnAnswerCorrect.AddListener(() => { _uiManager.UIM_UpdateUIForCorrectAnswer?.Invoke(); });

        GM_OnAnswerIncorrect.AddListener(() =>
        {
            _currLives--;
            _uiManager.UIM_UpdateUIForIncorrectAnswer?.Invoke(_currLives);
        });

        GM_OnFullAnswerSetCorrect.AddListener(() =>
        {
            _attemptedAnsCount++;
            _corrAnsCount++;
            _currLives = LivesPerQues;

            bool continueGame = _attemptedAnsCount >= QuestionsCount ? false : true;
            _uiManager.UIM_UpdateUIForCorrectFullAnswerSet?.Invoke(_corrAnsCount, QuestionsCount, _currLives,
                continueGame);
        });
        GM_OnFullAnswerSetIncorrect.AddListener(() =>
        {
            _attemptedAnsCount++;
            _currLives = LivesPerQues;

            bool continueGame = _attemptedAnsCount >= QuestionsCount ? false : true;
            _uiManager.UIM_UpdateUIForIncorrectFullAnswerSet?.Invoke(_corrAnsCount, QuestionsCount, _currLives,
                continueGame);
        });

        GM_OnGameOver.AddListener(() => _uiManager.UIM_GameOver?.Invoke(_corrAnsCount, QuestionsCount));

        GM_OnGameStart?.Invoke();

        _currLives = LivesPerQues;
    }
}