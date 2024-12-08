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

    private float _averageAnimDurations = 2f;

    #endregion

    #region Editor Variables

    #region Header Area

    [Header("Header Area")] [SerializeField]
    private Image[] LivesImages;

    #endregion

    #region Main Game Area

    [Header("Main Game Area")] [SerializeField]
    private Hexagon_HexagonObject[] CircleObjects;

    [SerializeField] private Hexagon_HexagonObject[] SquareObjects;
    [Space(20), SerializeField] private GameObject CorrectResultPanel;

    [SerializeField] private GameObject IncorrectResultPanel;
    [SerializeField] private Button SubmitBtn;

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

    public UnityEvent<int, int, int> UIM_OnGameStart = new();

    public UnityEvent UIM_UpdateUIForCorrectAnswer = new();
    public UnityEvent<int> UIM_UpdateUIForIncorrectAnswer = new();
    public UnityEvent<int, int> UIM_GameOver = new();

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


        UIM_OnGameStart.AddListener((int livesCount, int lowerRange, int higherRange) =>
        {
            GameSetup(lowerRange, higherRange);

            for (int i = 0; i < livesCount; i++)
            {
                LivesImages[i].gameObject.SetActive(true);
            }
        });


        UIM_UpdateUIForCorrectAnswer.AddListener(() =>
        {
            CorrectResultPanel.SetActive(true);
            SubmitBtn.interactable = false;

            DOVirtual.DelayedCall(_averageAnimDurations * 0.9f, () =>
            {
                CorrectResultPanel.SetActive(false);
                _gameManager.GM_OnGameOver?.Invoke();
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
                    _gameManager.GM_OnGameOver?.Invoke();
            });
        });


        UIM_GameOver.AddListener((int score, int maxScore) =>
        {
            GameOverPanel.SetActive(true);
            for (int i = 0; i < GetStarsCount((float)score / maxScore); i++)
                AchievedStarsImages[i].color = AchievedStarColor;

            if (score <= 0)
                GameOverText.text = "Out Of Lives";
        });

        SubmitBtn.onClick.AddListener(CheckAnswer);
    }

    private void GameSetup(int lowerRange, int highRange)
    {
        GeneratePuzzle(lowerRange, highRange);

        return;

        void GeneratePuzzle(int lowerRange, int highRange)
        {
            int hiddenCount = 6;

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
            while (hideIndices.Count < hiddenCount)
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
        for (int i = 0; i < CircleObjects.Length; i++)
        {
            if (CircleObjects[i].PreDefinedNumText.text != CircleObjects[i].InputField.text)
            {
                _gameManager.GM_OnAnswerIncorrect?.Invoke();
                return;
            }


            if (SquareObjects[i].PreDefinedNumText.text != SquareObjects[i].InputField.text)
            {
                _gameManager.GM_OnAnswerIncorrect?.Invoke();
                return;
            }
        }

        _gameManager.GM_OnAnswerCorrect?.Invoke();
    }
}