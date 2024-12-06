using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MainGame.MatchingGame
{
    [CreateAssetMenu(fileName = "NewMatchGameData", menuName = "Create Minigame Data/Match Game")]
    public class MatchingGameDataSO : ScriptableObject
    {
        [Tooltip("Difficulty of the minigame, for visual purposes. No background effects are present (as of now)"),
         Range(1, 5)]
        public int LivesCount = 3;

        [Tooltip(
            "Option Images in sequential order for left side. Caution: Right options list should also have the same order")]
        public List<Sprite> LeftOptionImages;

        [Tooltip(
            "Option Images in sequential order for right side. Caution: Left options list should also have the same order")]
        public List<Sprite> RightOptionImages;

        [Tooltip(
            "Coins reward count per correct answer. Also may depend on the difficulty so adjust values accordingly")]
        public int RewardCoinsPerAnswerCount;
    }
}