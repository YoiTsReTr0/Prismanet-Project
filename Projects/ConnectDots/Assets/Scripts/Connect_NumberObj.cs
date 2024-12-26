using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MainGame.ConnectDots
{
    public class Connect_NumberObj : MonoBehaviour
    {
        public Button Button;
        public TextMeshProUGUI NumberText;
        public Image NumBGImage;
        public GameObject DoneImage;
        public GameObject IncorrectImage;

        private Tween IncorrectSelectionTween;

        public void HandleIncorrectSelection()
        {
            if (IncorrectSelectionTween != null && IncorrectSelectionTween.active)
            {
                IncorrectImage.SetActive(false);
                IncorrectSelectionTween.Kill();
            }

            IncorrectImage.SetActive(true);

            IncorrectSelectionTween = DOVirtual.DelayedCall(2, () => { IncorrectImage.SetActive(false); });
        }
    }
}