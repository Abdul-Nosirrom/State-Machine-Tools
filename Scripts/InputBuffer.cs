using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InputBuffer
{
    public List<InputBufferFrame> buffer;// = new List<InputBufferFrame>();
    public static int bufferWindow = 25;

    public List<int> buttonCommandCheck;
    public List<int> motionCommandCheck;

    void InitializeBuffer()
    {
        buffer = new List<InputBufferFrame>();
        for (int i = 0; i < bufferWindow; i++)
        {
            InputBufferFrame newB = new InputBufferFrame();
            newB.InitializeFrame();
            buffer.Add(newB);
        }

        buttonCommandCheck = new List<int>();
        for (int i = 0; i < EngineData.actionData.rawInputs.Count; i++)
        {
            buttonCommandCheck.Add(-1);
        }

        motionCommandCheck = new List<int>();
        for (int i = 0; i < EngineData.actionData.motionCommands.Count; i++)
        {
            motionCommandCheck.Add(-1);
        }
    }

    public void Update()
    {
        EngineData.engineData.playerInputBuffer = this;
        if (buffer == null) { InitializeBuffer(); }
        if (buffer.Count < EngineData.actionData.rawInputs.Count || buffer.Count == 0)
        {
            InitializeBuffer();
        }



        for (int i = 0; i < buffer.Count - 1; i++)
        {
            for (int r = 0; r < buffer[i].rawInputs.Count; r++)
            {
                buffer[i].rawInputs[r].value = buffer[i + 1].rawInputs[r].value;
                buffer[i].rawInputs[r].hold = buffer[i + 1].rawInputs[r].hold;
                buffer[i].rawInputs[r].used = buffer[i + 1].rawInputs[r].used;
            }
        }
        buffer[buffer.Count - 1].Update();

        for (int r = 0; r < buttonCommandCheck.Count; r++)
        {
            buttonCommandCheck[r] = -1;
            for (int b = 0; b < buffer.Count; b++)
            {
                if (buffer[b].rawInputs[r].CanExecute()) { buttonCommandCheck[r] = b; }
            }
            if (EngineData.actionData.rawInputs[r].inputType == RawInput.InputType.IGNORE) { buttonCommandCheck[r] = 0; }

        }

        for (int m = 0; m < motionCommandCheck.Count; m++)
        {
            motionCommandCheck[m] = -1;
            EngineData.actionData.motionCommands[m].checkStep = 0;
            EngineData.actionData.motionCommands[m].curAngle = 0;
            for (int b = 0; b < buffer.Count; b++)
            {
                //if (UpdateMotionCheck(m, b)) { GameEngine.coreData.motionCommands[m].checkStep++; }
                //if (GameEngine.coreData.motionCommands[m].TestCheck(MotionCommand.GetNumPadDirection(buffer[b].rawInputs[4].value, buffer[b].rawInputs[5].value)))
                if (EngineData.actionData.motionCommands[m].TestCheck(buffer[b].rawInputs[0].value, buffer[b].rawInputs[1].value))
                { motionCommandCheck[m] = b; break; }
            }
        }

    }

    

    

    public void UseInput(int _i)
    {
        buffer[buttonCommandCheck[_i]].rawInputs[_i].used = true;
        //Debug.Log("USED UP!!!> : " + buttonCommandCheck[_i].ToString());
        buttonCommandCheck[_i] = -1;
        //buffer[buttonCommandCheck[_i]].rawInputs[_i].hold = -2;

    }
}

public class MotionChecker
{
    bool ready;
    List<bool> buffer;
}

public class InputBufferFrame
{
    public List<InputBufferFrameState> rawInputs;

    public void InitializeFrame()
    {
        rawInputs = new List<InputBufferFrameState>();
        for (int i = 0; i < EngineData.actionData.rawInputs.Count; i++)
        {
            InputBufferFrameState newFS = new InputBufferFrameState();
            newFS.rawInput = i;
            rawInputs.Add(newFS);
        }
    }

    public void Update()
    {
        if (rawInputs == null) { InitializeFrame(); }
        if(rawInputs.Count == 0 || rawInputs.Count != EngineData.actionData.rawInputs.Count) { InitializeFrame(); }
        foreach(InputBufferFrameState fs in rawInputs)
        {
            fs.ResolveCommand();
        }
    }

}

public class InputBufferFrameState
{
    public int rawInput;
    public float value;
    public int hold;
    public bool used;

