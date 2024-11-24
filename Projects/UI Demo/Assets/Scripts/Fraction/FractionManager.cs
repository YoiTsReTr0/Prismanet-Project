using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using static GameManager;
using TMPro;


public class FractionManager : MonoBehaviour
{

    #region [======== Variables =========]


    public AudioManager audioManager;

    public GameObject slashSprite;
    public GameObject equalsSprite;
    public GameObject questionMarkSprite;
    public Sprite[] numImages;

    public GameObject scorePanel;



    [SerializeField] private GameObject inputCanvas;
    [SerializeField] private GameObject outputCanvas;


    public int currentLength;
    public int correctAnswers = 0;


    public QuestionHandler questionHandler;

    private List<Questions> questions;


    [SerializeField] private List<Image> numeratorNumbers = new List<Image>();
    [SerializeField] private List<Image> denominatorNumbers = new List<Image>();
    [SerializeField] private List<Button> buttons;

    public RectTransform moveCloudDown;

    public ParticleSystem poof;

    [Header("Character Info")]
    public RectTransform moveCharacterUp;
    public TextMeshProUGUI speechBubbleText;
    public Sprite happySprite;
    public Sprite sadSprite;

    int index;
    bool isTouchEnabled = true;




    Vector2 cloudPos;
    Vector3 characterStartingPoisition;

    #endregion

    private void Start()
    {

        correctAnswers = 0;
        index = -1;
        characterStartingPoisition = moveCharacterUp.position;
        cloudPos = moveCloudDown.anchoredPosition;
        questions = questionHandler.questions;
        GenerateQuestion();

    }


    #region [======== Questions ============]

    private void GenerateOptions()
    {
        List<Sprite> options = questionHandler.questions[index].options;

        for(int i = 0; i < options.Count; i++)
        {
            buttons[i].image.sprite = options[i];

        }
    }

    private void GenerateQuestion()
    {
        index++;

        Questions ques = questions[index];
        GenerateOptions();
        //questions.RemoveAt(index);


        DisplayQuestion(ques);

    }

    private void DisplayQuestion(Questions _ques)
    {

        for (int i = 0; i < numeratorNumbers.Count; i++)
        {
            numeratorNumbers[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < denominatorNumbers.Count; i++)
        {
            denominatorNumbers[i].gameObject.SetActive(false);
        }

        string question = _ques.question + "=";

        bool isNumeratorNumberFinished = false;

        int imageIndex = 0;
        foreach (char c in question)
        {
            if (char.IsDigit(c))
            {
                int digit = int.Parse(c.ToString());

                Image tem = GetObjectFromPool(isNumeratorNumberFinished);
                tem.gameObject.transform.SetAsLastSibling();
                tem.sprite = numImages[digit];

                currentLength += 1;
            }
            else
            {
                GetSymbolSprite(c);
                currentLength += 1;
                isNumeratorNumberFinished = true;

            }
            imageIndex++;
        }

        questionMarkSprite.SetActive(true);

        //for (int i = 0; i < resultStringTemp.Length; i++)
        //{
        //    underScore[i].SetActive(true);
        //    underScore[i].gameObject.transform.SetAsLastSibling();
        //}
    }
    public void CheckAnswer(Image _image)
    {
        if (!isTouchEnabled)
            return;

        audioManager.PlayAudioClipByKey(SoundKey.ButtonClick);
        moveCharacterUp.GetComponent<Image>().sprite = sadSprite;
        isTouchEnabled = false;

        Questions question = questionHandler.questions[index];

        if (_image.sprite.name == question.correctAnswer.name)
        {
            correctAnswers++;
            moveCharacterUp.GetComponent<Image>().sprite = happySprite;
            audioManager.PlayAudioClipByKey(SoundKey.LevelWin);
            speechBubbleText.text = "Good Job!";
        }

        else
        {
            buttons.Find(x => x.image.sprite.name == question.correctAnswer.name).GetComponent<Animator>().SetTrigger("PlayBounce");
            audioManager.PlayAudioClipByKey(SoundKey.Error);
            moveCharacterUp.GetComponent<Image>().sprite = sadSprite;
            speechBubbleText.text = "Next Time";
            ShakeCamera();
        }


        moveCharacterUp.DOMoveY(-3, 1f).OnComplete(() => {

            speechBubbleText.transform.parent.gameObject.SetActive(true);
            LevelGratification(_image);

            isTouchEnabled = true;
        });

    }



    #endregion



    #region[ ======== Getters =========]

    private void GetSymbolSprite(char symbol)
    {
        switch (symbol)
        {
            case '/':
                slashSprite.transform.SetAsLastSibling();
                slashSprite.SetActive(true);
                break;
            case '=':
                equalsSprite.transform.SetAsLastSibling();
                equalsSprite.SetActive(true); ;
                break;
            default:
                break;
        }
    }

    public Image GetObjectFromPool(bool _isNumeratorFinished)
    {

        Image obj = _isNumeratorFinished ? denominatorNumbers.Find(o => !o.gameObject.activeSelf) : numeratorNumbers.Find(o => !o.gameObject.activeSelf);

        if (obj == null)
        {
            return null;
        }
        obj.gameObject.SetActive(true);

        return obj;
    }


    #endregion


    #region [ =========== Level Finish ========]
    public void LevelGratification(Image _image)
    {

        moveCloudDown.anchoredPosition = -cloudPos;

        float delayG = 0.6f;


        moveCharacterUp.DOMoveY(-3, 1f).OnComplete(() =>
        {
            moveCloudDown.anchoredPosition = cloudPos;

            moveCloudDown.DOAnchorPosY(-cloudPos.y, 3f);
            //delayG += 1f;

            if (_image.sprite.name == questionHandler.questions[index].correctAnswer.name)
                poof.Play();

            DOVirtual.DelayedCall(delayG, () =>
            {
                moveCharacterUp.position = characterStartingPoisition;
                speechBubbleText.transform.parent.gameObject.SetActive(false);
                questionMarkSprite.SetActive(false);
                slashSprite.SetActive(false);
                equalsSprite.SetActive(false);

                for (int i = 0; i < numeratorNumbers.Count; i++)
                {
                    numeratorNumbers[i].gameObject.SetActive(false);
                }

                for (int i = 0; i < denominatorNumbers.Count; i++)
                {
                    denominatorNumbers[i].gameObject.SetActive(false);
                }

                ChecKForLevelComplete();
            });
        });
        
    }

    private void ChecKForLevelComplete()
    {
        if (index == questionHandler.questions.Count - 1)
        {
            //TODO - Dispaly popup of play again
            inputCanvas.SetActive(false);
            outputCanvas.SetActive(false);
            scorePanel.SetActive(true);
        }
        else
            GenerateQuestion();
    }


    #endregion



    #region [ ========= Camera Shake =========]

    [Header("CameraShake")]
    [SerializeField] float shakeDuration = 0.5f;
    [SerializeField] float shakeStrength = 0.5f;
    [SerializeField] Ease shakeEase = Ease.OutQuad;
    [SerializeField] Camera mainC;
    Tween shake;



    public void ShakeCamera()
    {


        if (shake != null)
            shake.Kill(true);

        shake = mainC.transform.DOShakePosition(shakeDuration, shakeStrength).SetEase(shakeEase);
    }

    #endregion

}
