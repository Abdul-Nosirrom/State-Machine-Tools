using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[CustomEditor(typeof(InputManager))]
public class InputManagerInspectorGUI : UnityEditor.Editor
{
    private List<string> skippableBindings;

    private List<string> skippableMaps;
    
    private InputManager _target;

    private bool mapSkipsFold;
    private bool bindingSkipsFold;
    private bool buttonStatusFold;
    private bool axisStatusFold;
    private bool inputDataFold;
    private void OnEnable()
    {
        _target = target as InputManager;

        skippableBindings = new List<string>();
        skippableMaps = new List<string>();
        
        InitializeBindingMapList();
        
        mapSkipsFold = false;
        bindingSkipsFold = false;
        buttonStatusFold = true;
        axisStatusFold = true;
        inputDataFold = false;
    }

    private void InitializeBindingMapList()
    {
        foreach (var binding in _target.inputSystem.actions.bindings)
        {
            skippableBindings.Add(binding.path);
        }

        foreach (var map in _target.inputSystem.actions.actionMaps)
        {
            skippableMaps.Add(map.name);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Undo.RecordObject(_target, "Input Manager Change");
        
        EditorGUILayout.HelpBox("Parameters regarding gameplay input", MessageType.Info);
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            _target.inputSystem = EditorGUILayout.ObjectField("Player Input Component: ", _target.inputSystem, typeof(PlayerInput)) as PlayerInput;
            InputBuffer.bufferSize = EditorGUILayout.IntSlider("Buffer Size: ", InputBuffer.bufferSize, 1, 25);
            _target.inputHeldLength = EditorGUILayout.IntSlider("Input Hold Length: ", _target.inputHeldLength, 10, 40);
            _target.deadZone = EditorGUILayout.Slider("Dead Zone: ", _target.deadZone, 0, 1);
            _target.lookSensitivity = EditorGUILayout.Slider("Sensitivity: ", _target.lookSensitivity, 0, 10);
        }
        
        
        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Current Input State [ReadOnly]", MessageType.Info);
            buttonStatusFold = EditorGUILayout.Foldout(buttonStatusFold, "Button States");
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (buttonStatusFold)
                {
                    foreach (var button in _target.rawButtonContainer)
                    {
                        EditorGUILayout.Toggle(button.Key, button.Value);
                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    }
                }
            }

            axisStatusFold = EditorGUILayout.Foldout(axisStatusFold, "Axis States");
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (axisStatusFold)
                {
                    foreach (var axis in _target.rawAxisContainer)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(axis.Key);
                        EditorGUILayout.LabelField(" | X: " + axis.Value.x + " | Y: " + axis.Value.y + " |");
                        GUILayout.EndHorizontal();
                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    }
                }
            }
        }
        
        mapSkipsFold = EditorGUILayout.Foldout(mapSkipsFold, "Input Maps To Skip");
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            if (mapSkipsFold)
            {
                for (int i = 0; i < _target.mapsToSkip.Count; i++)
                {
                    int mapSkipIndex = skippableMaps.ToList().FindIndex(pred => pred.Contains(_target.mapsToSkip[i]));
                    if (mapSkipIndex >= skippableMaps.Count) mapSkipIndex = 0;
                    mapSkipIndex = EditorGUILayout.Popup(mapSkipIndex, skippableMaps.ToArray());
                    _target.mapsToSkip[i] = skippableMaps[mapSkipIndex];
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("+"))
                {
                    _target.mapsToSkip.Add(skippableMaps[0]);
                }
                if (GUILayout.Button("-"))
                {
                    if (_target.mapsToSkip.Count != 0)
                        _target.mapsToSkip.RemoveAt(_target.mapsToSkip.Count - 1);
                }
                GUILayout.EndHorizontal();
            }
        }
        
        bindingSkipsFold = EditorGUILayout.Foldout(bindingSkipsFold, "Input Bindings To Skip");
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            if (bindingSkipsFold)
            {
                for (int i = 0; i < _target.bindingsToSkip.Count; i++)
                {
                    int bindingSkipIndex = skippableBindings.ToList().FindIndex(pred => pred.Contains(_target.bindingsToSkip[i]));
                    if (bindingSkipIndex >= skippableBindings.Count) bindingSkipIndex = 0;
                    bindingSkipIndex = EditorGUILayout.Popup(bindingSkipIndex, skippableBindings.ToArray());
                    _target.bindingsToSkip[i] = skippableBindings[bindingSkipIndex];
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("+"))
                {
                    _target.bindingsToSkip.Add(skippableBindings[0]);
                }
                if (GUILayout.Button("-"))
                {
                    if (_target.bindingsToSkip.Count != 0)
                        _target.bindingsToSkip.RemoveAt(_target.bindingsToSkip.Count - 1);
                }
                GUILayout.EndHorizontal();
            }
        }


        inputDataFold = EditorGUILayout.Foldout(inputDataFold, "Generated Input Data [ReadOnly]");
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            if (inputDataFold)
            {
                foreach (var inputData in _target.inputDataContainer)
                {
                    EditorGUILayout.ObjectField(inputData.Key, inputData.Value, typeof(InputData));
                }
            }
        }

        if (GUILayout.Button("Regenerate Input Data"))
        {
            _target.RegenerateInputData();
        }
        EditorGUILayout.ObjectField("Current Input Data Object: ", _target.inputData, typeof(InputData));
        
        Undo.RecordObject(_target, "Input Manager Change Done");
        serializedObject.ApplyModifiedProperties();
    }
}