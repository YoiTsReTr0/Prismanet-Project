using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MainGame.TurnsAndPatterns
{
    public class RotateImgObj : MonoBehaviour
    {
        public Image RotateImage;
        public Image SelectedSurroundImage;
        public Button SelectionBtn;
        public ImageDirection CurrDirection;
        public ImageDirection FinalDirection;
    }
}