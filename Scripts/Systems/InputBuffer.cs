using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utilities;



public class InputBuffer
{
    // The rows of our input buffer, each containing a column corresponding to each input type
    // Each row is an InputBufferFrame, corresponding to the state of each input at the buffer 
    // frame (i) [the Queue position]
    public CircularBuffer<InputBufferFrame> buffer;
    public static int bufferSize = 10;    // Corresponds to how big our buffer is (units of fixed dT)

    /// <summary>
    /// Hold the frame of each input in which it can be used, -1 corresponds to no input that can be used,
    /// so either no input was registered, or it's been held for a while that it can't be used
    /// </summary>
    public List<int> buttonInputCurrentState;
    /// <summary>
    /// Hold the frame of each motion command which it can be used
    /// </summary>
    public List<int> motionInputCurrentState;
    
    void InitializeBuffer()
    {
        buffer = new CircularBuffer<InputBufferFrame>(bufferSize);

        for (int i = 0; i < bufferSize; i++)
        {
            InputBufferFrame newFrame = new InputBufferFrame();
            newFrame.InitializeFrame();
            buffer.PushBack(newFrame);
        }
        
        buttonInputCurrentState = new List<int>();
        for (int i = 0; i < InputManager.Instance.inputData.rawInputs.Count; i++)
        {
            buttonInputCurrentState.Add(-1);
        }

        motionInputCurrentState = new List<int>();
        for (int i = 0; i < InputManager.Instance.inputData.motionCommands.Count; i++)
        {
            motionInputCurrentState.Add(-1);
        }
    }


