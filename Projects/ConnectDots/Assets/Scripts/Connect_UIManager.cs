using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI.Extensions;
using Random = UnityEngine.Random;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine.UI;

namespace MainGame.ConnectDots
{
    public class Connect_UIManager : MonoBehaviour
    {
        public static Connect_UIManager instance;
        private Connect_GameManager _gameManager;

        #region Local Variables

        private Queue<UILineRenderer> _lineQueue = new(); // convert to UI line renderer
        private List<Connect_NumberObj> _numberObjList = new();
        private int _nextObjIndex = 0;

        private int _cartoonMoveCurrIndex;
        private float _cartoonMoveSpeed = 735;
        private Tween _cartoonMoveTween;

        private Coroutine _progressBarCoroutine;
        private float _averageAnimDurations = 2f;
        private int _currStars = 0;

        #endregion

        #region Editor Variables

        [SerializeField] private List<Sprite> NumbersSpriteList = new();

        [Header("Heading Area Vars")] [SerializeField]
        private TextMeshProUGUI HeadingCriteriaText;

        [SerializeField] private Image[] LivesImages;


        [Header("Main Game Area Vars")] [SerializeField]
        private Transform LinePoolParent;

        [SerializeField] private Slider ProgressBar;
        [SerializeField] private GameObject[] ProgressBarStars;
        [SerializeField] private GameObject LineAnimCartoon;

        [SerializeField, Space(20)] private List<Connect_PlacementObj> RandomPlacementList;

        [Header("Game Over Panel")] [SerializeField]
        private Image[] AchievedStarsImages;

        [SerializeField] private TextMeshProUGUI GameOverText;

        [Header("Misc Area Vars")] [SerializeField]
        private Connect_NumberObj NumberObjPrefab;

        [SerializeField] private Color AchievedStarColor;

        #endregion

        #region Unity Events

        [Header("Events")] public UnityEvent<string, List<int>,int> UIM_OnGameStart = new();
        [Header("Events")] public UnityEvent<int, int, bool> UIM_OnGameEnd = new();
        public UnityEvent<int, int> UIM_OnCorrectConnect = new();
        public UnityEvent<int> UIM_OnWrongSelection = new();

        #endregion

        #region Helper Functions

        /// <summary>
        /// Very efficient method to shuffle T
        /// </summary>
        /// <param name="list">List of type T</param>
        /// <typeparam name="T">Can be anything</typeparam>
        private void ShuffleList<T>(List<T> list)
        {
            // Fisher-Yates shuffle algorithm
            for (int i = list.Count - 1; i > 0; i--)
            {
                int rand = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[rand];
                list[rand] = temp;
            }
        }


        /// <summary>
        /// Very efficient method to shuffle T
        /// </summary>
        /// <param name="queue">Queue of type T</param>
        /// <typeparam name="T">Can be anything</typeparam>
        private void ShuffleQueue<T>(Queue<T> queue)
        {
            List<T> list = new();

            list = queue.ToList();
            queue.Clear();

            // Fisher-Yates shuffle algorithm
            for (int i = list.Count - 1; i > 0; i--)
            {
                int rand = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[rand];
                list[rand] = temp;
                queue.Enqueue(list[i]);
            }
        }

        private void RunGrowAndShrinkAnim(GameObject obj, Color newColor = default, bool useColor = false)
        {
            Vector3 OgSize1 = obj.transform.localScale;

            if (useColor)
                obj.GetComponent<Image>().color = newColor;

            obj.transform.DOScale(OgSize1 + Vector3.one, _averageAnimDurations / 16)
                .OnComplete(
                    () =>
                        obj.transform.DOScale(OgSize1, _averageAnimDurations / 16));
        }

        private int GetStarsCount(float quotient)
        {
            int stars = 0;

            if (quotient == 1)
                stars = 3;

            else if (quotient >= 0.6)
            {
                stars = 2;
            }

            else if (quotient >= 0.2)
            {
                stars = 1;
            }

            else if (quotient < 0.2)
                stars = 0;

            return stars;
        }

        #endregion

        private void Awake()
        {
            if (instance == null)
                instance = this;

            else
                Destroy(gameObject);
        }

        private void Start()
        {
            _gameManager = Connect_GameManager.instance;

            // spawn line objects in semi-pool
            UIM_OnGameStart.AddListener((string criteriaText, List<int> data, int LivesCount) =>
            {
                HeadingCriteriaText.text = criteriaText;
                SpawnLinePoolAndSetupNumObjs(data);

                for (int i = 0; i < LivesCount; i++)
                {
                    LivesImages[i].gameObject.SetActive(true);
                }
            });

            UIM_OnCorrectConnect.AddListener((int correctAns, int totalQues) =>
            {
                _progressBarCoroutine =
                    StartCoroutine(ProgressBarAnimIncrease(ProgressBar.value, (float)correctAns / totalQues));
            });

            UIM_OnGameEnd.AddListener((int correctAns, int totalQues, bool win) =>
            {
                for (int i = 0; i < GetStarsCount((float)correctAns / totalQues); i++)
                    AchievedStarsImages[i].color = AchievedStarColor;

                GameOverText.text = win ? "Well Done" : "Out Of Lives";
            });

            UIM_OnWrongSelection.AddListener((int count) => { LivesImages[count].color = Color.white; });


            _cartoonMoveSpeed *= _averageAnimDurations;
        }


