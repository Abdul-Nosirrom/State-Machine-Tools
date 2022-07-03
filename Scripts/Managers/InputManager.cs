using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using UnityEditor;

[ExecuteAlways]
public class InputManager : EditorSingleton<InputManager>
{
    #region Parameters

    [Header("Input Parameters")] 
    [Tooltip("Specify how many frames an input must be held to consider it held")]
    [Range(0, 25)]
    public int inputHeldLength;

    [Range(0, 1)] public float deadZone;

    [Range(0, 1)] public float lookSensitivity;

    #endregion
    
    #region DATAFIELDS
    
    [Header("Input Data")]
    [Tooltip("Currently Active Input Data Object, changes according to state machine of the player")]
    public InputData inputData;

    public List<string> bindingsToSkip = new List<string>();
    public List<string> mapsToSkip = new List<string>();

    private InputBuffer inputBuffer;
    [HideInInspector] public PlayerInput inputSystem;

    public GenericDictionary<string, Vector2> rawAxisContainer;
    public GenericDictionary<string, bool> rawButtonContainer;
    public GenericDictionary<string, Vector2> rawPassthroughContainer;

    public GenericDictionary<string, InputData> inputDataContainer;
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
        else if (value.action.type == InputActionType.PassThrough)
        {
            rawPassthroughContainer[value.action.name] = value.ReadValue<Vector2>();
        }
    }

    private void InitializeInputDictionaries()
    {
        rawAxisContainer = new GenericDictionary<string, Vector2>();
        rawButtonContainer = new GenericDictionary<string, bool>();
        rawPassthroughContainer = new GenericDictionary<string, Vector2>();

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
            else if (inputAction.type == InputActionType.PassThrough)
            {
                rawPassthroughContainer[inputAction.name] = Vector2.zero;
            }
        }
    }

    // Setup input data objects
    
    void InitializeInputData()
    {
#if UNITY_EDITOR
        inputDataContainer = new GenericDictionary<string, InputData>();
        string[] guids = AssetDatabase.FindAssets("t: InputData");

        foreach (var GUID in guids)
        {
            InputData establishedInputData = AssetDatabase.LoadAssetAtPath<InputData>(AssetDatabase.GUIDToAssetPath(GUID));
            
            // We skip this map
            if (mapsToSkip.Contains(establishedInputData.inputActionMap))
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(GUID));
            
            if (!inputDataContainer.ContainsKey(establishedInputData.inputActionMap))
                inputDataContainer[establishedInputData.inputActionMap] = establishedInputData;
            else if (inputDataContainer.ContainsKey(establishedInputData.inputActionMap) &&
                     inputDataContainer[establishedInputData.inputActionMap] == null)
                inputDataContainer[establishedInputData.inputActionMap] = establishedInputData;
            //else
            //    AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(GUID));
        }

        // Find the map that's not there, and create its appropriate InputData object
        foreach (var mapObject in inputSystem.actions.actionMaps)
        {
            string map = mapObject.name;
            if (inputDataContainer.ContainsKey(map)) continue;
            if (mapsToSkip.Contains(map)) continue;

            Debug.Log("No Input Data Found For Input Map: " + map);
            
            /*
            string objectName = "InputData_" + map.ToUpper();
            InputData newInputData = ScriptableObject.CreateInstance<InputData>();
            AssetDatabase.CreateAsset(newInputData, $"Assets/Data/Inputs/Input Definitions/{objectName}.asset");
            newInputData.inputActionMap = mapObject.name;
            newInputData.InitializeActionMap();
            inputDataContainer[map] = newInputData;
            AssetDatabase.SaveAssets();
            */
        }
#endif
    }

    public void SwitchActionMap(string newMap)
    {
        if (inputData != null && newMap.Equals(inputData.inputActionMap)) return;
        
        inputSystem.SwitchCurrentActionMap(newMap);
        if (mapsToSkip.Contains(newMap)) return;
        
        
        inputData = inputDataContainer[newMap];
        
        // Reinitialize buffer for new input & dictionaries
        InitializeInputDictionaries();
    }

    public bool SkipBinding(InputAction action)
    {
        var bindings = action.bindings.ToArray();
        
        foreach (var binding in bindings)
        {
            if (bindingsToSkip.Contains(binding.path)) return true;
        }
        
        return false;
    }

    public void RegenerateInputData()
    {
        InitializeInputData();
        
        if (Application.isPlaying)
            InitializeInputDictionaries();
    }

    #endregion

    #region MonoBehaviors

    protected override void Awake()
    {
        base.Awake();
        if (!Application.isPlaying) return;

        inputData = inputDataContainer[inputSystem.currentActionMap.name];
        InitializeInputDictionaries();
        inputBuffer = new InputBuffer();
    }

    // Fixed Update never called in editor
    private void FixedUpdate()
    {
        if (inputData == null) inputData = inputDataContainer[inputSystem.currentActionMap.name];
        else inputBuffer.UpdateBuffer();
    }
    

    #endregion

    #region Utilities

    public InputBuffer GetInputBuffer()
    {
        return inputBuffer;
    }

    public List<string> GetInputMaps()
    {
        List<string> maps = new List<string>();

        foreach (var map in inputDataContainer.Keys)
        {
            if (mapsToSkip.Contains(map)) continue;
            maps.Add(map);
        }

        return maps;
    }

    public List<string> GetAllInputMaps()
    {
        List<string> maps = new List<string>();

        foreach (var map in inputSystem.actions.actionMaps)
        {
            maps.Add(map.name);
        }

        return maps;
    }

    #endregion

    #region Check Input

    // These dont really respect the fact that we're using a buffer
    public bool IsInputHeld(InputCommand command)
    {
        Debug.Log("Input Status: " + command.input);
        return inputBuffer.buffer.Front().inputsFrameState[command.input].hold > inputHeldLength;
    }

    public bool IsHoldReleased(InputCommand command)
    {
        // First buffer frame state is non-zero but next frame state is zero
        return inputBuffer.buffer.Front().inputsFrameState[command.input].hold == 0;
    }

    public int HowLongIsInputHeld(InputCommand command)
    {
        return inputBuffer.buffer.Front().inputsFrameState[command.input].hold;
    }
    
    public bool IsInputPressed(InputCommand command)
    {
        return inputBuffer.buffer.Front().inputsFrameState[command.input].hold > 0;
    }

    #endregion

    #region Register Input

    public void UseInput(int input)
    {
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
        
        if (inputBuffer.buttonInputCurrentState[command.input] < 0) return false;
        if (inputBuffer.motionInputCurrentState.Count != 0 && inputBuffer.motionInputCurrentState[command.motionCommand] < 0) return false;

        return true;
    }

    public bool IsInputNone(InputCommand command)
    {
        return (inputData.rawInputs[command.input].inputType == RawInput.InputType.IGNORE);
    }

    #endregion
    public Vector3 GetNormalizedStickInput()
    {
        if (!rawAxisContainer.ContainsKey("Movement")) return Vector3.zero;
        
        Vector2 movementInput = rawAxisContainer["Movement"];
        return Vector3.ClampMagnitude(new Vector3(movementInput.x, 0f, movementInput.y), 1);
    }

    public Vector3 GetLookInput()
    {
        if (!rawAxisContainer.ContainsKey("Look")) return Vector3.zero;
        
        Vector3 lookInput = rawAxisContainer["Look"];
        return Vector3.ClampMagnitude(lookInput, 1) * lookSensitivity;
    }
    
}