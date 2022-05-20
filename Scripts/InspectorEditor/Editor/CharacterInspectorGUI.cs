using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
/*
[CustomEditor(typeof(StateManager))]
public class CharacterInspector : UnityEditor.Editor
{
    private StateManager _target;
    

    private void OnEnable()
    {
        _target = target as StateManager;
    }

    public override void OnInspectorGUI()
    {
        /*
        serializedObject.Update();

        EditorGUILayout.HelpBox("Determines which FSM corresponds to this character", MessageType.Info);
        // Choose an option from a list
        // Update the selected option on the underlying instance of our StateManager class
        using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            GUILayout.Label("Character:", GUILayout.Width(100));
            _charIndex.intValue = EditorGUILayout.Popup(_target._charIndex, EngineData.characterData.GetCharacterNames());
        }

        serializedObject.ApplyModifiedProperties();
      
        //DrawDefaultInspector();
    }
}
*/
[CustomEditor(typeof(PlayerStateManager)), CanEditMultipleObjects]
public class PlayerCharacterInspector : UnityEditor.Editor
{
    private PlayerStateManager _target;
    private bool stateInfoFold;
    private bool modifiableFold;
    private void OnEnable()
    {
        _target = target as PlayerStateManager;
        stateInfoFold = true;
        modifiableFold = true;
    }

    public override void OnInspectorGUI()
    {
        
        serializedObject.Update();

        EditorGUILayout.HelpBox("Determines which FSM corresponds to this character", MessageType.Info);
        // Choose an option from a list
        // Update the selected option on the underlying instance of our StateManager class
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            //GUILayout.Label("Character:", GUILayout.Width(100));
            _target.characterData =
                EditorGUILayout.ObjectField("Character Data: ", _target.characterData, typeof(CharacterData)) as CharacterData;

            //if (GUILayout.Button("Open Attack Editor Window"))
            
            _target.hitbox = EditorGUILayout.ObjectField("Hit Box Attached: ", _target.hitbox, typeof(HitBox)) as HitBox;
        }
        
        //EditorGUILayout.HelpBox("Open Appropriate Editor", MessageType.Info);
        using (new GUILayout.HorizontalScope(EditorStyles.label))
        {
            if (GUILayout.Button("Open State Machine Editor"))
            {
                StateMachineGraph.OpenStateMachineWindow();
                StateMachineGraph.OpenGraphEditor(_target);
            }

            if (GUILayout.Button("Open State Definition Editor"))
            {
                CharacterStateEditorWindow.Init();
                CharacterStateEditorWindow.SetCharacter(_target);
            }

            if (GUILayout.Button("Open State Event Editor"))
            {
                CharacterScriptEditorWindow.Init();
                CharacterScriptEditorWindow.SetCharacter(_target);
            }
        }

        modifiableFold = EditorGUILayout.Foldout(modifiableFold, "Modifiable Data");
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            if (modifiableFold)
            {
                _target.stateMachineChangeGrace =
                    EditorGUILayout.IntField("Grace Period Before Valid State Machine Selection: ",
                        _target.stateMachineChangeGrace);
                StateManager.whiffWindow = EditorGUILayout.FloatField("Global Whiff Window: ", StateManager.whiffWindow);
            }
        }
        
        stateInfoFold = EditorGUILayout.Foldout(stateInfoFold, "State Information");
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            if (stateInfoFold)
            {
                //EditorGUILayout.LabelField("Current State Index: ", _target.currentStateIndex.ToString());
                EditorGUILayout.LabelField("Current State Time: ", _target.currentStateTime.ToString());
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Previous State Time: ", _target.prevStateTime.ToString());
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Current Attack Index: ", _target.currentAttackIndex.ToString());
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.Toggle("Can Cancel", _target.canCancel);
            } 
        }

        EditorGUILayout.LabelField("Debug Events");
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            _target.OnStateChangedEvent = EditorGUILayout.ObjectField("On State Change: ", _target.OnStateChangedEvent, typeof(StringEvent)) as StringEvent;
            _target.OnStateMachineChangedEvent = EditorGUILayout.ObjectField("On State Machine Change: ", _target.OnStateMachineChangedEvent, typeof(StringEvent)) as StringEvent;
        }

        serializedObject.ApplyModifiedProperties();
    }
}