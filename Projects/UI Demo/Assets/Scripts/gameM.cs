using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameM : MonoBehaviour
{
    [System.Serializable]
    public class Level
    {
        public List<int> sequence;  // The number sequence, like 9, 8, 7, ?
        public int correctAnswer;   // Correct answer for the question mark
    }

    public List<Level> levels;  // Levels list that you can set manually in the inspector
    public int currentLevel = 0; // Track current level
    
    public Image[] sequenceImages; // UI Images for number sequence
    public Sprite[] numberSprites; // Array of sprites 0-9, index matches the number
    public Sprite questionMarkSprite;
    public Button[] numberButtons; // Number buttons

    // Popup GameObjects instead of Sprites
    public GameObject correctPopup; // GameObject for the correct popup
    public GameObject incorrectPopup; // GameObject for the incorrect popup
    public Slider progressionSlider; // Slider for progression

    void Start()
    {
        SetupLevel();
        SetupButtons();
    }

    void SetupLevel()
    {
        Level level = levels[currentLevel];

        // Set up the sequence, except the last "?" slot
        for (int i = 0; i < level.sequence.Count - 1; i++)
        {
            sequenceImages[i].sprite = numberSprites[level.sequence[i]];
        }

        // Set the last image to be the question mark
        sequenceImages[level.sequence.Count - 1].sprite = questionMarkSprite;

        // Update progression bar
        progressionSlider.value = (float)currentLevel / (float)levels.Count;
    }

    void SetupButtons()
    {
        foreach (Button btn in numberButtons)
        {
            btn.onClick.RemoveAllListeners(); // Remove any previous listeners
            int btnNumber = int.Parse(btn.GetComponentInChildren<Text>().text); // Get number from the button
            btn.onClick.AddListener(() => CheckAnswer(btnNumber));
        }
    }

    void CheckAnswer(int chosenAnswer)
    {
        Level level = levels[currentLevel];

        if (chosenAnswer == level.correctAnswer)
        {
            // Correct answer, show correct popup and update the question mark image
            sequenceImages[level.sequence.Count - 1].sprite = numberSprites[chosenAnswer];
            ShowPopup(true);

            // Move to the next level after a short delay
            Invoke("NextLevel", 1f);
        }
        else
        {
            // Incorrect answer, show incorrect popup
            ShowPopup(false);
        }
    }

    void ShowPopup(bool correct)
    {
        // Activate the appropriate popup
        if (correct)
        {
            correctPopup.SetActive(true);
        }
        else
        {
            incorrectPopup.SetActive(true);
        }

        // Hide the popup after 1 second
        Invoke("HidePopup", 1f);
    }

    void HidePopup()
    {
        // Deactivate both popups
        correctPopup.SetActive(false);
        incorrectPopup.SetActive(false);
    }

    void NextLevel()
    {
        currentLevel++;

        if (currentLevel < levels.Count)
        {
            SetupLevel();
        }
        else
        {
            // Game Completed, handle completion here
        }
    }
}
