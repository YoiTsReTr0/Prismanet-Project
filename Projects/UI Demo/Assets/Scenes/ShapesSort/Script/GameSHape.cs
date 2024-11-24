using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameShape : MonoBehaviour
{
    public Text scoreText;
    public Slider progressBar;
    public GameObject correctPopup;
    public GameObject incorrectPopup;
    public Button doneButton;
    public RectTransform shapeContainer;
    public List<GroupPanel> groupPanels;
    public List<GameObject> shapePrefabs;

    [Tooltip("Leave blank for default behavior (level + 1 shapes)")]
    public int customShapeCount = 0;

    private int score = 0;
    private int currentLevel = 1;
    private int totalLevels = 5;
    private List<GameObject> currentShapes = new List<GameObject>();

    private void Start()
    {
        UpdateUI();
        doneButton.onClick.AddListener(CheckLevel);
        LoadLevel(currentLevel);
    }

    public void CheckLevel()
    {
        bool allShapesCorrect = CheckAllShapesInCorrectGroups();
        
        if (allShapesCorrect)
        {
            ShowCorrectPopup();
            score += 100;
            currentLevel++;
            if (currentLevel <= totalLevels)
            {
                LoadLevel(currentLevel);
            }
            else
            {
                // Game completed
                Debug.Log("Game Completed!");
            }
        }
        else
        {
            ShowIncorrectPopup();
        }
        
        UpdateUI();
    }

    private bool CheckAllShapesInCorrectGroups()
    {
        foreach (var shape in currentShapes)
        {
            DraggableShape draggableShape = shape.GetComponent<DraggableShape>();
            if (!draggableShape.IsInCorrectGroup())
            {
                return false;
            }
        }
        return true;
    }

    private void LoadLevel(int level)
    {
        // Clear previous shapes
        foreach (var shape in currentShapes)
        {
            Destroy(shape);
        }
        currentShapes.Clear();

        // Determine number of shapes to spawn
        int shapeCount = (customShapeCount > 0) ? customShapeCount : level + 1;

        // Generate new shapes
        for (int i = 0; i < shapeCount; i++)
        {
            GameObject shapePrefab = shapePrefabs[Random.Range(0, shapePrefabs.Count)];
            GameObject shape = Instantiate(shapePrefab, shapeContainer);
            shape.GetComponent<RectTransform>().anchoredPosition = GetRandomPositionWithinContainer();
            currentShapes.Add(shape);
        }
    }

    private Vector2 GetRandomPositionWithinContainer()
    {
        Vector2 containerSize = shapeContainer.rect.size;
        float shapeSize = 50f; // Adjust this based on your shape size

        float minX = -containerSize.x / 2 + shapeSize / 2;
        float maxX = containerSize.x / 2 - shapeSize / 2;
        float minY = -containerSize.y / 2 + shapeSize / 2;
        float maxY = containerSize.y / 2 - shapeSize / 2;

        return new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
    }

    private void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        progressBar.value = (float)currentLevel / totalLevels;
    }

    private void ShowCorrectPopup()
    {
        correctPopup.SetActive(true);
        Invoke("HidePopups", 2f);
    }

    private void ShowIncorrectPopup()
    {
        incorrectPopup.SetActive(true);
        Invoke("HidePopups", 2f);
    }

    private void HidePopups()
    {
        correctPopup.SetActive(false);
        incorrectPopup.SetActive(false);
    }
}