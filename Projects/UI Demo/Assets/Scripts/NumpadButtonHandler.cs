using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NumpadButtonHandler : MonoBehaviour
{
    [SerializeField] ParticleSystem boomVfx;

    [SerializeField] Animator btnAnim;

    public void HandleButtonClick(int value)
    {
       
        GameManager.Instance.GetButtonInput(value);
        boomVfx.Play();
        btnAnim.SetTrigger("Pressed");
        GameManager.Instance.audioManager.PlayAudioClipByKey(SoundKey.ButtonClick);
    }
}