    public void ResolveCommand()
    {
        used = false;
        switch (EngineData.actionData.rawInputs[rawInput].inputType)
        {
            case RawInput.InputType.BUTTON:
                if (Input.GetButton(EngineData.actionData.rawInputs[rawInput].name))
                {
                    HoldUp(1f);
                }
                else
                {
                    ReleaseHold();
                }
                break;
            case RawInput.InputType.AXIS:
                if (Mathf.Abs(Input.GetAxisRaw(EngineData.actionData.rawInputs[rawInput].name)) > EngineData.engineData.deadZone)
                {
                    HoldUp(Input.GetAxisRaw(EngineData.actionData.rawInputs[rawInput].name));
                }
                else
                { ReleaseHold(); }
                break;
        }
    }

    public void HoldUp(float _val)
    {
        value = _val;

        if (hold < 0) { hold = 1; }
        else { hold += 1; }

    }

    public void ReleaseHold()
    {
        if (hold > 0) { hold = -1; used = false; }
        else { hold = 0; }
        value = 0;
        //GameEngine.gameEngine.playerInputBuffer.buttonCommandCheck[rawInput] = 0;
    }

    public bool CanExecute()
    {
        if (hold == 1 && !used) { return true; }
        return false;
    }

    public bool MotionNeutral()
    {
        if(Mathf.Abs(value) < EngineData.engineData.deadZone) { return true; }
        return false;
    }
}


[System.Serializable]
public class RawInput
{
    public enum InputType { BUTTON, AXIS, DOUBLE_AXIS, DIRECTION, IGNORE }
    public InputType inputType;
    public string name;
}



[System.Serializable]
public class MotionCommand
{
    public string name;
    public int motionWindow;
    public int confirmWindow;
    //[IndexedItem(IndexedItemAttribute.IndexedItemType.MOTION_COMMAND_STEP)]
    public List<MotionCommandDirection> commands;
    public bool clean;
    public bool anyOrder;

    public int checkStep;

    //public bool absoluteDirection;
    public int angleChange;

    public float prevAngle;
    public float curAngle;

    public bool TestCheck(float _x, float _y)//(MotionCommandDirection _dir)
    {
        if (angleChange > 0)
        {
            GetNumPadDirection(_x, _y);
            if (curAngle >= angleChange) { return true; }
        }
        else
        {
            if (commands == null) { return true; }

            if (checkStep >= commands.Count) { return true; }
            if (commands[checkStep] == GetNumPadDirection(_x, _y)) { checkStep++; }
            //if (commands[checkStep] == _dir) { checkStep++; }
        }
        return false;
    }

    public enum MotionCommandDirection
    {
        NEUTRAL, FORWARD, BACK, SIDE, ANGLE_CHANGE
    }

    public MotionCommandDirection GetNumPadDirection(float _x, float _y)
    {
        //if (Mathf.Abs(_x) > GameEngine.gameEngine.deadZone || Mathf.Abs(_y) > GameEngine.gameEngine.deadZone) { return 8; }
        //else { return 5; }
        //return 5;

        if (Mathf.Abs(_x) > EngineData.engineData.deadZone || Mathf.Abs(_y) > EngineData.engineData.deadZone)
        {

            Vector3 charForward = EngineData.engineData.mainCharacter.character.transform.forward;
            Vector3 stickForward = new Vector3();// = buffer[buffer.Count - 1].stick;
            Vector3 camForward = Camera.main.transform.forward;



            camForward.y = 0;
            camForward.Normalize();
            stickForward += camForward * _y;

            stickForward += Camera.main.transform.right * _x;
            stickForward.y = 0;
            stickForward.Normalize();


            float _stickAngle = Vector2.Angle(new Vector2(charForward.x, charForward.z), new Vector2(stickForward.x, stickForward.z));

            if (angleChange > 0)
            {

                //if (Mathf.Abs(_stickAngle) > angleChange / commands.Count) { return MotionCommandDirection.ANGLE_CHANGE; }
                // curr
                _stickAngle = Vector2.Angle(new Vector2(0f, 1f), new Vector2(stickForward.x, stickForward.z));
                float angleDiff = Mathf.Abs(_stickAngle - prevAngle);
                if(angleDiff < 90) { curAngle += angleDiff; }
                
                prevAngle = _stickAngle;
                //curAngle = _stickAngle;
                //if (Mathf.Abs(curAngle) >= angleChange) { return MotionCommandDirection.ANGLE_CHANGE; }
                //if (Mathf.Abs(curAngle) > angleChange / commands.Count) { return MotionCommandDirection.ANGLE_CHANGE; }
                return MotionCommandDirection.ANGLE_CHANGE;
            }
            

            if (_stickAngle < 45) { return MotionCommandDirection.FORWARD; }
            else if (_stickAngle < 135) { return MotionCommandDirection.SIDE; }
            else { return MotionCommandDirection.BACK; }
        }

        return MotionCommandDirection.NEUTRAL;


    }
}