        /// <summary>
        /// Initialization of the game
        /// </summary>
        /// <param name="data">List of numbers to be put inside the sequence</param>
        private void SpawnLinePoolAndSetupNumObjs(List<int> data)
        {
            // shuffle placement transforms
            {
                ShuffleList(RandomPlacementList);

                foreach (var item in RandomPlacementList)
                {
                    ShuffleQueue(item.AvailableChildren);
                }
            }

            // Main run area (when number is greater than 9 then additional images has to be added till the number. Or simply get the modular setup for the images and make new scriptings. Much better approach. UI provision limitations led to this approach)
            for (int i = 0; i < data.Count; i++)
            {
                SetupLineRenderer();

                Transform spawnParent = default;

                int listIndex = i;

                if (i >= RandomPlacementList.Count)
                    listIndex = i - RandomPlacementList.Count;

                spawnParent = RandomPlacementList[listIndex].AvailableChildren.Dequeue();

                Connect_NumberObj nObj = Instantiate(NumberObjPrefab, spawnParent, false);

                nObj.NumberText.text = data[i].ToString();
                nObj.NumberImage.sprite = NumbersSpriteList[data[i]];

                int helper = i;
                nObj.Button.onClick.AddListener(() => { SetupButtonClick(nObj, helper); });

                _numberObjList.Add(nObj);
            }

            return;

            // Create the pool of UI Line Renderers for faster usage, single run creates 1 obj in pool
            void SetupLineRenderer()
            {
                GameObject lObj = new GameObject("LineObject");
                lObj.transform.SetParent(LinePoolParent);

                UILineRenderer line = lObj.AddComponent<UILineRenderer>();
                line.LineThickness = 7.35f;

                lObj.SetActive(false);

                _lineQueue.Enqueue(line);
            }

            // Func to connect line between 2 points. Handling dynamically.
            void ConnectLineRenderer()
            {
                UILineRenderer lObj = _lineQueue.Dequeue();

                lObj.gameObject.SetActive(true);

                // Setup transforms
                lObj.Points.Add(_numberObjList[_nextObjIndex - 1].transform.position);
                lObj.Points.Add(_numberObjList[_nextObjIndex].transform.position);
            }

            // Main setup of what happens on clicking number
            void SetupButtonClick(Connect_NumberObj obj, int currIndex)
            {
                if (currIndex == _nextObjIndex)
                {
                    if (currIndex != 0)
                        ConnectLineRenderer();

                    // if 1st button then spawn cartoon at its position
                    else
                        LineAnimCartoon.transform.position = obj.transform.position;

                    _gameManager.GM_OnCorrectConnect?.Invoke();
                    _nextObjIndex++;

                    obj.Button.interactable = false;
                    obj.Button.onClick.RemoveAllListeners();

                    obj.DoneImage.SetActive(true);

                    StartLineCartoonAnim();
                }

                else
                {
                    _gameManager.GM_OnIncorrectConnect?.Invoke();
                }
            }
        }


        /// <summary>
        /// Manage Line cartoon movement using DOTween (anim control can be added here)
        /// </summary>
        private void StartLineCartoonAnim()
        {
            if (_cartoonMoveTween == null)
            {
                _cartoonMoveTween = LineAnimCartoon.transform.DOMove(
                    _numberObjList[_cartoonMoveCurrIndex].transform.position,
                    _cartoonMoveSpeed).SetSpeedBased(true).OnComplete(() =>
                {
                    _cartoonMoveTween = null;
                    _cartoonMoveCurrIndex++;
                    if (_cartoonMoveCurrIndex < _nextObjIndex)
                    {
                        StartLineCartoonAnim();
                    }

                    else if (_cartoonMoveCurrIndex >= _numberObjList.Count)
                        _gameManager.GM_OnGameEnd?.Invoke(true);
                });
            }

            else return;
        }

        private IEnumerator ProgressBarAnimIncrease(float initialVal, float finalVal)
        {
            void ClaimProgressBarStar(int starNo)
            {
                RunGrowAndShrinkAnim(ProgressBarStars[starNo - 1], Color.yellow, true);
                RunGrowAndShrinkAnim(ProgressBar.handleRect.gameObject);
            }


            float elapsedTime = 0f;

            while (elapsedTime < _averageAnimDurations)
            {
                elapsedTime += Time.deltaTime;

                ProgressBar.value = Mathf.Lerp(initialVal, finalVal, elapsedTime / _averageAnimDurations);

                if (_currStars < GetStarsCount(ProgressBar.value))
                {
                    _currStars++;
                    ClaimProgressBarStar(_currStars);
                }

                yield return new WaitForEndOfFrame();
            }


            yield return null;
        }
    }
}