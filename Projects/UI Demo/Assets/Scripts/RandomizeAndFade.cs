using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RandomizeAndFadeUI : MonoBehaviour
{
    public float minScale = 0f;
    public float maxScale = 1.5f;
    public float fadeDuration = 1f;
    public float moveDuration = 1f;
    public float delayBetweenActions = 0.5f;

    private RectTransform rectTransform;
    private Canvas canvas;
    private Image image;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        image = GetComponent<Image>();

        if (rectTransform == null || canvas == null || image == null)
        {
            Debug.LogError("RectTransform, Canvas, or Image component missing.");
            return;
        }

        // Start the loop
        StartRandomizing();
    }

    void StartRandomizing()
    {
        // Sequence for the actions
        Sequence sequence = DOTween.Sequence();

        sequence.AppendCallback(ChangePosition)
                .Append(rectTransform.DOScale(Vector3.one * Random.Range(minScale, maxScale), moveDuration))
                .AppendInterval(delayBetweenActions)
                .Append(image.DOFade(0, fadeDuration))
                .AppendCallback(() => gameObject.SetActive(false))
                .AppendInterval(delayBetweenActions)
                .AppendCallback(() => 
                {
                    gameObject.SetActive(true);
                    image.DOFade(1, 0); // Reset the alpha to 1 immediately
                })
                .SetLoops(-1, LoopType.Restart);
    }

    void ChangePosition()
    {
        Vector2 randomPosition = GetRandomPositionWithinCanvas();
        rectTransform.anchoredPosition = randomPosition;
    }

    Vector2 GetRandomPositionWithinCanvas()
    {
        RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();

        float canvasWidth = canvasRectTransform.rect.width;
        float canvasHeight = canvasRectTransform.rect.height;

        float randomX = Random.Range(0, canvasWidth) - canvasWidth / 2;
        float randomY = Random.Range(0, canvasHeight) - canvasHeight / 2;

        return new Vector2(randomX, randomY);
    }
}
