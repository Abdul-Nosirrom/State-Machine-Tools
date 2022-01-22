using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(CharacterStateManager))]
public class CharacterInspector : Editor
{
    private CharacterStateManager _target;
    
    private SerializedProperty _charIndex;

    private void OnEnable()
    {
        _target = target as CharacterStateManager;
        _charIndex = serializedObject.FindProperty("_charIndex");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.HelpBox("Determines which FSM corresponds to this character", MessageType.Info);
        // Choose an option from a list
        // Update the selected option on the underlying instance of our StateManager class
        using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            GUILayout.Label("Character:", GUILayout.Width(100));
            _charIndex.intValue = EditorGUILayout.Popup(_target._charIndex, EngineData.actionData.GetCharacterNames());
        }

        serializedObject.ApplyModifiedProperties();
      
        DrawDefaultInspector();
    }
    
    
}