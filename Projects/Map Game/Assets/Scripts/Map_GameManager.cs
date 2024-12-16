using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Map_GameManager : MonoBehaviour
{
    public static Map_GameManager instance;

    #region Local Variables

    private Map_UIManager _uiManager;

    private int _corrAnsCount;

    private int _attemptedAnsCount;
    //private int _totalLives;
    //private int _currLives;

    private Stack<QuestionStateObjectSet> _quesStack = new();

    #endregion

    #region Editor Fields

    [SerializeField] private int QuesCount;
    //[SerializeField, Range(1, 5)] private int LivesPerQues = 3;

    [Space(35), SerializeField] private List<QuestionStateObjectSet> GameData;

    #endregion

    #region Unity Events

    [HideInInspector] public UnityEvent GM_OnGameStart = new();
    [HideInInspector] public UnityEvent GM_OnAnswerCorrect = new();
    [HideInInspector] public UnityEvent GM_OnAnswerIncorrect = new();
    [HideInInspector] public UnityEvent GM_OnGameOver = new();

    #endregion

    #region Helper Functions

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

    private QuestionStateObjectSet GetNewQuestion()
    {
        QuestionStateObjectSet data;

        _quesStack.TryPop(out data);

        return data;
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
        _uiManager = Map_UIManager.instance;

        GM_OnGameStart.AddListener(() =>
        {
            _uiManager.UIM_OnGameStart?.Invoke(QuesCount);
            _uiManager.UIM_SetupNextQuestion?.Invoke(GetNewQuestion(), _attemptedAnsCount);
        });

        GM_OnAnswerCorrect.AddListener(() =>
        {
            _attemptedAnsCount++;
            _corrAnsCount++;
            _uiManager.UIM_UpdateUIForCorrectAnswer?.Invoke(GetNewQuestion(), _attemptedAnsCount, _corrAnsCount);
        });

        GM_OnAnswerIncorrect.AddListener(() =>
        {
            _attemptedAnsCount++;
            _uiManager.UIM_UpdateUIForIncorrectAnswer?.Invoke(GetNewQuestion(), _attemptedAnsCount);
        });

        //_totalLives = LivesPerQues;
        //_currLives = _totalLives;

        GM_OnGameOver.AddListener(() => _uiManager.UIM_GameOver?.Invoke(_corrAnsCount, QuesCount));


        // Setup Questions Stack
        {
            List<QuestionStateObjectSet> list = new();

            list = GameData;
            ShuffleList(list);

            for (int i = 0; i < QuesCount; i++)
            {
                _quesStack.Push(list[i]);
            }
        }

        DOVirtual.DelayedCall(0.1f, () => { GM_OnGameStart?.Invoke(); });
    }
}