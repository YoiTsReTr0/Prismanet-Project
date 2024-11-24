using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumpadKeyboardHandler : MonoBehaviour
{
    public NumpadButtonHandler[] buttonHandlers; 

    void Update()
    {
        for (int i = 0; i < 10; )
        {
            if (Input.GetKeyUp(KeyCode.Alpha0 + i))
            {
                if(buttonHandlers[i]!= null)
                buttonHandlers[i].HandleButtonClick(i);
            }
            i++;
        }
           CheckForKeyBoardKey();
    }


    private void CheckForKeyBoardKey()
    {
         if (Input.GetKeyUp(KeyCode.Keypad0) &&  buttonHandlers[0]!= null)
             buttonHandlers[0].HandleButtonClick(0);
        else if (Input.GetKeyUp(KeyCode.Keypad1) && buttonHandlers[1]!= null)
            buttonHandlers[1].HandleButtonClick(1);
        else if (Input.GetKeyUp(KeyCode.Keypad2)&& buttonHandlers[2]!= null)
            buttonHandlers[2].HandleButtonClick(2);
        else if (Input.GetKeyUp(KeyCode.Keypad3)&& buttonHandlers[3]!= null)
            buttonHandlers[3].HandleButtonClick(3);
        else if (Input.GetKeyUp(KeyCode.Keypad4)&& buttonHandlers[4]!= null)
            buttonHandlers[4].HandleButtonClick(4);
        else if (Input.GetKeyUp(KeyCode.Keypad5)&& buttonHandlers[5]!= null)
            buttonHandlers[5].HandleButtonClick(5);
        else if (Input.GetKeyUp(KeyCode.Keypad6)&& buttonHandlers[6]!= null)
            buttonHandlers[6].HandleButtonClick(6);
        else if (Input.GetKeyUp(KeyCode.Keypad7)&& buttonHandlers[7]!= null)
            buttonHandlers[7].HandleButtonClick(7);
        else if (Input.GetKeyUp(KeyCode.Keypad8)&& buttonHandlers[8]!= null)
            buttonHandlers[8].HandleButtonClick(8);
        else if (Input.GetKeyUp(KeyCode.Keypad9)&& buttonHandlers[9]!= null)
            buttonHandlers[9].HandleButtonClick(9);
    }
}
