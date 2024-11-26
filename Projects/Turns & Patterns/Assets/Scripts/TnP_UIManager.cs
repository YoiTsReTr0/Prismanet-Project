using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace MainGame.TurnsAndPatterns
{
    public class TnP_UIManager : MonoBehaviour
    {
        public static TnP_UIManager instance;

        #region Local Variables

        private TnP_GameManager _gameManager;

        private PatternDataSO _gameData;

        private List<RotateImgObj> _rotateImgObjList = new();
        private RotateImgObj _currSelObj;

        #endregion

        #region Editor Variables

        [SerializeField] private float AvgAnimTime = 2;

        [Header("Gameplay Area")] [SerializeField]
        private GameObject ObjSpawnParent;

        [SerializeField] private PresetObj PresetObj;
        [SerializeField] private RotateImgObj RotateImgObj;

        [Space(35)] [SerializeField] private GameObject CorrectResultPanel;
        [SerializeField] private GameObject IncorrectResultPanel;

        [Header("Game Over Area")] [SerializeField]
        private GameObject GameOverPanel;

        #endregion

        #region Unity Events

        [Space(35), Header("Events")] public UnityEvent<PatternDataSO> UIM_OnGameStart = new();

        public UnityEvent UIM_UpdateUIForCorrectAnswer = new();
        public UnityEvent UIM_UpdateUIForIncorrectAnswer = new();
        public UnityEvent UIM_GameOver = new();

        #endregion

        #region Pre-Requisites

        private ImageDirection NumToImgDirEnum(int no)
        {
            int currNo = no % 8;

            return (ImageDirection)currNo;
        }

        private float ImgDirEnumToRotation(ImageDirection dir)
        {
            float zRotation = 0;

            switch (dir)
            {
                case ImageDirection.North:
                    zRotation = 0;
                    break;
                case ImageDirection.NorthEast:
                    zRotation = 45;
                    break;
                case ImageDirection.East:
                    zRotation = 90;
                    break;
                case ImageDirection.SouthEast:
                    zRotation = 135;
                    break;
                case ImageDirection.South:
                    zRotation = 180;
                    break;
                case ImageDirection.SouthWest:
                    zRotation = 225;
                    break;
                case ImageDirection.West:
                    zRotation = 270;
                    break;
                case ImageDirection.NorthWest:
                    zRotation = 315;
                    break;
            }

            return zRotation;
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
            _gameManager = TnP_GameManager.instance;

            UIM_OnGameStart.AddListener((PatternDataSO data) =>
            {
                _gameData = data;
                SetupGame();
            });

            UIM_UpdateUIForCorrectAnswer.AddListener(() =>
            {
                CorrectResultPanel.SetActive(true);
                DOVirtual.DelayedCall(AvgAnimTime, () =>
                {
                    CorrectResultPanel.SetActive(false);
                    _gameManager.GM_OnGameOver?.Invoke();
                });
            });

            UIM_UpdateUIForIncorrectAnswer.AddListener(() =>
            {
                IncorrectResultPanel.SetActive(true);
                DOVirtual.DelayedCall(AvgAnimTime, () => { IncorrectResultPanel.SetActive(false); });
            });

            UIM_GameOver.AddListener(() => { GameOverPanel.SetActive(true); });
        }

        private void SetupGame()
        {
            for (int i = 0; i < ObjSpawnParent.transform.childCount; i++)
            {
                Destroy(ObjSpawnParent.transform.GetChild(i).gameObject);
            }

            int posNo = 0;
            for (int i = 0; i < _gameData.TotalBoxesCount; i++)
            {
                if (i == _gameData.PositionsList[posNo])
                {
                    PresetObj obj = Instantiate(PresetObj, ObjSpawnParent.transform, false);

                    obj.RotateImage.sprite = _gameData.RotateableSprite;
                    obj.Direction = NumToImgDirEnum(i);
                    obj.RotateImage.transform.rotation = Quaternion.Euler(0, 0, ImgDirEnumToRotation(obj.Direction));

                    posNo++;
                    posNo = Mathf.Clamp(posNo, 0, _gameData.PositionsList.Count - 1);
                }

                else
                {
                    RotateImgObj obj = Instantiate(RotateImgObj, ObjSpawnParent.transform, false);
                    _rotateImgObjList.Add(obj);

                    obj.RotateImage.sprite = _gameData.RotateableSprite;
                    obj.CurrDirection =
                        ImageDirection.North; // Always North in the beginning, controls will control new directions
                    obj.FinalDirection = NumToImgDirEnum(i);

                    obj.SelectionBtn.onClick.AddListener(() => { SetupBtnClickToSelect(obj); });
                }
            }

            return;

            void SetupBtnClickToSelect(RotateImgObj obj)
            {
                foreach (var v in _rotateImgObjList)
                {
                    v.SelectedSurroundImage.gameObject.SetActive(false);
                    v.SelectionBtn.interactable = true;
                }

                obj.SelectedSurroundImage.gameObject.SetActive(true);
                obj.SelectionBtn.interactable = false;

                _currSelObj = obj;
            }
        }

        public void RotateObj(int zRot)
        {
            if (_currSelObj == null)
            {
                Debug.Log("None Selected");
                return;
            }

            _currSelObj.RotateImage.transform.rotation =
                Quaternion.Euler(0, 0, _currSelObj.RotateImage.transform.rotation.eulerAngles.z + zRot);

            int rotNo = (zRot / 45 + (int)_currSelObj.CurrDirection) >= 0
                ? (zRot / 45 + (int)_currSelObj.CurrDirection)
                : 8 + (zRot / 45 + (int)_currSelObj.CurrDirection);

            _currSelObj.CurrDirection = (ImageDirection)rotNo;
        }

        public void CheckFinalAnswer()
        {
            foreach (var v in _rotateImgObjList)
            {
                v.SelectedSurroundImage.gameObject.SetActive(false);
                v.SelectionBtn.interactable = false;
            }

            foreach (var vObj in _rotateImgObjList)
            {
                if (vObj.CurrDirection != vObj.FinalDirection)
                {
                    _gameManager.GM_OnAnswerIncorrect?.Invoke();
                    _currSelObj = null;


                    DOVirtual.DelayedCall(AvgAnimTime, () =>
                    {
                        foreach (var v in _rotateImgObjList)
                        {
                            v.SelectionBtn.interactable = true;
                        }
                    });

                    return;
                }
            }

            _gameManager.GM_OnAnswerCorrect?.Invoke();
        }
    }
}