    public void UpdateBuffer()
    {
        if (buffer == null || buttonInputCurrentState.Count != InputManager.Instance.inputData.rawInputs.Count || motionInputCurrentState.Count != InputManager.Instance.inputData.motionCommands.Count) 
            InitializeBuffer();
        if (bufferSize != buffer.Size) InitializeBuffer();
        
        // Each frame, push new list of inputs and their states to the front
        InputBufferFrame newFrame = new InputBufferFrame();
        newFrame.InitializeFrame();
        newFrame.CopyFrameState(buffer.Front());
        newFrame.UpdateFrameState();
        
        // Setup newframe to match old frame before updating
        buffer.PushFront(newFrame);
        
        // Now update values of current frame in the currentState lists
        for (int b = 0; b < buttonInputCurrentState.Count; b++)
        {
            buttonInputCurrentState[b] = -1;
            for (int f = 0; f < bufferSize; f++)
            {
                // Set buttonState to the frame it can be executed
                if (buffer[f].inputsFrameState[b].CanExecute()) buttonInputCurrentState[b] = f;
            }
            // Check for NONE input type, set it to always be valid (greater than -1)
            if (InputManager.Instance.inputData.rawInputs[b].inputType == RawInput.InputType.IGNORE)
                buttonInputCurrentState[b] = 0;
        }

        for (int m = 0; m < motionInputCurrentState.Count; m++)
        {
            motionInputCurrentState[m] = -1;
            InputManager.Instance.inputData.motionCommands[m].checkStep = 0;
            InputManager.Instance.inputData.motionCommands[m].curAngle = 0;
            for (int f = 0; f < bufferSize; f++)
            {
                // Below is with the assumption that axis input is inputs 0 and 1 in the InputData, not a very good approach
                Vector2 axisFrame = new Vector2(buffer[f].inputsFrameState[0].value, buffer[f].inputsFrameState[1].value);
                //Vector2 axisFrame = InputManager.Instance.GetNormalizedStickInput();
                if (InputManager.Instance.inputData.motionCommands[m].CheckMotionDirection(axisFrame))
                {
                    motionInputCurrentState[m] = f;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// If we want to use an input, and we can use it, then use it and set it to used
    /// </summary>
    /// <param name="inputIndex"></param>
    public void UseInput(int inputIndex)
    {
        buffer[buttonInputCurrentState[inputIndex]].inputsFrameState[inputIndex].used = true;
        buttonInputCurrentState[inputIndex] = -1;
    }
}

#region Frame Data - A Row Of States Corresponding To One Input

/// <summary>
/// A row of our input buffer containing the state of each input at the frame in the buffer
/// </summary>
public class InputBufferFrame
{
    public List<InputBufferFrameState> inputsFrameState;

    /// <summary>
    /// Setup buffer row with input data
    /// </summary>
    public void InitializeFrame()
    {
        inputsFrameState = new List<InputBufferFrameState>();
        for (int i = 0; i < InputManager.Instance.inputData.rawInputs.Count; i++)
        {
            InputBufferFrameState newFS = new InputBufferFrameState
            {
                rawInput = i
            };
            inputsFrameState.Add(newFS);
        }
    }

    /// <summary>
    /// Update the state of an input in this current frame, is it held, what value does the associated input have
    /// (0,1) for button - axis value for axis, and so on
    /// </summary>
    public void UpdateFrameState()
    {
        if (inputsFrameState == null || inputsFrameState.Count != InputManager.Instance.inputData.rawInputs.Count)
        {
            InitializeFrame();
        }
        
        foreach(InputBufferFrameState frameState in inputsFrameState)
        {
            frameState.ResolveCommand();
        }
        
    }

    public void CopyFrameState(InputBufferFrame frontFrame)
    {
        for (int i = 0; i < inputsFrameState.Count; i++)
        {
            inputsFrameState[i].hold = frontFrame.inputsFrameState[i].hold;
            inputsFrameState[i].value = frontFrame.inputsFrameState[i].value;
            inputsFrameState[i].used = frontFrame.inputsFrameState[i].used;
        }
    }
    
}

/// <summary>
/// Class corresponding to the state of a given input in a specific frame
/// </summary>
public class InputBufferFrameState
{
    /// <summary>
    /// Corresponds to input index in InputData object
    /// </summary>
    public int rawInput;
    
    /// <summary>
    /// Value of the input. If it's just a button, it's zero or one depending on whether it is pressed.
    /// If it's an axis, then it has the value along that axis/how far it is pushed
    /// </summary>
    public float value;
    
    /// <summary>
    /// How many frames has this input been held at the current buffer frame
    /// </summary>
    public int hold;    // How many frames has this input been held at the given frame
    
    /// <summary>
    /// Has the input been already used up earlier in the buffer
    /// </summary>
    public bool used;   

    /// <summary>
    /// Updates the state of the input in the current frame. If it's pressed or released update accordingly
    /// the values above.
    /// </summary>
    public void ResolveCommand()
    {
        used = false;
        switch (InputManager.Instance.inputData.rawInputs[rawInput].inputType)
        {
            case RawInput.InputType.BUTTON:
                if (InputManager.Instance.rawButtonContainer[InputManager.Instance.inputData.rawInputs[rawInput].name])
                {
                    HoldUp(1f);
                }
                else
                {
                    ReleaseHold();
                }
                break;
            // Here we assume we have an X and Y in InputData, expectation is that its decomposed to each axis and labeled
            // as somethingX and somethingY - with the name of the axis input being something
            case RawInput.InputType.AXIS:
                string axisName = InputManager.Instance.inputData.rawInputs[rawInput].name;
                Vector2 axisInput = InputManager.Instance.rawAxisContainer[axisName.Remove(axisName.Length - 1, 1)];
                float val = axisName[^1] == 'X' ? axisInput.x : axisInput.y;

                if (val != 0)
                {
                    HoldUp(val);
                }
                else
                {
                    ReleaseHold();
                }
                break;
        }
    }

    /// <summary>
    /// Called when the input is being held.
    /// </summary>
    /// <param name="_val"></param>
    public void HoldUp(float _val)
    {
        value = _val;

        if (hold < 0) { hold = 1; }
        else { hold += 1; }

    }

    /// <summary>
    /// Call when an input has been released, or is not registered
    /// </summary>
    public void ReleaseHold()
    {
        if (hold > 0) { hold = -1; used = false; }
        else { hold = 0; }
        value = 0;
        
        // Below could also be another way to test if an input is released from a hold
        //GameEngine.gameEngine.playerInputBuffer.buttonCommandCheck[rawInput] = 0;
    }

    /// <summary>
    /// Is an input registered in the current frame that has not been used up previously?
    /// </summary>
    /// <returns></returns>
    public bool CanExecute()
    {
        if (hold == 1 && !used) { return true; }
        return false;
    }

    
    public bool MotionNeutral()
    {
        if(Mathf.Abs(value) < InputManager.Instance.deadZone) { return true; }
        return false;
    }
}

#endregion

// The input data stored in the InputData scriptable object
#region Defining Input Types

/// <summary>
/// Defining the various inputs and their type. Name corresponds to the Action in the input system map
/// </summary>
[System.Serializable]
public class RawInput
{
    public enum InputType { BUTTON, AXIS, IGNORE }
    public InputType inputType;
    public string name;
}


/// <summary>
/// Defines the various motion direction commands. Checked against the "movement" axis
/// </summary>
[System.Serializable]
public class MotionCommand
{
    public string name;
    public List<MotionCommandDirection> commands;

    public int checkStep;

    public int angleChange;

    public float prevAngle;
    public float curAngle;

    public bool CheckMotionDirection(Vector2 axisInput)
    {
        if (angleChange > 0)
        {
            GetAxisDirection(axisInput);
            if (curAngle >= angleChange) { return true; }
        }
        else
        {
            if (commands == null) { return true; }

            if (checkStep >= commands.Count) { return true; }
            if (commands[checkStep] == GetAxisDirection(axisInput)) { checkStep++; }
        }
        return false;
    }

    public enum MotionCommandDirection
    {
        NEUTRAL, FORWARD, BACK, SIDE, ANGLE_CHANGE
    }

    /// <summary>
    /// Could probably not handle any cameras here, and do any pre-processing relative to the camera in
    /// input manager
    /// </summary>
    /// <param name="axisInput"></param>
    /// <returns></returns>
    public MotionCommandDirection GetAxisDirection(Vector2 axisInput)
    {
        if (Mathf.Abs(axisInput.x) < InputManager.Instance.deadZone && Mathf.Abs(axisInput.y) < InputManager.Instance.deadZone)
            return MotionCommandDirection.NEUTRAL;
        
        Vector3 charForward = DataManager.Instance.mainCharacter.characterObject.transform.forward;
        // This is already processed if we get it directly from the character controller
        Vector3 stickInput = DataManager.Instance.mainCharacter.characterController.moveInputVec;
        
        
        
        charForward.y = 0; charForward.Normalize();
        stickInput.y = 0; stickInput.Normalize();

        float angle = Vector2.Angle(new Vector2(charForward.x, charForward.z), new Vector2(stickInput.x, stickInput.z));
        
        if (angle < 45) return MotionCommandDirection.FORWARD;
        if (angle < 135) return MotionCommandDirection.SIDE;
        return MotionCommandDirection.BACK;
        
    }
}

#endregion


