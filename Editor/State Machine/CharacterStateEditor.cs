using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        characterIndex =
            FSMDataUtilities.GetCharacterNames().FindIndex(pred => pred.Contains(stateManager._character.name));
    }
    
    CharacterData dataAsset;
    Character character;
    private static int characterIndex;
    
    CharacterState currentCharacterState;

    bool startEventFold;
    bool exitEventFold;
    bool eventFold;
    bool generalEventFold = true;
    bool animOverrideFold;
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
        if (!FSMDataUtilities.AreThereCharacters())
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

            if (!FSMDataUtilities.isDataCollected) FSMDataUtilities.CollectCharacterData();
            
            characterIndex = EditorGUILayout.Popup(characterIndex, FSMDataUtilities.GetCharacterNames().ToArray());
            dataAsset = FSMDataUtilities.GetCharacterData(characterIndex);
            character = dataAsset.character;
            
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
            currentCharacterState.strict =
                GUILayout.Toggle(currentCharacterState.strict, "Strict?", EditorStyles.miniButton);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            currentCharacterState.hasCoolDown =
                GUILayout.Toggle(currentCharacterState.hasCoolDown, "Has Cooldown?", EditorStyles.miniButton);
            if (currentCharacterState.hasCoolDown)
            {
                currentCharacterState.coolDown = EditorGUILayout.Slider(currentCharacterState.coolDown, 0f, 20f);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            currentCharacterState.isUnlockable = GUILayout.Toggle(currentCharacterState.isUnlockable, "Is Unlockable?", EditorStyles.miniButton);
            if (currentCharacterState.isUnlockable)
            {
                currentCharacterState.stateUnlocked =
                    EditorGUILayout.Toggle("Unlock Status: ", currentCharacterState.stateUnlocked);
            }
            GUILayout.EndHorizontal();
        }
        //GUILayout.BeginHorizontal();
        //GUILayout.EndHorizontal();

        // =============== Animation Overrides =============== //

        if (character.animator != null && character.animator.animationClips.Length > 0)
        {

            animOverrideFold = EditorGUILayout.Foldout(animOverrideFold, "Animation Overrides");

            if (animOverrideFold)
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.HelpBox("If the condition is satisfied, state will use the override animation clip",
                        MessageType.Info);

                    AnimationOverrideEditor("Animation Overrides", ref currentCharacterState.animationOverrides,
                        character.animator);
                }
            }

        }
        // ================================================== //


        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider, GUILayout.Height(10f));
  
        generalEventFold = EditorGUILayout.Foldout(generalEventFold, "State Events");

        if (generalEventFold)
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                StateEventEditor("On Start Events", ref currentCharacterState.onStateEnterEvents, ref startEventFold,
                    false);

                StateEventEditor("On Exit Events", ref currentCharacterState.onStateExitEvents, ref exitEventFold,
                    false);

                StateEventEditor("General Events", ref currentCharacterState.events, ref eventFold, true);
            }
        }

        GUILayout.EndScrollView();
        EditorUtility.SetDirty(dataAsset);
        
    }

    void StateEventEditor(string title, ref List<StateEvent> eventList, ref bool fold, bool timeEditor)
    {
        //GUILayout.Label("");

        fold = EditorGUILayout.Foldout(fold, title);
        if (fold)
        {
            int deleteEvent = -1;
            //if(GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(35))){ currentCharacterState.events.Add(new StateEvent()); }
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                for (int e = 0; e < eventList.Count; e++)
                {
                    if (e > 0) EditorGUILayout.LabelField("", GUI.skin.horizontalSlider, GUILayout.Height(10f));
                    
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
                            currentCharacterState.length, GUILayout.Width(250));
                        GUILayout.Label(
                            Mathf.Round(currentEvent.start).ToString() + " ~ " +
                            Mathf.Round(currentEvent.end).ToString(),
                            GUILayout.Width(75));
                    }
                    
                    
                    var checkingTypes = EditorGUILayout.ObjectField(currentEvent.eventObject, typeof(StateEventObject), GUILayout.Width(200f)) as StateEventObject;

                    if (checkingTypes is PlayerEventObject or StateEventObject)
                    {
                        if (dataAsset is PlayableCharacterData) currentEvent.eventObject = checkingTypes;
                    }

                    if (checkingTypes is AIEventObject or StateEventObject)
                    {
                        if (dataAsset is AICharacterData) currentEvent.eventObject = checkingTypes;
                    }
                    

                    //GUILayout.EndHorizontal();

                    //GUILayout.BeginHorizontal();
                    
                    //if (currentEvent.parameters == null) currentEvent.parameters = eventObjectContainer.baseParamList;
                    EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.Width(5f));
                    if (currentEvent.eventObject != null)
                        GenerateParameterFields(currentEvent.eventObject, ref currentEvent.parameters, ref dataAsset);

                    GUILayout.EndHorizontal();
                    //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider, GUILayout.Height(10f));
                    // Condition Shit
                    GUILayout.BeginHorizontal();

                    EditorGUIUtility.labelWidth = 100;
                    currentEvent.hasCondition = EditorGUILayout.Toggle("Has Condition: ", currentEvent.hasCondition, GUILayout.Width(200f));

                    if (currentEvent.hasCondition)
                    {
                        if (currentEvent.condition == null) currentEvent.condition = new Conditions();
                        currentEvent.condition.condition = EditorGUILayout.ObjectField(currentEvent.condition.condition, typeof(Condition), GUILayout.Width(300f)) as Condition;
                    }

                    EditorGUIUtility.labelWidth = 0;
                    GUILayout.EndHorizontal();
                    
                    //GUILayout.Label("");
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
    
    void AnimationOverrideEditor(string title, ref List<AnimationConditionOverrides> animationConditions, RuntimeAnimatorController animController)
    {
        GUILayout.Label("");
        
        AnimatorController usableAnimController = animController as AnimatorController;
        

        if (animationConditions == null) animationConditions = new List<AnimationConditionOverrides>();
        
        {
            int deleteEvent = -1;
            //if(GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(35))){ currentCharacterState.events.Add(new StateEvent()); }
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                for (int e = 0; e < animationConditions.Count; e++)
                {
                    if (e > 0) EditorGUILayout.LabelField("", GUI.skin.horizontalSlider, GUILayout.Height(10f));
                    
                    AnimationConditionOverrides currentAnimOverride = animationConditions[e];
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(30)))
                    {
                        deleteEvent = e;
                    }

                    List<string> animNames = new List<string>();

                    foreach (var anim in usableAnimController.layers[0].stateMachine.states)
                    {
                        animNames.Add(anim.state.name);
                    }

                    foreach (var animSub in usableAnimController.layers[0].stateMachine.stateMachines)
                    {
                        foreach (var anim in animSub.stateMachine.states)
                        {
                            animNames.Add(anim.state.name);
                        }
                    }

                    int animIndex = 0;
                    foreach (var anim in animNames)
                    {
                        if (anim.Equals(currentAnimOverride.animName)) break;
                        animIndex++;
                    }

                    Debug.Log("Animation Clip Count: " + animNames.Count);
                    animIndex = EditorGUILayout.Popup("Animation Clip: ", animIndex, animNames.ToArray());

                    if (animIndex >= animNames.Count) animIndex = 0;
                    
                    currentAnimOverride.animName = animNames[animIndex];

                    currentAnimOverride.condition = EditorGUILayout.ObjectField("Condition: ", currentAnimOverride.condition,
                        typeof(Condition)) as Condition;

                    EditorGUIUtility.labelWidth = 0;
                    GUILayout.EndHorizontal();
                    
                    //GUILayout.Label("");
                }

                if (deleteEvent > -1)
                {
                    animationConditions.RemoveAt(deleteEvent);
                } //currentCharacterState.events.RemoveAt(deleteEvent); }

                if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(35)))
                {
                    animationConditions.Add(new AnimationConditionOverrides());
                }

                GUILayout.Label("");
            }
        }
    }
    
    
    public void GenerateParameterFields(StateEventObject _event, ref List<GenericValueWrapper> paramVals, ref CharacterData data)
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

            EditorGUIUtility.wideMode = true;
            //EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - 212;
            var origFont = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = FontStyle.BoldAndItalic;
            

            Rect curveRect = new Rect(0, -1, 1, 2);
            
            switch (true)
            {
                case true when param.Item1 == typeof(float):
                    EditorGUIUtility.fieldWidth = 50;
                    paramVals[i].Value = EditorGUILayout.FloatField(param.Item2, (float) paramVals[i].Value);
                    paramVals[i].floatVal = (float) paramVals[i].Value;
                    break;
                case true when param.Item1 == typeof(int):
                    EditorGUIUtility.fieldWidth = 50;
                    paramVals[i].Value = EditorGUILayout.IntField(param.Item2, (int) paramVals[i].Value);
                    paramVals[i].intVal = (int) paramVals[i].Value;
                    break;
                case true when param.Item1 == typeof(bool): 
                    paramVals[i].Value = EditorGUILayout.Toggle(param.Item2, (bool) paramVals[i].Value);
                    paramVals[i].boolVal = (bool) paramVals[i].Value;
                    break;
                case true when param.Item1 == typeof(Vector3):
                    EditorGUIUtility.labelWidth = 100;
                    EditorGUIUtility.fieldWidth = 250;
                    paramVals[i].Value = EditorGUILayout.Vector3Field(param.Item2, (Vector3) paramVals[i].Value);
                    paramVals[i].vec3Val = (Vector3) paramVals[i].Value;
                    break;
                case true when param.Item1 == typeof(Vector2): 
                    paramVals[i].Value = EditorGUILayout.Vector2Field(param.Item2, (Vector2) paramVals[i].Value);
                    paramVals[i].vec2Val = (Vector2) paramVals[i].Value;
                    break;
                case true when param.Item1 == typeof(AnimationCurve): 
                    paramVals[i].Value = EditorGUILayout.CurveField(param.Item2, (AnimationCurve) paramVals[i].Value, Color.green, curveRect);
                    paramVals[i].curveVal = (AnimationCurve) paramVals[i].Value;
                    break;
                case true when param.Item1 == typeof(Texture2D): 
                    paramVals[i].Value = EditorGUILayout.ObjectField(param.Item2, (Texture2D) paramVals[i].Value, typeof(Texture2D));
                    paramVals[i].textureVal = (Texture2D) paramVals[i].Value;
                    break;
                case true when param.Item1 == typeof(VoidEvent):
                    paramVals[i].Value = EditorGUILayout.ObjectField(param.Item2, (VoidEvent) paramVals[i].Value, typeof(VoidEvent));
                    paramVals[i].voidEventVal = (VoidEvent) paramVals[i].Value;
                    break;
                default:
                    GUILayout.Label("TYPE CURRENTLY NOT SUPPORTED: " + param.Item1);
                    break;
            }

            EditorGUIUtility.labelWidth = 0;
            EditorGUIUtility.fieldWidth = 0;
            
            EditorStyles.label.fontStyle = origFont;
            EditorGUILayout.LabelField("", GUI.skin.verticalSlider, GUILayout.Width(5f));
            
            EditorUtility.SetDirty(data);
            i++;
        }

    }
    
}
