using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[CustomEditor(typeof(InputData))]
public class InputDataInspectorGUI : Editor
{
    private InputData _target;
    private bool editMapName;
    private void OnEnable()
    {
        _target = target as InputData;
        editMapName = false;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        
        EditorGUILayout.LabelField("Input Map: ", _target.inputActionMap, EditorStyles.boldLabel);
        if (GUILayout.Button("DEBUG TOOLS")) editMapName = true;
        if (editMapName)
        {
            List<string> inputMaps = InputManager.Instance.GetAllInputMaps();
            int curIndex = inputMaps.FindIndex(pred => pred.Contains(_target.inputActionMap));
            if (inputMaps.Count <= curIndex) curIndex = 0;

            curIndex = EditorGUILayout.Popup("Select Input Map: ", curIndex, inputMaps.ToArray());
            _target.inputActionMap = inputMaps[curIndex];
            
        }
        
        
        if (GUILayout.Button("Reinitialize Input Data")) _target.InitializeActionMap(true);

        DrawDefaultInspector();
        
        EditorUtility.SetDirty(_target);

        serializedObject.ApplyModifiedProperties();
    }
}