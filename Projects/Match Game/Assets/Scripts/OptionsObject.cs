using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class OptionsObject : MonoBehaviour
{
    public Button ObjButton;
    public Transform[] LineAttachTransforms = new Transform[2];
    public GameObject MatchedImage;
    public GameObject IncorrectMatchImage;

    private Tween IncorrectMatchTween;

    public void HandleIncorrectSelection()
    {
        if (IncorrectMatchTween != null && IncorrectMatchTween.active)
        {
            IncorrectMatchTween.Kill();
            IncorrectMatchImage.gameObject.SetActive(false);
        }

        IncorrectMatchImage.gameObject.SetActive(true);

        IncorrectMatchTween = DOVirtual.DelayedCall(2f, () => { IncorrectMatchImage.gameObject.SetActive(false); });
    }
}