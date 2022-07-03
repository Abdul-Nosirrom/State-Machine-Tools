
using System;
using UnityEngine;

public class InputBufferUI : MonoBehaviour
{

#if UNITY_EDITOR
    void OnGUI()
    {

        InputBuffer inputBuffer = InputManager.Instance.GetInputBuffer();
        InputData inputData = InputManager.Instance.inputData;


        if (inputBuffer != null)
        {
            int xSpace = 25;
            int ySpace = 15;

            for (int i = 0; i < inputBuffer.buttonInputCurrentState.Count; i++)
            {
                GUI.Label(new Rect(10f + (i * xSpace), 15f, 100, 20),
                    inputBuffer.buttonInputCurrentState[i].ToString());
            }

            for (int b = 0; b < inputBuffer.buffer.Size; b++)
            {
                //GUI.Label(new Rect(xSpace - 10f, b * ySpace, 100, 20), b.ToString() + ":");
                for (int i = 0; i < inputBuffer.buffer[b].inputsFrameState.Count; i++)
                {
                    if (inputBuffer.buffer[b].inputsFrameState[i].used)
                    {
                        GUI.Label(new Rect(10f + (i * xSpace), 35f + (b * ySpace), 100, 20),
                            inputBuffer.buffer[b].inputsFrameState[i].hold.ToString("0") + ">");
                    }
                    else
                    {
                        GUI.Label(new Rect(10f + (i * xSpace), 35f + (b * ySpace), 100, 20),
                            inputBuffer.buffer[b].inputsFrameState[i].hold.ToString("0"));
                    }
                }
            }

            for (int m = 0; m < inputBuffer.motionInputCurrentState.Count; m++)
            {
                GUI.Label(new Rect(500f - 25f, m * ySpace, 100, 20),
                    inputBuffer.motionInputCurrentState[m].ToString());
                GUI.Label(new Rect(500f, m * ySpace, 100, 20), inputData.motionCommands[m].name);

            }
            
        }
    }
#endif
}