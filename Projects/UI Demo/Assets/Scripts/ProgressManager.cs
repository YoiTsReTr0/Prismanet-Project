using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }
    public float Progress { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // This will persist the object across scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }
}
