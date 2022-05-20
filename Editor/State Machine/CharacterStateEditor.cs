using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class CharacterStateEditorWindow : EditorWindow
{
    [MenuItem("State Machine/Character State Editor")]
    public static void Init()
    {
        EditorWindow.GetWindow(typeof(CharacterStateEditorWindow), false, "Character State Editor");
    }

    /// <summary>
    /// NOTE FOR SELF: NAME STORED IN CHARACTER DATA IS THE RIGHT ONE, FOR SOME REASON THEY'RE DIFFERENT IN BOTH
    /// BECAUSE USING NAME OF SO FILE NOT CHARACTER NAME ITSELF
    /// </summary>
    /// <param name="stateManager"></param>
    public static void SetCharacter(StateManager stateManager)
    {
        DataManager.Instance.currentCharacterEditorIndex =
            DataManager.Instance.GetCharacterNames().FindIndex(pred => pred.Contains(stateManager._character.name));
    }
    
    CharacterData dataAsset;
    Character character;
    
    CharacterState currentCharacterState;

    bool startEventFold;
    bool exitEventFold;
    bool eventFold;
    bool interruptFold;
    bool attackFold;
    string characterName = "Enter Character Name Here";


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

            // Make A specific editor to handle character creation & removal to ensure EngineData is properly clean


            GUILayout.EndHorizontal();
        }
        
        ////////////////////////////////////////////////////////////////

        scrollView = GUILayout.BeginScrollView(scrollView);
        GUILayout.BeginHorizontal();
        if (currentCharacterState == null)
            goto BADCHECK;
        GUILayout.Label(character.currentStateIndex.ToString() + " : " + currentCharacterState.stateName, GUILayout.Width(200));
        BADCHECK:
        character.currentStateIndex = EditorGUILayout.Popup(character.currentStateIndex, dataAsset.GetStateNames());
        
        if (GUILayout.Button("New Character State")) { character.characterStates.Add(new CharacterState()); character.currentStateIndex = character.characterStates.Count - 1; }

        currentCharacterState = character.characterStates[character.currentStateIndex];
        

        
        GUILayout.EndHorizontal();

        using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            currentCharacterState.stateName = EditorGUILayout.TextField("State Name : ", currentCharacterState.stateName, GUILayout.Width(500));
        }
        //GUILayout.EndHorizontal();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        //Animation
        EditorGUILayout.LabelField("State Animation Fields");
        Rect curveRange = new Rect(0, 0, 1, 2);
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUILayout.BeginHorizontal();
            currentCharacterState.length = EditorGUILayout.FloatField("Length : ", currentCharacterState.length);
            currentCharacterState.animCurve =
                EditorGUILayout.CurveField("Animation Curve: ", currentCharacterState.animCurve, Color.green, curveRange);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            currentCharacterState.blendRate = EditorGUILayout.FloatField("Blend Rate : ", currentCharacterState.blendRate);
            currentCharacterState.loop = GUILayout.Toggle(currentCharacterState.loop, "Loop?", EditorStyles.miniButton);
            GUILayout.EndHorizontal();
        }
        //GUILayout.BeginHorizontal();
        //GUILayout.EndHorizontal();
        
        StateEventEditor("On Start Events", ref currentCharacterState.onStateEnterEvents, ref startEventFold, false);

        StateEventEditor("On Exit Events", ref currentCharacterState.onStateExitEvents, ref exitEventFold, false);
   
        StateEventEditor("General Events", ref currentCharacterState.events, ref eventFold, true);

        GUILayout.EndScrollView();
        EditorUtility.SetDirty(dataAsset);
        
    }

    void StateEventEditor(string title, ref List<StateEvent> eventList, ref bool fold, bool timeEditor)
    {
        GUILayout.Label("");

        fold = EditorGUILayout.Foldout(fold, title);
        if (fold)
        {
            int deleteEvent = -1;
            //if(GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(35))){ currentCharacterState.events.Add(new StateEvent()); }
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                for (int e = 0; e < eventList.Count; e++)
                {

                    StateEvent currentEvent = eventList[e];
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(30)))
                    {
                        deleteEvent = e;
                    }

                    currentEvent.active = EditorGUILayout.Toggle(currentEvent.active, GUILayout.Width(20));
                    GUILayout.Label(e.ToString() + " : ", GUILayout.Width(25));

                    if (timeEditor)
                    {
                        EditorGUILayout.MinMaxSlider(ref currentEvent.start, ref currentEvent.end, 0f,
                            currentCharacterState.length, GUILayout.Width(400));
                        GUILayout.Label(
                            Mathf.Round(currentEvent.start).ToString() + " ~ " +
                            Mathf.Round(currentEvent.end).ToString(),
                            GUILayout.Width(75));
                    }
                    
                    currentEvent.script = EditorGUILayout.Popup(currentEvent.script, dataAsset.GetScriptNames());
                    GUILayout.EndHorizontal();

                    GUILayout.BeginVertical();

                    EventScript eventObjectContainer = character.eventScripts[currentEvent.script];
                    
                    //if (currentEvent.parameters == null) currentEvent.parameters = eventObjectContainer.baseParamList;
                    CharacterScriptEditorWindow.GenerateParameterFields(eventObjectContainer.eventScript, ref currentEvent.parameters, ref dataAsset);

                    GUILayout.EndVertical();
                    
                    // Condition Shit
                    GUILayout.BeginHorizontal();

                    currentEvent.hasCondition = EditorGUILayout.Toggle("Has Condition: ", currentEvent.hasCondition);
                    if (currentEvent.hasCondition)
                    {
                        if (currentEvent.condition == null) currentEvent.condition = new Conditions();
                        currentEvent.condition.condition = EditorGUILayout.ObjectField("Condition:", currentEvent.condition.condition, typeof(Condition)) as Condition;
                    }
                    
                    GUILayout.EndHorizontal();
                    
                    GUILayout.Label("");
                }

                if (deleteEvent > -1)
                {
                    eventList.RemoveAt(deleteEvent);
                } //currentCharacterState.events.RemoveAt(deleteEvent); }

                if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(35)))
                {
                    eventList.Add(new StateEvent());
                }

                GUILayout.Label("");
            }
        }
    }
    
}
