using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Input Data", menuName = "Core Data/Input Data", order=1)]
[System.Serializable]
public class InputData : ScriptableObject
{
    [HideInInspector] [SerializeField] 
    public string inputActionMap;
    public List<RawInput> rawInputs;
    public List<MotionCommand> motionCommands;

    public void InitializeActionMap(bool skipMotionCommands = false)
    {
        rawInputs = new List<RawInput>();

        foreach (var action in InputManager.Instance.inputSystem.actions.FindActionMap(inputActionMap))
        {
            // Skip mouse 
            if (InputManager.Instance.SkipBinding(action)) continue;
            
            RawInput actionInput = new RawInput();
            switch (action.type)
            {
                // Both these take vector 2s
                case InputActionType.PassThrough:
                case InputActionType.Value:
                    // Decompose to X and Y
                    RawInput actionInputY = new RawInput();
                    actionInput.inputType = RawInput.InputType.AXIS;
                    actionInputY.inputType = RawInput.InputType.AXIS;
                    actionInput.name = action.name + "X";
                    actionInputY.name = action.name + "Y";
                    rawInputs.Add(actionInput);
                    rawInputs.Add(actionInputY);
                    break;
                case InputActionType.Button:
                    actionInput.name = action.name;
                    actionInput.inputType = RawInput.InputType.BUTTON;
                    rawInputs.Add(actionInput);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        // Add NONE raw input type
        RawInput noneInput = new RawInput()
        {
            name = "None",
            inputType = RawInput.InputType.IGNORE
        };
        
        rawInputs.Add(noneInput);

        if (skipMotionCommands) return;
        
        // Setup NONE motion command
        motionCommands = new List<MotionCommand>();
        motionCommands.Add(new MotionCommand());
        motionCommands[0].name = "None";
        
        // Other motion commands should be manually set-up
    }
    
    
    public string[] GetRawInputNames()
    {
        string[] _names = new string[rawInputs.Count];
        for (int i = 0; i < _names.Length; i++)
        {
            _names[i] = rawInputs[i].name;
        }
        return _names;
    }
    
    public string[] GetMotionCommandNames()
    {
        string[] _names = new string[motionCommands.Count];
        for (int i = 0; i < _names.Length; i++)
        {
            _names[i] = motionCommands[i].name;
        }
        return _names;
    }
}

