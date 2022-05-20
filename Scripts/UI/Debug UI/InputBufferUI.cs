
using System;
using UnityEngine;

public class InputBufferUI : MonoBehaviour
{

#if UNITY_EDITOR
    void OnGUI()
    {

        InputBuffer inputBuffer = InputManager.Instance.GetInputBuffer();
        InputData inputData = InputManager.Instance.inputData;

        Debug.Log("Is Input buffer null? : " + (inputBuffer == null));

        if (inputBuffer != null)
        {
            int xSpace = 25;
            int ySpace = 15;
            //GUI.Label(new Rect(10, 10, 100, 20), "Hello World!");
            for (int i = 0; i < inputBuffer.buttonCommandCheck.Count; i++)
            {
                GUI.Label(new Rect(10f + (i * xSpace), 15f, 100, 20),
                    inputBuffer.buttonCommandCheck[i].ToString());
            }

            for (int b = 0; b < inputBuffer.buffer.Count; b++)
            {
                //GUI.Label(new Rect(xSpace - 10f, b * ySpace, 100, 20), b.ToString() + ":");
                for (int i = 0; i < inputBuffer.buffer[b].rawInputs.Count; i++)
                {
                    if (inputBuffer.buffer[b].rawInputs[i].used)
                    {
                        GUI.Label(new Rect(10f + (i * xSpace), 35f + (b * ySpace), 100, 20),
                            inputBuffer.buffer[b].rawInputs[i].hold.ToString("0") + ">");
                    }
                    else
                    {
                        GUI.Label(new Rect(10f + (i * xSpace), 35f + (b * ySpace), 100, 20),
                            inputBuffer.buffer[b].rawInputs[i].hold.ToString("0"));
                    }
                }
            }

            for (int m = 0; m < inputBuffer.motionCommandCheck.Count; m++)
            {
                GUI.Label(new Rect(500f - 25f, m * ySpace, 100, 20),
                    inputBuffer.motionCommandCheck[m].ToString());
                GUI.Label(new Rect(500f, m * ySpace, 100, 20), inputData.motionCommands[m].name);

            }

            // CHANGE THE CURRENT MOVE LIST CHARACTER INDEX IT IS CURRENTLY TEMPORARY AND SET TO DEFAULT
            //GUI.Label(new Rect(600f, 10f, 100, 20), CurrentMoveList(0).name.ToString());

        }
    }
#endif
}