[System.Serializable]
public class MotionCommandStep
{
    public string name;
    
    public MotionCommand.MotionCommandDirection command;
}





/*
/// <summary>
/// To hold input and perform continuous checks on them depending on current state
/// rather than discard them if check isn't met. So if in a jump state and jump input
/// is pressed a set amount of frames before landing, then we can decide if the jump
/// input is valid or not
/// </summary>
public class InputBuffer
{

    public List<InputBufferFrame> buffer;
    public static int bufferWindow = 25;  // How many frames our buffer lasts

    public List<int> buttonCommandCheck;
    public List<int> motionCommandCheck;

    void InitializeBuffer()
    {
        buffer = new List<InputBufferFrame>();

        for (int i = 0; i < bufferWindow; i++)
        {
            InputBufferFrame newBF = new InputBufferFrame();
            newBF.InitializeFrame();
            buffer.Add(newBF);
        }

        buttonCommandCheck = new List<int>();

        for (int i = 0; i < EngineData.actionData.rawInputs.Count; i++)
        {
            buttonCommandCheck.Add(-1);
        }
        
        motionCommandCheck = new List<int>();

        for (int i = 0; i < EngineData.actionData.motionCommands.Count; i++)
        {
            motionCommandCheck.Add(-1);
        }
    }


    public void Update()
    {
        EngineData.engineData.playerInputBuffer = this;
        if (buffer == null || buffer.Count < EngineData.actionData.rawInputs.Count || buffer.Count == 0) 
            InitializeBuffer();

        for (int i = 0; i < buffer.Count - 1; i++)
        {
            for (int r = 0; r < buffer[i].rawInputs.Count; r++)
            {
                // Set buffer of given input to most recent frame its inputted
                // and update previous frames to the current frame
                // So I press 'k' on frame 1, after 1 frame it'll be on frame 2
                buffer[i].rawInputs[r].value = buffer[i + 1].rawInputs[r].value;
                buffer[i].rawInputs[r].hold = buffer[i + 1].rawInputs[r].hold;
                buffer[i].rawInputs[r].used = buffer[i + 1].rawInputs[r].used;
            }
        }
        
        buffer[buffer.Count - 1].Update();

        for (int r = 0; r < buttonCommandCheck.Count; r++)
        {
            buttonCommandCheck[r] = -1;
            for (int b = 0; b < buffer.Count; b++)
            {
                if (buffer[b].rawInputs[r].CanExecute()) buttonCommandCheck[r] = b;
            }

            if (EngineData.actionData.rawInputs[r].inputType == RawInput.InputType.IGNORE)
                buttonCommandCheck[r] = 0;
        }

        int horIndex = 0, verIndex = 1;
        for (int m = 0; m < motionCommandCheck.Count; m++)
        {
            motionCommandCheck[m] = -1;
            EngineData.actionData.motionCommands[m].checkStep = 0;
            EngineData.actionData.motionCommands[m].curAngle = 0;
            for (int b = 0; b < buffer.Count; b++)
            {
                if (EngineData.actionData.motionCommands[m].TestCheck(buffer[b].rawInputs[horIndex].value,
                    buffer[b].rawInputs[verIndex].value))
                {
                    motionCommandCheck[m] = b;
                    break;
                }
            }
        }
    }
    
    public void UseInput(int _i)
    {
        
        buffer[buttonCommandCheck[_i]].rawInputs[_i].used = true;
        buttonCommandCheck[_i] = -1;

    }


}



/// <summary>
/// Contains the raw inputs in a single buffer frame
/// </summary>
public class InputBufferFrame
{
    // Button string as defined by input manager
    public List<InputBufferFrameState> rawInputs;

    public void InitializeFrame()
    {
        rawInputs = new List<InputBufferFrameState>();
        for (int i = 0; i < EngineData.actionData.rawInputs.Count; i++)
        {
            InputBufferFrameState newFS = new InputBufferFrameState();
            newFS.rawInput = i;
            rawInputs.Add(newFS);
        }
    }

    public void Update()
    {
        if (rawInputs == null || rawInputs.Count == 0 || rawInputs.Count != EngineData.actionData.rawInputs.Count) 
            InitializeFrame();
        foreach (InputBufferFrameState FS in rawInputs)
        {
            FS.ResolveCommand();
        }
        
    }

}

/// <summary>
/// State of a specific input during a given frame in the buffer
/// </summary>
public class InputBufferFrameState
{
    public int rawInput;
    public float value; // Value of the stick
    public int hold;    // Is Input Held
    public bool used;   // Did Input do something and should be cleared from buffer

    public void ResolveCommand()
    {
        used = false;
        RawInput _raw = EngineData.actionData.rawInputs[rawInput];
        switch (_raw.inputType)
        {
            case RawInput.InputType.BUTTON:
                if (Input.GetButton(_raw.name)) HoldUp(1f);
                else                            ReleaseHold();
                break;
            case RawInput.InputType.AXIS:
                if (Mathf.Abs(Input.GetAxisRaw(_raw.name)) > EngineData.engineData.deadZone)
                    HoldUp(Input.GetAxisRaw(_raw.name));
                else
                    ReleaseHold();
                break;
        }
    }

    public bool CanExecute()
    {
        return (hold == 1 && !used);
    }

    /// <summary>
    /// Holds neutral state is -1, so if pressed we set to 1 and begin incrementing
    /// based on how long its been held
    /// </summary>
    public void HoldUp(float _val)
    {
        value = _val;
        hold = hold < 0 ? 1 : hold++;
    }

    /// <summary>
    /// Reset Input Items state when released
    /// </summary>
    public void ReleaseHold()
    {
        if (hold > 0)
        {
            hold = -1;
            used = false;
        }
        else hold = 0;

        value = 0;
    }

    public bool MotionNeutral()
    {
        return Mathf.Abs(value) < EngineData.engineData.deadZone;
    }

}

/// <summary>
/// Types of raw inputs, an axis, moving an axis twice, button, etc...
/// </summary>
[System.Serializable]
public class RawInput
{
    public enum InputType { BUTTON, AXIS, DOUBLE_AXIS, DIRECTION, IGNORE }

    public InputType inputType;
    public string name;
}

/// <summary>
/// Class to handle directional inputs
/// </summary>
[System.Serializable]
public class MotionCommand
{
    public string name;
    public int motionWindow, confirmWindow;

    public List<MotionCommandDirection> commands;

    public bool clean;
    public bool anyOrder;

    public int checkStep;
    public int angleChange; //????

    public float curAngle, prevAngle; // ?????

    public enum MotionCommandDirection
    {
        NEUTRAL, FORWARD, BACK, SIDE, ANGLE_CHANGE
    }

    public bool TestCheck(float x, float y)
    {
        if (angleChange > 0)
        {
            //GetNumPadDirection(x, y);
            if (curAngle >= angleChange) { return true; }
        }
        else
        {
            if (commands == null) { return true; }

            if (checkStep >= commands.Count) { return true; }
            //if (commands[checkStep] == GetNumPadDirection(x, y)) { checkStep++; }
            //if (commands[checkStep] == _dir) { checkStep++; }
        }
        return false;
    }

    public MotionCommandDirection GetNumPadDirection(float x, float y)
    {
        if (Mathf.Abs(x) > EngineData.engineData.deadZone || Mathf.Abs(y) > EngineData.engineData.deadZone)
        {

            Vector3 charForward = EngineData.engineData.mainCharacter.character.transform.forward;
            Vector3 stickForward = new Vector3();
            Vector3 camForward = Camera.current.transform.forward;


            camForward.y = 0;
            camForward.Normalize();
            stickForward += camForward * y;

            stickForward += Camera.current.transform.right * x;
            stickForward.y = 0;
            stickForward.Normalize();
            
            float _stickAngle = Vector2.Angle(new Vector2(charForward.x, charForward.z), new Vector2(stickForward.x, stickForward.z));

            if (angleChange > 0)
            {
                
                _stickAngle = Vector2.Angle(new Vector2(0f, 1f), new Vector2(stickForward.x, stickForward.z));
                float angleDiff = Mathf.Abs(_stickAngle - prevAngle);
                if(angleDiff < 90) { curAngle += angleDiff; }
                
                prevAngle = _stickAngle;

                return MotionCommandDirection.ANGLE_CHANGE;
            }

            if (_stickAngle < 45) { return MotionCommandDirection.FORWARD; }
            else if (_stickAngle < 135) { return MotionCommandDirection.SIDE; }
            else { return MotionCommandDirection.BACK; }
        }

        return MotionCommandDirection.NEUTRAL;   
    }
    
}

[System.Serializable]
public class MotionCommandStep
{
    public string name;
    
    public MotionCommand.MotionCommandDirection command;
}

public class MotionChecker
{
    bool ready;
    List<bool> buffer;
}

*/