using System;
using UnityEditor;
using UnityEngine;


public class StateConditionEditor : EditorWindow
{
    private CommandStep _commandStep;
    private Conditions _conditions;
    private ActionData _data;

    public  void SetCommandStep(ref ActionData data, ref CommandStep givenCS, int moveList, int commandState, int commandID)
    {
        _commandStep = givenCS;
        _conditions = givenCS.conditions;
        _data = data;
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.Toggle(_commandStep.conditions.grounded, nameof(_conditions.grounded));
        GUILayout.Toggle(_conditions.holdButton, nameof(_conditions.holdButton));
        GUILayout.Toggle(_conditions.inAir, nameof(_conditions.inAir));
        GUILayout.Toggle(_conditions.distToTargetValid, "Distance To Target Valid");
        
        GUILayout.EndVertical();
        
        if (GUILayout.Button("Close"))
        {
            Close();
        }
        
        EditorUtility.SetDirty(_data);
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }
}
