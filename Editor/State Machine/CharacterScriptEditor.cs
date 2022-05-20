using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class CharacterScriptEditorWindow : EditorWindow
{
    [MenuItem("State Machine/Character Script Editor")]
    public static void Init()
    {
        EditorWindow.GetWindow(typeof(CharacterScriptEditorWindow), false, "Event Script Editor");
    }
    
    public static void SetCharacter(StateManager stateManager)
    {
        DataManager.Instance.currentCharacterEditorIndex =
            DataManager.Instance.GetCharacterNames().FindIndex(pred => pred.Contains(stateManager._character.name));
    }
    
    CharacterData dataAsset;
    private Character character;

    Vector2 scrollView;
    private void OnGUI()
    {

        var labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontStyle = FontStyle.BoldAndItalic;
        labelStyle.fontSize = 18;

        // No Character Guard
        if (DataManager.Instance.characterData.Count == 0)
        {
            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Create A Character First In The State Editor", labelStyle);
            }

            return;
        }

        using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
        {

            GUILayout.BeginHorizontal();
            if (DataManager.Instance.characterData == null) DataManager.Instance.ReloadFields();
            DataManager.Instance.currentCharacterEditorIndex =
                EditorGUILayout.Popup(DataManager.Instance.currentCharacterEditorIndex, DataManager.Instance.GetCharacterNames().ToArray());
            dataAsset = DataManager.Instance.characterData[DataManager.Instance.currentCharacterEditorIndex];
            character = dataAsset.character;

            GUILayout.EndHorizontal();
        }
        
        
        ////////////////////////////////////////////////////////////////

        scrollView = GUILayout.BeginScrollView(scrollView);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Script Index : " + character.currentScriptIndex.ToString());
        character.currentScriptIndex = EditorGUILayout.Popup(character.currentScriptIndex, dataAsset.GetScriptNames());
        if (GUILayout.Button("New Event Script")) { character.eventScripts.Add(new EventScript()); character.currentScriptIndex = character.eventScripts.Count - 1; }
        GUILayout.EndHorizontal();
        
        EventScript currentScript = character.eventScripts[character.currentScriptIndex];

        currentScript.eventName = EditorGUILayout.TextField("Name : ", currentScript.eventName);
        
        currentScript.eventScript =
            EditorGUILayout.ObjectField("Event Object: ", currentScript.eventScript, typeof(StateEventObject), false) as StateEventObject;

        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            if (currentScript.eventScript != null)
                GenerateParameterFields(currentScript.eventScript, ref currentScript.baseParamList, ref dataAsset);
            
        }
        
        GUILayout.EndScrollView();
        EditorUtility.SetDirty(dataAsset);
    }


    public static void GenerateParameterFields(StateEventObject _event, ref List<GenericValueWrapper> paramVals, ref CharacterData data)
    {
        List<(Type, string)> parameterTypes = _event.GetParameterTypes();

        if (paramVals == null || paramVals.Count != parameterTypes.Count)
        {
            paramVals = new List<GenericValueWrapper>();
        }
        
        int i = 0;

        foreach (var param in parameterTypes)
        {
            if (i >= paramVals.Count) paramVals.Add(new GenericValueWrapper());
            // General Initilization if null value
            try
            { 
                if (!paramVals[i].IsObjectSaved(param.Item1))
                    paramVals[i].Value = Activator.CreateInstance(param.Item1);
            }
            catch (MissingMethodException e)
            {
                Debug.Log("You're good budd");
            }

            // Maybe some way to do it like this rather than a large if-else or switch statement?
            // paramVals[i] = EditorGUILayout.ObjectField(param.Item2, paramVals[i], param.Item1);


            switch (true)
            {
                case true when param.Item1 == typeof(float): 
                    paramVals[i].Value = EditorGUILayout.FloatField(param.Item2, (float) paramVals[i].Value);
                    paramVals[i].floatVal = (float) paramVals[i].Value;
                    break;
                case true when param.Item1 == typeof(bool): 
                    paramVals[i].Value = EditorGUILayout.Toggle(param.Item2, (bool) paramVals[i].Value);
                    paramVals[i].boolVal = (bool) paramVals[i].Value;
                    break;
                case true when param.Item1 == typeof(Vector3): 
                    paramVals[i].Value = EditorGUILayout.Vector3Field(param.Item2, (Vector3) paramVals[i].Value);
                    paramVals[i].vec3Val = (Vector3) paramVals[i].Value;
                    break;
                case true when param.Item1 == typeof(Vector2): 
                    paramVals[i].Value = EditorGUILayout.Vector2Field(param.Item2, (Vector2) paramVals[i].Value);
                    paramVals[i].vec2Val = (Vector2) paramVals[i].Value;
                    break;
                case true when param.Item1 == typeof(AnimationCurve): 
                    paramVals[i].Value = EditorGUILayout.CurveField(param.Item2, (AnimationCurve) paramVals[i].Value);
                    paramVals[i].curveVal = (AnimationCurve) paramVals[i].Value;
                    break;
                case true when param.Item1 == typeof(Texture2D): 
                    paramVals[i].Value = EditorGUILayout.ObjectField(param.Item2, (Texture2D) paramVals[i].Value, typeof(Texture2D));
                    paramVals[i].textureVal = (Texture2D) paramVals[i].Value;
                    break;
                default:
                    GUILayout.Label("TYPE CURRENTLY NOT SUPPORTED: " + param.Item1);
                    break;
            }
            
            EditorUtility.SetDirty(data);
            i++;
        }

    }
}