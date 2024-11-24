using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance; // Singleton instance to access the score from anywhere

    public int Score { get; private set; } // Property to store the current score

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object persistent across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    // Method to add score
    public void AddScore(int points)
    {
        Score += points;
        Debug.Log("Score Updated: " + Score);
    }

    // Method to reset score
    public void ResetScore()
    {
        Score = 0;
    }
}
