using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    [System.Serializable]
    public class Question
    {
        public int number1;
        public int number2;
        public int result;
    }

    public enum Difficulty
    {
        Easy,
        Mid,
        Hard
    }

    public enum Operation
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        None,
    }

    [Header("Game Settings")]
    public int totalLevels = 10; // Set the number of levels (questions)
    public List<Question> manualQuestions = new List<Question>(); // List for manually added questions
    public bool useManualQuestions = false; // Toggle to use manual questions

    [Header("UI Elements")]
    public Slider progressSlider;
    public Text levelText;
    public Text scoreText;

    public int total;
    public Difficulty currentDifficulty = Difficulty.Easy;
    public Operation currentOperation = Operation.Addition;

    public GameObject plusSprite;
    public GameObject equalsSprite;
    public Sprite[] numImages;

    private int number1;
    private int number2;
    private string userInput = "";
    private int result;

    public int currentLength;
    public int correctAnswers = 0;

    public Color[] bgColor;
    public Image bgRef;

    public bool isGamePaused;

    public RectTransform moveCloudDown;
    public ParticleSystem poof;

    public AudioManager audioManager;

    public GameObject[] underScore;

    Vector2 cloudPos;

    public GridLayoutGroup gridLayoutGroup;
    public Vector2 gridSize;
    public Vector2 defaultSize;

    public List<GameObject> resultString = new List<GameObject>();

    private int currentLevel = 0;
    private int correctAnswersInLevel = 0;

    void Start()
    {
        defaultSize = gridLayoutGroup.cellSize;
        gridSize.x = defaultSize.x;
        gridSize.y = defaultSize.y;
        correctAnswers = 0;
        InitializeUI();
        GenerateQuestion();
      
        
    }
      private void InitializeUI()
    {
        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = totalLevels;
            progressSlider.value = 0;
        }
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Backspace) || Input.GetKeyUp(KeyCode.Escape))
        {
            ClearStuff();
        }

        if (Input.GetKeyUp(KeyCode.KeypadEnter) || Input.GetKeyUp(KeyCode.Return))
        {
            SubmitAnswer();
        }
    }

    [SerializeField] private List<Image> objectPool = new List<Image>();
    public Image GetObjectFromPool()
    {
        Image obj = objectPool.Find(o => !o.gameObject.activeSelf);
        if (obj == null)
        {
            return null;
        }
        obj.gameObject.SetActive(true);
        return obj;
    }

    private void SetRandomBGColor()
    {
        if (bgColor.Length > 0)
        {
            bgRef.color = bgColor[Random.Range(0, bgColor.Length)];
        }
    }

