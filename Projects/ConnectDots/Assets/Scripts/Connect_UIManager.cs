using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Random = UnityEngine.Random;

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

        #endregion

        #region Editor Variables

        [Header("Heading Area Vars")] [SerializeField]
        private TextMeshProUGUI HeadingCriteriaText;

        [Header("Main Game Area Vars")] [SerializeField]
        private Transform LinePoolParent;

        [SerializeField] private GameObject LineAnimCartoon;

        [SerializeField, Space(20)] private List<Connect_PlacementObj> RandomPlacementList;

        [Header("Misc Area Vars")] [SerializeField]
        private Connect_NumberObj NumberObjPrefab;

        #endregion

        #region Unity Events

        [Header("Events")] public UnityEvent<string, List<int>> UIM_OnGameStart = new();
        [Header("Events")] public UnityEvent UIM_OnGameEnd = new();
        public UnityEvent UIM_OnCorrectConnect = new();

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
            UIM_OnGameStart.AddListener((string criteriaText, List<int> data) =>
            {
                HeadingCriteriaText.text = criteriaText;
                SpawnLinePoolAndSetupNumObjs(data);
            });

            UIM_OnCorrectConnect.AddListener(() =>
            {
                // Connect Lines
            });
        }

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

            void SetupLineRenderer()
            {
                GameObject lObj = new GameObject("LineObject");
                lObj.transform.SetParent(LinePoolParent);

                UILineRenderer line = lObj.AddComponent<UILineRenderer>();
                line.LineThickness = 7.35f;

                lObj.SetActive(false);

                _lineQueue.Enqueue(line);
            }

            void ConnectLineRenderer()
            {
                UILineRenderer lObj = _lineQueue.Dequeue();

                lObj.gameObject.SetActive(true);

                // Setup transforms
                lObj.Points.Add(_numberObjList[_nextObjIndex - 1].transform.position);
                lObj.Points.Add(_numberObjList[_nextObjIndex].transform.position);
            }

            void SetupButtonClick(Connect_NumberObj obj, int currIndex)
            {
                if (currIndex == _nextObjIndex)
                {
                    if (currIndex != 0)
                        ConnectLineRenderer();

                    _gameManager.GM_OnCorrectConnect?.Invoke();
                    _nextObjIndex++;

                    obj.Button.interactable = false;
                    obj.Button.onClick.RemoveAllListeners();

                    obj.DoneImage.SetActive(true);
                }

                // add OnIncorrect.
            }

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

                int helper = i;
                nObj.Button.onClick.AddListener(() =>
                {
                    // anim added
                    SetupButtonClick(nObj, helper);
                });

                _numberObjList.Add(nObj);
            }
        }
    }
}