using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NumberSequence : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI[] optionButtons;
    public GameObject correctPopup;
    public GameObject incorrectPopup;
    public Slider progressBar;
    public TextMeshProUGUI scoreText;

    [Header("Sprite Components")]
    public Image questionMarkImage; // Reference to the single question mark Image component
    public Sprite questionMarkSprite; // The default question mark sprite
    public Sprite[] digitSprites; // Array of sprites for digits 0-9
    public float digitSpacing = 30f; // Spacing between digit sprites
    public RectTransform numberPanel; // The panel to contain the number sprites

    [Header("Question Settings")]
    public string question;
    public int correctAnswerIndex;
    public float progressIncrement = 0.1f;
    public int pointsForCorrectAnswer = 10;

    private List<Image> numberImages = new List<Image>();

    private void Start()
    {
        progressBar.value = ProgressManager.Instance.Progress;
        SetQuestion();
        UpdateScoreUI();
        ResetQuestionMarkSprite();
    }

    void SetQuestion()
    {
        questionText.text = question;
    }

    void ResetQuestionMarkSprite()
    {
        questionMarkImage.sprite = questionMarkSprite;
        questionMarkImage.gameObject.SetActive(true);
        ClearNumberImages();
    }

    void ClearNumberImages()
    {
        foreach (var image in numberImages)
        {
            Destroy(image.gameObject);
        }
        numberImages.Clear();
    }

    public void OnOptionSelected(int index)
    {
        foreach (var button in optionButtons)
        {
            button.GetComponentInParent<Button>().interactable = false;
        }

        string selectedNumber = optionButtons[index].text;
        CreateNumberSprites(selectedNumber);

        if (index == correctAnswerIndex)
        {
            questionText.text = question.Replace("_", selectedNumber);
            Debug.Log("Correct Answer");

            correctPopup.SetActive(true);

            ProgressManager.Instance.Progress += progressIncrement;
            progressBar.value = ProgressManager.Instance.Progress;

            ScoreManager.Instance.AddScore(pointsForCorrectAnswer);
            UpdateScoreUI();

            Invoke("LoadNextScene", 2f);
        }
        else
        {
            Debug.Log("Incorrect Answer");

            incorrectPopup.SetActive(true);
            Invoke("LoadNextScene", 2f);
        }
    }

    void CreateNumberSprites(string number)
    {
        ClearNumberImages();
        questionMarkImage.gameObject.SetActive(false);

        float totalWidth = (number.Length - 1) * digitSpacing;
        float startX = -totalWidth / 2;

        for (int i = 0; i < number.Length; i++)
        {
            int digit = int.Parse(number[i].ToString());
            if (digit >= 0 && digit < digitSprites.Length)
            {
                GameObject digitObj = new GameObject($"Digit_{i}");
                Image digitImage = digitObj.AddComponent<Image>();
                digitImage.sprite = digitSprites[digit];
                digitImage.rectTransform.SetParent(numberPanel, false);
                digitImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                digitImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                digitImage.rectTransform.anchoredPosition = new Vector2(startX + i * digitSpacing, 0);
                numberImages.Add(digitImage);
            }
            else
            {
                Debug.LogWarning("Digit sprite not found for: " + digit);
            }
        }
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    void UpdateScoreUI()
    {
        scoreText.text = "Score: " + ScoreManager.Instance.Score;
    }
}