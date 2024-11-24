using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AdditionGame : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI questionText; // Reference to the question text
    public TextMeshProUGUI[] optionButtons; // References to the answer option buttons
    public GameObject correctPopup; // Reference to the correct answer popup panel
    public GameObject incorrectPopup; // Reference to the incorrect answer popup panel
    public Slider progressBar; // Reference to the progress bar
    public TextMeshProUGUI scoreText; // UI element to display score

    [Header("Question Settings")]
    public string question; // The question to display
    public int correctAnswerIndex; // Index of the correct answer
    public float progressIncrement = 0.1f; // Amount to increment the progress bar on a correct answer
    public int pointsForCorrectAnswer = 10; // Points awarded for a correct answer

    private void Start()
    {
        progressBar.value = ProgressManager.Instance.Progress;
        SetQuestion();
        UpdateScoreUI(); // Display initial score
    }

    void SetQuestion()
    {
        // Set the question text
        questionText.text = question;
    }

    public void OnOptionSelected(int index)
    {
        // Disable all option buttons to prevent multiple selections
        foreach (var button in optionButtons)
        {
            button.GetComponentInParent<Button>().interactable = false;
        }

        if (index == correctAnswerIndex)
        {
            questionText.text = question.Replace("_", optionButtons[index].text);
            Debug.Log("Correct Answer");

            correctPopup.SetActive(true);

            // Update the persistent progress value and the slider
            ProgressManager.Instance.Progress += progressIncrement;
            progressBar.value = ProgressManager.Instance.Progress;

            // Add points for the correct answer
            ScoreManager.Instance.AddScore(pointsForCorrectAnswer);
            UpdateScoreUI(); // Update score display

            Invoke("LoadNextScene", 2f);
        }
        else
        {
            Debug.Log("Incorrect Answer");

            incorrectPopup.SetActive(true);
            Invoke("LoadNextScene", 2f);
        }
    }

    void LoadNextScene()
    {
        // Assuming you're using build index order to load the next scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    void UpdateScoreUI()
    {
        scoreText.text = "Score: " + ScoreManager.Instance.Score; // Display the updated score
    }
}
