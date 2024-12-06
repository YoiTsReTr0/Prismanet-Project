using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MainGame.MatchingGame
{
    public class Matching_HandleDeselection : MonoBehaviour, IPointerClickHandler
    {
        private QuizUIManager _uiManager;

        private void Start()
        {
            _uiManager = QuizUIManager.instance;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("Out Click");
            if (!eventData.pointerCurrentRaycast.gameObject.CompareTag("MatchingOption"))
                _uiManager.UIM_OutsideClick?.Invoke();
        }
    }
}