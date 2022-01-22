using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



public class ChainEditorWindow : EditorWindow
{
    [MenuItem("Window/Movelist Editor")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(ChainEditorWindow),false, "Movelist Editor");
    }

    
    public int currentChainStep;


    ActionData dataAsset;
    private Character coreData;
    
    Vector2 scrollView;
    int sizer = 0;
    int sizerStep = 30;
    Vector2 xButton = new Vector2(20, 20);

    
    //public string[] lineTypes = new string[] { "Center", "End to End", "End to End Bezier" };
    int drawBase = -1; //-1: Draw Base and Followups, 0: DON'T Draw Base and Followups
    bool drawBaseToggle;

    
    private void OnGUI()
    {
        
        //GUI.DrawTexture(new Rect(0,0,maxSize.x,maxSize.y), Texture2D.blackTexture,ScaleMode.StretchToFill);

        string guid = AssetDatabase.FindAssets("t: ActionData")[0];
            
        dataAsset = AssetDatabase.LoadAssetAtPath<ActionData>(AssetDatabase.GUIDToAssetPath(guid)); 
        //GUILayout.Label("ASSET NUM " + j + " " + (allData == null) + " " + allData[0-].GetStateNames()[2]);
    
        // Character Select Label
        coreData = dataAsset.characters[dataAsset.currentCharacterIndex];
        // Character Name
        using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            //GUILayout.Label("");


            GUILayout.BeginHorizontal();
            dataAsset.currentCharacterIndex =
                EditorGUILayout.Popup(dataAsset.currentCharacterIndex, dataAsset.GetCharacterNames());
            
            

            coreData = dataAsset.characters[dataAsset.currentCharacterIndex];



            GUILayout.EndHorizontal();
        }
        ////////////////////////////////////////////////////////////////
        
        // Move List Select
        MoveList currentMovelist = coreData.moveLists[coreData.currentMoveListIndex];
        
        GUILayout.Label("");

        currentMovelist.name = GUILayout.TextField(currentMovelist.name);

        coreData.currentMoveListIndex = Mathf.Clamp(coreData.currentMoveListIndex, 0, coreData.moveLists.Count - 1);
        GUILayout.BeginHorizontal();
        coreData.currentMoveListIndex = GUILayout.Toolbar(coreData.currentMoveListIndex, dataAsset.GetMoveListNames());
      

        if (GUILayout.Button("New Move List", GUILayout.Width(175))) { coreData.moveLists.Add(new MoveList()); }

        if (GUILayout.Button("Remove Move List", GUILayout.Width(175)))
        {
            if (coreData.moveLists.Count > 2) coreData.moveLists.RemoveAt(coreData.currentMoveListIndex);
        }
        GUILayout.EndHorizontal();
        //////////////////////////////////////////////////////////////////


        CommandState currentCommandStateObject = currentMovelist.commandStates[coreData.currentCommandStateIndex];

        coreData.currentCommandStateIndex = Mathf.Clamp(coreData.currentCommandStateIndex, 0, currentMovelist.commandStates.Count - 1);
        GUILayout.BeginHorizontal();
        coreData.currentCommandStateIndex = GUILayout.Toolbar(coreData.currentCommandStateIndex, dataAsset.GetCommandStateNames());
        

        if (GUILayout.Button("New Command State", GUILayout.Width(175))) { currentMovelist.commandStates.Add(new CommandState()); }
        GUILayout.EndHorizontal();
        currentCommandStateObject.stateName = GUILayout.TextField(currentCommandStateObject.stateName, GUILayout.Width(200));
        //coreData.commandStates[currentCommandState].stateName = GUI.TextField(new Rect(0, 0, 300, 50), coreData.commandStates[currentCommandState].stateName);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Command", GUILayout.Width(120)))
        {
            if (currentCommandStateObject.commandSteps == null) { currentCommandStateObject.commandSteps = new List<CommandStep>(); }
            currentCommandStateObject.AddCommandStep();
            currentCommandStateObject.CleanUpBaseState();
            
            //coreData.commandStates[currentCommandState].chainSteps.Add(new ChainStep(coreData.commandStates[currentCommandState].chainSteps.Count));
        }
        drawBaseToggle = GUILayout.Toggle(drawBaseToggle, "Draw Base Node", EditorStyles.miniButton, GUILayout.Width(150));
        if (drawBaseToggle) { drawBase = -1; } else { drawBase = 0; }
        GUILayout.EndHorizontal();
        scrollView = EditorGUILayout.BeginScrollView(scrollView);

        GUILayout.Label("", GUILayout.Height(2000), GUILayout.Width(2000));

        //draw your nodes here

        
        Handles.BeginGUI();
        GUI.backgroundColor = Color.red;
        int sCounter = 0;

        //foreach (CommandStep s in coreData.commandStates[currentCommandState].commandSteps)
        foreach (CommandStep s in currentCommandStateObject.commandSteps)
        {
            if (sCounter > drawBase)
            {
                int deleteMe = -1;
                int fCounter = 0;
                foreach (int f in s.followUps)
                {


                    CommandStep t = currentCommandStateObject.commandSteps[f];

                    if (t.activated)
                    {
                        Handles.DrawBezier(
                                new Vector2(s.myRect.xMax - 2f, s.myRect.center.y),
                                new Vector2(t.myRect.xMin + 2f, t.myRect.center.y),
                                new Vector2(s.myRect.xMax + 30f, s.myRect.center.y),
                                new Vector2(t.myRect.xMin - 30f, t.myRect.center.y),
                                Color.white, null, 3f);
                        

                        if (GUI.Button(new Rect((t.myRect.center + s.myRect.center) * 0.5f + (xButton * -0.5f), xButton), "X"))
                        {
                            deleteMe = fCounter;
                        }
                    }
                    fCounter++;
                }
                if (deleteMe > -1) { s.followUps.RemoveAt(deleteMe); currentCommandStateObject.CleanUpBaseState(); }
            }
            sCounter++;
        }

        Handles.EndGUI();


        BeginWindows();
        sizerStep = 30;
        //GUI.backgroundColor = Color.black;
        int cCounter = 0;
        foreach (CommandStep c in currentCommandStateObject.commandSteps)
        {
            
            if (c.activated && cCounter > drawBase)
            {
                c.myRect = GUI.Window(c.idIndex, c.myRect, WindowFunction, "", EditorStyles.miniButton);
            }
            cCounter++;
        }
        
        EndWindows();
        EditorGUILayout.EndScrollView();
        EditorUtility.SetDirty(dataAsset);
    }
 // THIS PART DEALS WITH DRAWING AND CONNECTING THE NODES
    void WindowFunction(int windowID)
    {
        GUI.backgroundColor = Color.cyan;
        MoveList currentMovelist = coreData.moveLists[coreData.currentMoveListIndex];
        CommandState currentCommandStateObject = currentMovelist.commandStates[coreData.currentCommandStateIndex];

        if (coreData.currentCommandStateIndex >= currentMovelist.commandStates.Count) { coreData.currentCommandStateIndex = 0; }
        if (windowID >= currentCommandStateObject.commandSteps.Count) { return; }
        currentCommandStateObject.commandSteps[windowID].myRect.width = 175;
        currentCommandStateObject.commandSteps[windowID].myRect.height = 50;
        
        EditorGUI.DrawRect(new Rect(0, 0, 270, 100), Color.grey);
        
        EditorGUI.LabelField(new Rect(6, 7, 35, 20), windowID.ToString());
        currentCommandStateObject.commandSteps[windowID].command.motionCommand =
            EditorGUI.IntPopup(new Rect(25, 5, 50, 20), currentCommandStateObject.commandSteps[windowID].command.motionCommand, dataAsset.GetMotionCommandNames(), null, EditorStyles.miniButtonLeft);

        currentCommandStateObject.commandSteps[windowID].command.input =
            EditorGUI.IntPopup(new Rect(75, 5, 65, 20), currentCommandStateObject.commandSteps[windowID].command.input, dataAsset.GetRawInputNames(), null, EditorStyles.miniButtonMid);
        currentCommandStateObject.commandSteps[windowID].command.state =
            EditorGUI.IntPopup(new Rect(40, 26, 100, 20), currentCommandStateObject.commandSteps[windowID].command.state, dataAsset.GetStateNames(), null, EditorStyles.miniButton);

        currentCommandStateObject.commandSteps[windowID].priority =
           EditorGUI.IntField(new Rect(6, 26, 20, 20), currentCommandStateObject.commandSteps[windowID].priority);

        //currentCommandStateObject.commandSteps[windowID].conditions.holdButton =
        //    EditorGUI.Toggle(new Rect(150, 30, 10, 10), currentCommandStateObject.commandSteps[windowID].conditions.holdButton);

        if (GUI.Button(new Rect(150,30,10,10), String.Empty))
        {
            ConditionsWindow(currentCommandStateObject.commandSteps[windowID]);
        }
        
        int nextFollowup = -1;
        nextFollowup = EditorGUI.IntPopup(new Rect(150, 5, 21, 20), nextFollowup, dataAsset.GetFollowUpNames(coreData.currentCommandStateIndex, true), null, EditorStyles.miniButton);

        if(nextFollowup != -1)
        {
            if (currentCommandStateObject.commandSteps.Count > 0)
            {
                if (nextFollowup >= currentCommandStateObject.commandSteps.Count + 1)
                {
                    currentCommandStateObject.RemoveChainCommands(windowID);

                }
                else if(nextFollowup >= currentCommandStateObject.commandSteps.Count)
                {
                    CommandStep nextCommand = currentCommandStateObject.AddCommandStep();
                    nextCommand.myRect.x = currentCommandStateObject.commandSteps[windowID].myRect.xMax + 40f;
                    nextCommand.myRect.y = currentCommandStateObject.commandSteps[windowID].myRect.center.y - 15f;
                    nextCommand.command.input = currentCommandStateObject.commandSteps[windowID].command.input;
                    nextCommand.command.state = currentCommandStateObject.commandSteps[windowID].command.state;

                    currentCommandStateObject.commandSteps[windowID].AddFollowUp(nextCommand.idIndex);
                    
                }
                else { currentCommandStateObject.commandSteps[windowID].AddFollowUp(nextFollowup); }
            }
            else
            {
                currentCommandStateObject.commandSteps[windowID].AddFollowUp(nextFollowup);
            }
            currentCommandStateObject.CleanUpBaseState();
        }

        if ((Event.current.button == 0) && (Event.current.type == EventType.MouseDown))
        {
            currentChainStep = windowID;
        }
        
        GUI.DragWindow();

    }

    void ConditionsWindow(CommandStep CS)
    {
        StateConditionEditor window = ScriptableObject.CreateInstance(typeof(StateConditionEditor)) as StateConditionEditor;
        window.titleContent = new GUIContent("Conditions");
        window.SetCommandStep(ref dataAsset, ref CS, 0, 0, 0);
        window.ShowUtility();
    }

}



