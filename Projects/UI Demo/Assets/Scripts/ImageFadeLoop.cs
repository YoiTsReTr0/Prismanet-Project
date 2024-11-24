using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class ImageFadeLoop : MonoBehaviour
{
    public Image imageToFade;

    public bool shouldShake;
    public bool shouldFade;

    public float strength;
    public int vibrato;


    void Start()
    {
        if (imageToFade != null)
        {
            StartCoroutine(FadeLoop());
        }
        else
        {
            Debug.LogError("No Image component found.");
        }
    }

    IEnumerator FadeLoop()
    {
        while (true)
        {
            float fadeOutDuration = Random.Range(8f, 15f);
            if (shouldShake)
                imageToFade.transform.DOShakePosition(fadeOutDuration+5,strength,vibrato);

            if (shouldFade)
                imageToFade.DOFade(0f, fadeOutDuration);
            yield return new WaitForSeconds(fadeOutDuration);

            float fadeInDuration = Random.Range(8f, 15f);

            if (shouldShake)
                imageToFade.transform.DOShakePosition(fadeOutDuration+5,strength,vibrato);

            if (shouldFade)
                imageToFade.DOFade(Random.Range(0.2f, 0.5f), fadeInDuration);
            yield return new WaitForSeconds(fadeOutDuration);
        }
    }
}
