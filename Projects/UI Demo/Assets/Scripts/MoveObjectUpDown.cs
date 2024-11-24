using UnityEngine;
using DG.Tweening;

public class MoveObjectUpDown : MonoBehaviour
{
    public float moveDistance = 1f;  // The distance to move up and down
    public float moveDuration = 1f;  // The duration of one up or down movement

    void Start()
    {
        MoveUpAndDown();
    }

    void MoveUpAndDown()
    {
        Sequence sequence = DOTween.Sequence();

        // Move the object up
        sequence.Append(transform.DOMoveY(transform.position.y + moveDistance, moveDuration).SetEase(Ease.InOutSine));
        // Move the object down
        sequence.Append(transform.DOMoveY(transform.position.y - moveDistance, moveDuration).SetEase(Ease.InOutSine));

        // Loop the sequence indefinitely
        sequence.SetLoops(-1, LoopType.Yoyo);
    }
}
