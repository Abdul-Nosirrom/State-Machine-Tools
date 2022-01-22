using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CharacterStateEditorWindow : EditorWindow
{
    [MenuItem("Window/Character State Editor")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(CharacterStateEditorWindow), false, "Character State Editor");
    }

    ActionData dataAsset;
    Character coreData;
    
    CharacterState currentCharacterState;
    
    bool eventFold;
    bool interruptFold;
    bool attackFold;


    Vector2 scrollView;
    private void OnGUI()
    {
        
        if (dataAsset == null)
        {
            foreach (string guid in AssetDatabase.FindAssets("t: ActionData"))
            {
                dataAsset = AssetDatabase.LoadAssetAtPath<ActionData>(AssetDatabase.GUIDToAssetPath(guid));
            }
        }
        
        // Character Select Label
        coreData = dataAsset.characters[dataAsset.currentCharacterIndex];
        // Character Name
        using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            //GUILayout.Label("");

            coreData.name = GUILayout.TextField(coreData.name);

            GUILayout.BeginHorizontal();
            dataAsset.currentCharacterIndex =
                EditorGUILayout.Popup(dataAsset.currentCharacterIndex, dataAsset.GetCharacterNames());

            if (GUILayout.Button("New Character"))
            {
                dataAsset.characters.Add(new Character());
                dataAsset.currentCharacterIndex = dataAsset.characters.Count - 1;
            }
            if (GUILayout.Button("Remove Character"))
            {
                if (dataAsset.characters.Count > 1)
                {
                    dataAsset.characters.RemoveAt(dataAsset.currentCharacterIndex);
                    dataAsset.currentCharacterIndex = dataAsset.characters.Count - 1;
                }
            }
            coreData = dataAsset.characters[dataAsset.currentCharacterIndex];



            GUILayout.EndHorizontal();
        }
        
        ////////////////////////////////////////////////////////////////

        scrollView = GUILayout.BeginScrollView(scrollView);
        GUILayout.BeginHorizontal();
        if (currentCharacterState == null)
            goto BADCHECK;
        GUILayout.Label(coreData.currentStateIndex.ToString() + " : " + currentCharacterState.stateName, GUILayout.Width(200));
        BADCHECK:
        coreData.currentStateIndex = EditorGUILayout.Popup(coreData.currentStateIndex, dataAsset.GetStateNames());
        
        if (GUILayout.Button("New Character State")) { coreData.characterStates.Add(new CharacterState()); coreData.currentStateIndex = coreData.characterStates.Count - 1; }

        currentCharacterState = coreData.characterStates[coreData.currentStateIndex];
        

        
        GUILayout.EndHorizontal();

        currentCharacterState.stateName = EditorGUILayout.TextField("State Name : ", currentCharacterState.stateName, GUILayout.Width(500));
        //Animation
        GUILayout.BeginHorizontal();
        currentCharacterState.length = EditorGUILayout.FloatField("Length : ", currentCharacterState.length);
        currentCharacterState.blendRate = EditorGUILayout.FloatField("Blend Rate : ", currentCharacterState.blendRate);
        currentCharacterState.loop = GUILayout.Toggle(currentCharacterState.loop, "Loop?", EditorStyles.miniButton);
        GUILayout.EndHorizontal();

        //Add Flags Here
        currentCharacterState.groundedReq = GUILayout.Toggle(currentCharacterState.groundedReq, "Grounded?", EditorStyles.miniButton, GUILayout.Width(75));
        currentCharacterState.railReq = GUILayout.Toggle(currentCharacterState.railReq, "On Rail?", EditorStyles.miniButton, GUILayout.Width(75));

        //Events!
        GUILayout.Label("");
        //GUILayout.Label("Events");
        eventFold = EditorGUILayout.Foldout(eventFold, "Events");
        if (eventFold)
        {
            int deleteEvent = -1;
            //if(GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(35))){ currentCharacterState.events.Add(new StateEvent()); }
            for (int e = 0; e < currentCharacterState.events.Count; e++)
            {
                StateEvent currentEvent = currentCharacterState.events[e];
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("X", EditorStyles.miniButton,GUILayout.Width(30))){ deleteEvent = e; }
                currentEvent.active = EditorGUILayout.Toggle(currentEvent.active, GUILayout.Width(20));
                GUILayout.Label(e.ToString() + " : ", GUILayout.Width(25));
                EditorGUILayout.MinMaxSlider(ref currentEvent.start, ref currentEvent.end, 0f, currentCharacterState.length, GUILayout.Width(400));
                GUILayout.Label(Mathf.Round(currentEvent.start).ToString() + " ~ " + Mathf.Round(currentEvent.end).ToString(), GUILayout.Width(75));
                currentEvent.script = EditorGUILayout.Popup(currentEvent.script, dataAsset.GetScriptNames());
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();

                if (currentEvent.parameters.Count != coreData.eventScripts[currentEvent.script].parameters.Count)
                {
                    currentEvent.parameters = new List<EventParameter>();
                    for (int i = 0; i < coreData.eventScripts[currentEvent.script].parameters.Count; i++)
                    {
                        currentEvent.parameters.Add(new EventParameter());
                    }
                }
              
                for(int p = 0; p < currentEvent.parameters.Count; p++)
                {
                    if (p % 3 == 0) {  GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); GUILayout.Label("", GUILayout.Width(250)); }

                    currentEvent.parameters[p].val.paramType =
                        coreData.eventScripts[currentEvent.script].parameters[p].val.paramType;
                        GUILayout.Label(coreData.eventScripts[currentEvent.script].parameters[p].name + " : ",
                            GUILayout.Width(85));
                        //currentEvent.parameters[p].val =
                            //EditorGUILayout.FloatField(currentEvent.parameters[p].val, GUILayout.Width(75));
                        Debug.Log("Parameter " + currentEvent.parameters[p].val.paramType);
                        switch (currentEvent.parameters[p].val.paramType)
                        {
                            case ParameterType.SupportedTypes.FLOAT:
                                currentEvent.parameters[p].val.floatVal = EditorGUILayout.FloatField( (float)currentEvent.parameters[p].val.GetVal(), GUILayout.Width(75));
                                break;
                            case ParameterType.SupportedTypes.BOOL:
                                currentEvent.parameters[p].val.boolVal = EditorGUILayout.Toggle( (bool)currentEvent.parameters[p].val.GetVal());
                                break;
                            case ParameterType.SupportedTypes.ANIMATION_CURVE:
                                currentEvent.parameters[p].val.curveVal = EditorGUILayout.CurveField( (AnimationCurve)currentEvent.parameters[p].val.GetVal());
                                break;
                        }

                }
                
                GUILayout.EndHorizontal();
                GUILayout.Label(""); 
            }
            
            if(deleteEvent > -1) { currentCharacterState.events.RemoveAt(deleteEvent); }
            if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(35))) { currentCharacterState.events.Add(new StateEvent()); }
            GUILayout.Label("");
        }
        
        // Interrupts!
        GUILayout.Label("");
        interruptFold = EditorGUILayout.Foldout(interruptFold, "Interrupts");
        if (interruptFold)
        {
            int deleteInterrupt = -1;

            for (int i = 0; i < currentCharacterState.interrupts.Count; i++)
            {
                Interrupt currentInterrupt = currentCharacterState.interrupts[i];
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(30)))
                {
                    deleteInterrupt = i;
                }

                currentInterrupt.type = (Interrupt.InterruptTypes)EditorGUILayout.EnumPopup("Interrupt Type:", currentInterrupt.type);
                currentInterrupt.state = EditorGUILayout.Popup(currentInterrupt.state, dataAsset.GetStateNames());
                GUILayout.EndHorizontal();
            }
            
            if (deleteInterrupt > -1) currentCharacterState.interrupts.RemoveAt(deleteInterrupt);
            if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(35))) { currentCharacterState.interrupts.Add(new Interrupt()); }
            GUILayout.Label("");
        }

        GUILayout.EndScrollView();
        EditorUtility.SetDirty(dataAsset);
        
    }
    
}