private void SelectRandomDifficulty()
    {
        List<Difficulty> possibleDifficulties = new List<Difficulty>();

        if (total > 5 && total <= 10)
        {
            possibleDifficulties.Add(Difficulty.Easy);
            possibleDifficulties.Add(Difficulty.Mid);
        }
        else if (total > 10)
        {
            possibleDifficulties.Add(Difficulty.Easy);
            possibleDifficulties.Add(Difficulty.Mid);
            possibleDifficulties.Add(Difficulty.Hard);
        }
        else
        {
            possibleDifficulties.Add(Difficulty.Easy);
        }

        int randomIndex = Random.Range(0, possibleDifficulties.Count);
        currentDifficulty = possibleDifficulties[randomIndex];
    }
    

    public void GenerateQuestion()
    {
           if (currentLevel >= totalLevels)
        {
            EndGame();
            return;
        }
        isGamePaused = false;
        currentLength = 0;
        gridSize.x = defaultSize.x;
        gridSize.y = defaultSize.y;
        SetCellSize(gridSize);

        ClearStuff();
       foreach (var lineU in underScore)
        {
            lineU.SetActive(false);
        }

        if (useManualQuestions && manualQuestions.Count > currentLevel)
        {
            SetManualQuestion(manualQuestions[currentLevel]);
        }
        else
        {
            AdditionLogic();
        }

        DisplayQuestion();
        UpdateUI();
    }

      private void SetManualQuestion(Question question)
    {
        number1 = question.number1;
        number2 = question.number2;
        result = question.result;
        SetRandomBGColor();
    }

    private void AdditionLogic()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                number1 = Random.Range(1, 100);
                number2 = Random.Range(1, 100);
                break;
            case Difficulty.Mid:
                number1 = Random.Range(10, 1000);
                number2 = Random.Range(10, 1000);
                break;
            case Difficulty.Hard:
                number1 = Random.Range(100, 1000);
                number2 = Random.Range(100, 1000);
                break;
        }

        result = number1 + number2;
        SetRandomBGColor();
    }

    void DisplayQuestion()
    {
        for (int i = 0; i < objectPool.Count; i++)
        {
            objectPool[i].gameObject.SetActive(false);
        }

        string question = number1.ToString() + "+" + number2.ToString() + "=";

        foreach (char c in question)
        {
            if (char.IsDigit(c))
            {
                int digit = int.Parse(c.ToString());

                Image tem = GetObjectFromPool();
                tem.gameObject.transform.SetAsLastSibling();
                tem.sprite = numImages[digit];

                currentLength += 1;
            }
            else
            {
                GetSymbolSprite(c);
                currentLength += 1;
            }
        }

        string resultStringTemp = result.ToString();
        for (int i = 0; i < resultStringTemp.Length; i++)
        {
            if (i < underScore.Length)
            {
                underScore[i].SetActive(true);
                underScore[i].gameObject.transform.SetAsLastSibling();
            }
            else
            {
                Debug.LogWarning("Not enough underScore objects for the result. Please increase the underScore array size.");
            }
        }
    }

    private void GetSymbolSprite(char symbol)
    {
        switch (symbol)
        {
            case '+':
                plusSprite.transform.SetAsLastSibling();
                plusSprite.SetActive(true);
                break;
            case '=':
                equalsSprite.transform.SetAsLastSibling();
                equalsSprite.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void SetCellSize(Vector2 newSize)
    {
        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.cellSize = newSize;
        }
    }

    public void GetButtonInput(int keyValue)
    {
        currentLength += 1;
        string resultStringTemp = result.ToString();

        foreach (var lineU in underScore)
        {
            lineU.SetActive(false);
        }

        Image tem = GetObjectFromPool();
        tem.gameObject.transform.SetAsLastSibling();
        tem.sprite = numImages[keyValue];

        resultString.Add(tem.gameObject);
        userInput += keyValue.ToString();

        // Adjust grid size for larger numbers
        if (currentLength > 12)
        {
            gridSize.y -= 5;
            gridSize.x -= 5;
            SetCellSize(gridSize);
        }

        for (int i = 0; i < resultStringTemp.Length - userInput.Length; i++)
        {
            if (i < underScore.Length)
            {
                underScore[i].SetActive(true);
                underScore[i].gameObject.transform.SetAsLastSibling();
            }
        }

        if (userInput.Length >= resultStringTemp.Length)
        {
            DOVirtual.DelayedCall(0.3f, () => { SubmitAnswer(); });
        }
    }

    public void ClearStuff()
    {
        userInput = "";
        gridSize.x = defaultSize.x;
        gridSize.y = defaultSize.y;
        SetCellSize(gridSize);
        foreach (var img in resultString)
        {
            img.SetActive(false);
        }

        for (int i = 0; i < underScore.Length; i++)
        {
            underScore[i].SetActive(false);
        }
        resultString.Clear();
        isGamePaused = false;
        string resultStringTemp = result.ToString();
        for (int i = 0; i < resultStringTemp.Length; i++)
        {
            if (i < underScore.Length)
            {
                underScore[i].SetActive(true);
                underScore[i].gameObject.transform.SetAsLastSibling();
            }
        }
    }

    public void SubmitAnswer()
    {
        if (result.ToString() == userInput)
        {
            audioManager.PlayAudioClipByKey(SoundKey.LevelWin);
            correctAnswers++;
            LevelWinGratification();
            foreach (var lineU in underScore)
            {
                lineU.SetActive(false);
            }
        }
        else
        {
            audioManager.PlayAudioClipByKey(SoundKey.Error);
            ClearStuff();
            ShakeCamera();
        }
        currentLength = 0;
        UpdateUI();
    }


    bool isFlip;
 public void LevelWinGratification()
    {
        for (int i = 0; i < objectPool.Count; i++)
        {
            objectPool[i].gameObject.SetActive(false);
        }
        plusSprite.SetActive(false);
        equalsSprite.SetActive(false);

        ClearStuff();
        moveCloudDown.anchoredPosition = -cloudPos;

        float delayG = 2f;
        moveCloudDown.DOAnchorPosY(cloudPos.y, 2f);

        poof.Play();
        
        DOVirtual.DelayedCall(delayG, () => {
            currentLevel++;
            SelectRandomDifficulty();
            GenerateQuestion();
        });
    }

      private void EndGame()
    {
        // Implement end game logic here
        Debug.Log("Game Over! Final Score: " + correctAnswers);
        // You might want to show a game over screen, restart option, etc.
    }

        public void AddManualQuestion(int num1, int num2)
    {
        Question newQuestion = new Question
        {
            number1 = num1,
            number2 = num2,
            result = num1 + num2
        };
        manualQuestions.Add(newQuestion);
    }
     private void UpdateUI()
    {
        // Update any UI elements that show the current level, score, etc.
        // For example:
        // levelText.text = "Level: " + total;
        // scoreText.text = "Score: " + correctAnswers;
        if (levelText != null)
            levelText.text = "Level: " + (currentLevel + 1) + " / " + totalLevels;
        
        if (scoreText != null)
            scoreText.text = "Score: " + correctAnswers;
        
        if (progressSlider != null)
            progressSlider.value = currentLevel;
    }
    [Header("CameraShake")]
    [SerializeField] float shakeDuration = 0.5f;
    [SerializeField] float shakeStrength = 0.5f;
    [SerializeField] Ease shakeEase = Ease.OutQuad;
    [SerializeField] Camera mainC;
    Tween shake;

    public void ShakeCamera()
    {
        if (shake != null)
            shake.Kill(true);

        shake = mainC.transform.DOShakePosition(shakeDuration, shakeStrength).SetEase(shakeEase);
    }
}