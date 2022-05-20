using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using UnityEditor;

//[ExecuteAlways]
public class InputManager : EditorSingleton<InputManager>
{
    #region DATAFIELDS
    
    public InputData inputData;
    private InputBuffer inputBuffer;
    private PlayerInput inputSystem;

    public Dictionary<string, Vector2> rawAxisContainer;
    public Dictionary<string, bool> rawButtonContainer;

    #endregion
    
    #region INPUTACTIONS_CALLBACKS

    public void StoreInput(InputAction.CallbackContext value)
    {
        // Could be a way to get name and auto-set it here
        if (value.action.type == InputActionType.Button)
        {
            rawButtonContainer[value.action.name] = value.ReadValueAsButton();
        }
        else if (value.action.type == InputActionType.Value)
        {
            rawAxisContainer[value.action.name] = value.ReadValue<Vector2>();
        }
    }

    private void InitializeInputDictionaries()
    {
        rawAxisContainer = new Dictionary<string, Vector2>();
        rawButtonContainer = new Dictionary<string, bool>();

        foreach (var inputAction in inputSystem.currentActionMap)
        {
            if (inputAction.type == InputActionType.Button)
            {
                rawButtonContainer[inputAction.name] = false;
            }
            else if (inputAction.type == InputActionType.Value)
            {
                rawAxisContainer[inputAction.name] = Vector2.zero;
            }
        }
    }

    #endregion

    #region MonoBehaviors

    protected override void Awake()
    {
        base.Awake();
        ReloadFields();

        if (!Application.isPlaying) return;
        
        inputSystem = GetComponent<PlayerInput>();
        InitializeInputDictionaries();
        inputBuffer = new InputBuffer();
        inputBuffer.InitializeBuffer();
    }
    
    private void OnEnable()
    {
        //AssemblyReloadEvents.afterAssemblyReload += ReloadFields;
        //AssemblyReloadEvents.afterAssemblyReload += Awake;
    }

    private void OnDisable()
    {
        //AssemblyReloadEvents.afterAssemblyReload -= ReloadFields;
        //AssemblyReloadEvents.afterAssemblyReload -= Awake;
    }

    // Fixed Update never called in editor
    private void FixedUpdate()
    {
        inputBuffer.Update();
    }

    #endregion

    #region Utilities

    public void ReloadFields()
    {
#if UNIT_EDITOR
        string[] guids = AssetDatabase.FindAssets("t: InputData");
            
        inputData = AssetDatabase.LoadAssetAtPath<InputData>(AssetDatabase.GUIDToAssetPath(guids[0]));
#endif
    }
    
    public InputBuffer GetInputBuffer()
    {
        return inputBuffer;
    }

    public void PrintMessage(string s) => Debug.Log(s);

    #endregion

    #region Check Input

    public bool IsInputHeld(InputCommand command)
    {

        return false;
    }

    public bool IsHoldReleased(InputCommand command)
    {
        // First buffer frame state is non-zero but next frame state is zero
        
        return false;
    }

    public int HowLongIsInputHeld(InputCommand command)
    {

        return 0;
    }
    
    public bool IsInputPressed(InputCommand command)
    {

        return false;
    }

    #endregion

    #region Register Input

    public void UseInput(int input)
    {
        if (inputBuffer.buffer == null || inputBuffer.buttonCommandCheck == null) inputBuffer.InitializeBuffer();
        inputBuffer.UseInput(input);
    }

    /// <summary>
    /// Checks whether the input has been registers
    /// At any given point, the input buffer holds the frames since an input was held
    /// Once released, the frame in which it was released has value -1 in the input buffer
    /// Hence we check for valid frames in the input buffer here CHECK THIS NOW MAY 13 2022!!!!!!!!!!!!!!!!!!!!!!!!!!!
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public bool CheckInputCommand(InputCommand command)
    {
        Debug.Log("Button Frame State: " + inputBuffer.buttonCommandCheck[command.input]);
        
        if (inputBuffer.buttonCommandCheck[command.input] < 0) return false;
        if (inputBuffer.motionCommandCheck[command.motionCommand] < 0) return false;

        return true;
    }

    #endregion
    public Vector3 GetNormalizedStickInput()
    {
        Vector2 movementInput = rawAxisContainer["Movement"];
        return Vector3.ClampMagnitude(new Vector3(movementInput.x, 0f, movementInput.y), 1);
    }

    public Vector3 GetLookInput()
    {
        Vector3 lookInput = rawAxisContainer["Look"];
        return Vector3.ClampMagnitude(lookInput, 1);
    }


    #region Debug UI

    void OnGUI()
    {
        Debug.Log("Being Called");
        Debug.Log("Null Buffer? " + (inputBuffer == null));
        if (Application.isPlaying && inputBuffer != null)
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

    #endregion
    
}