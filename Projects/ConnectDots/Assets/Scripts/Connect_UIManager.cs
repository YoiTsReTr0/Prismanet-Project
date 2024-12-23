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
using UnityEngine.Serialization;
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

        [Header("Heading Area Vars")] [SerializeField]
        private TextMeshProUGUI QuesCountText;

        [SerializeField] private Image[] LivesImages;


        [Header("Main Game Area Vars")] [SerializeField]
        private Transform LinePoolParent;

        [SerializeField] private Slider ProgressBar;
        [SerializeField] private GameObject[] ProgressBarStars;
        [SerializeField] private GameObject LineAnimCartoon;

        [SerializeField] private GameObject GameProtectorScreen;

        [Space(20), SerializeField] private GameObject AnswerSetResultPanel;
        [SerializeField] private TextMeshProUGUI AnswerSetResultPanelText;


        [SerializeField, Space(20)] private List<Connect_PlacementObj> RandomPlacementList;

        [Header("Game Over Panel")] [SerializeField]
        private Image[] AchievedStarsImages;

        [SerializeField] private GameObject GameOverPanel;
        [SerializeField] private TextMeshProUGUI GameOverText;

        [Header("Misc Area Vars")] [SerializeField]
        private Connect_NumberObj NumberObjPrefab;

        [SerializeField] private Color AchievedStarColor;

        [SerializeField] private List<Color> NumBGColorList = new();

        #endregion

        #region Unity Events

        [Header("Events")] public UnityEvent<int> UIM_OnGameStart = new();
        public UnityEvent<Connect_DataSO, int, int> UIM_SetupNextQuestion = new();

        public UnityEvent UIM_UpdateUIForCorrectAnswer = new();
        public UnityEvent<int> UIM_UpdateUIForIncorrectAnswer = new();
        public UnityEvent<Vector3, bool> UIM_UpdateUIForCorrectFullAnswerSet = new();
        public UnityEvent<int, int, bool> UIM_UpdateUIForIncorrectFullAnswerSet = new();
        public UnityEvent<int, int> UIM_GameOver = new();

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

        private void ReSetupLineRendererQueue()
        {
            // Re setup queue
            _lineQueue.Clear();

            var lines = LinePoolParent.GetComponentsInChildren<UILineRenderer>(true);
            foreach (var line in lines)
            {
                _lineQueue.Enqueue(line);
            }
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

            UIM_OnGameStart.AddListener(
                (int quesCount) =>
                {
                    QuesCountText.text = $"Ques: 0/{quesCount}";

                    for (int i = 0; i < 20; i++)
                    {
                        SetupLineRenderer();
                    }
                });

            UIM_SetupNextQuestion.AddListener((Connect_DataSO gameData, int attemptedQues, int quesCount) =>
            {
                SetupNumObjs(gameData.AnswersList);

                QuesCountText.text = $"Ques: {attemptedQues + 1}/{quesCount}";

                for (int i = 0; i < LivesImages.Length; i++)
                {
                    LivesImages[i].gameObject.SetActive(false);

                    if (i < gameData.LivesCount)
                    {
                        LivesImages[i].gameObject.SetActive(true);

                        if (ColorUtility.TryParseHtmlString("#DA3D3D", out Color newColor))
                        {
                            LivesImages[i].color = newColor;
                        }
                    }
                }
            });

            UIM_UpdateUIForCorrectAnswer.AddListener(() => { _gameManager.GM_OnFullAnswerSetCorrect?.Invoke(); });

            UIM_UpdateUIForIncorrectAnswer.AddListener((int count) =>
            {
                LivesImages[count].color = Color.white;

                if (count <= 0)
                    _gameManager.GM_OnFullAnswerSetIncorrect?.Invoke();
            });


            UIM_UpdateUIForCorrectFullAnswerSet.AddListener((Vector3 data, bool continueGame) =>
            {
                ReSetupLineRendererQueue();

                GameProtectorScreen.SetActive(true);

                _progressBarCoroutine =
                    StartCoroutine(ProgressBarAnimIncrease(ProgressBar.value,
                        (float)data.x / data.z));

                DOVirtual.DelayedCall(_averageAnimDurations * 0.9f, () =>
                {
                    AnswerSetResultPanel.SetActive(true);
                    AnswerSetResultPanelText.text = "Perfection!!";

                    DOVirtual.DelayedCall(_averageAnimDurations * 1.35f, () =>
                    {
                        AnswerSetResultPanel.SetActive(false);

                        if (!continueGame)
                            _gameManager.GM_OnGameOver?.Invoke();

                        else
                        {
                            GameProtectorScreen.SetActive(false);
                            _gameManager.GM_SetupNewQuestion?.Invoke();
                        }
                    });
                });
            });


            UIM_UpdateUIForIncorrectFullAnswerSet.AddListener(
                (int attemptedAnsCount, int questionsCount, bool continueGame) =>
                {
                    ReSetupLineRendererQueue();

                    GameProtectorScreen.SetActive(true);

                    AnswerSetResultPanel.SetActive(true);
                    AnswerSetResultPanelText.text = "Out Of Lives";

                    DOVirtual.DelayedCall(_averageAnimDurations * 1.35f, () =>
                    {
                        AnswerSetResultPanel.SetActive(false);

                        if (continueGame)
                        {
                            GameProtectorScreen.SetActive(false);

                            _gameManager.GM_SetupNewQuestion?.Invoke();
                        }


                        else
                            _gameManager.GM_OnGameOver?.Invoke();
                    });
                });


            UIM_GameOver.AddListener((int score, int maxScore) =>
            {
                GameOverPanel.SetActive(true);

                int finalStars = GetStarsCount((float)score / maxScore);

                if (finalStars == 0)
                    GameOverText.text = "Try Harder Next Time";


                for (int i = 0; i < finalStars; i++)
                    AchievedStarsImages[i].color = AchievedStarColor;
            });


            _cartoonMoveSpeed *= _averageAnimDurations;
        }


        /// <summary>
        /// Initialization of the game
        /// </summary>
        /// <param name="data">List of numbers to be put inside the sequence</param>
        private void SetupNumObjs(List<int> data)
        {
            LineAnimCartoon.transform.position = new(35000, 16000, 19000);
            _nextObjIndex = 0;
            _cartoonMoveCurrIndex = 0;
            _numberObjList.Clear();

            for (int i = 0; i < LinePoolParent.childCount; i++)
            {
                LinePoolParent.GetChild(i).gameObject.SetActive(false);
            }

            // shuffle placement transforms
            {
                ShuffleList(RandomPlacementList);

                foreach (var item in RandomPlacementList)
                {
                    item.ResetChildrenAndDespawn();
                    ShuffleQueue(item.AvailableChildren);
                }
            }


            for (int i = 0; i < data.Count; i++)
            {
                Transform spawnParent = default;

                int listIndex = i;

                //ONLY SUPPORTS 2x the count of RandomPlacementList 
                if (i >= RandomPlacementList.Count)
                    listIndex = i - RandomPlacementList.Count;

                spawnParent = RandomPlacementList[listIndex].AvailableChildren.Dequeue();

                Connect_NumberObj nObj = Instantiate(NumberObjPrefab, spawnParent, false);

                nObj.NumberText.text = data[i].ToString();
                nObj.NumBGImage.color = NumBGColorList[Random.Range(0, NumBGColorList.Count)];

                int helper = i;
                nObj.Button.onClick.AddListener(() => { SetupButtonClick(nObj, helper); });

                _numberObjList.Add(nObj);
            }

            return;


            // Func to connect line between 2 points. Handling dynamically.
            void ConnectLineRenderer()
            {
                UILineRenderer lObj = _lineQueue.Dequeue();

                lObj.gameObject.SetActive(true);
                lObj.Points.Clear();

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

                    //_gameManager.GM_OnCorrectConnect?.Invoke();
                    _nextObjIndex++;

                    obj.Button.interactable = false;
                    obj.Button.onClick.RemoveAllListeners();

                    obj.DoneImage.SetActive(true);

                    StartLineCartoonAnim();
                }

                else
                {
                    _gameManager.GM_OnAnswerIncorrect?.Invoke();
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
                        _gameManager.GM_OnAnswerCorrect?.Invoke();
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

        /// <summary>
        /// Create the pool of UI Line Renderers for faster usage, single run creates 1 obj in pool
        /// </summary>
        private void SetupLineRenderer()
        {
            GameObject lObj = new GameObject("LineObject");
            lObj.transform.SetParent(LinePoolParent);

            UILineRenderer line = lObj.AddComponent<UILineRenderer>();
            line.LineThickness = 7.35f;

            lObj.SetActive(false);

            _lineQueue.Enqueue(line);
        }
    }
}