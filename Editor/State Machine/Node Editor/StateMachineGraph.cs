using System;
using System.Collections;
using System.Linq;
using UnityEditor;

using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;

public class StateMachineGraph : EditorWindow
{
    private StateMachineGraphView _graphView;
    
    public static CharacterData charData;
    public static Character character;
    //private StateMachine _currentStateMachineMachine;
    
    // Callback to open editor when asset is double clicked
    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        if (Selection.activeObject is CharacterData characterSelected)
        {
            OpenStateMachineWindow();
            charData = characterSelected;
            character = charData.character;
            DataManager.Instance.currentCharacterEditorIndex =
                DataManager.Instance.GetCharacterNames().FindIndex(pred => pred.Contains(character.name));
            return true;
        }

        return false;
    }
    
    [MenuItem("State Machine/Node Graph")]
    public static void OpenStateMachineWindow()
    {
        var window = GetWindow<StateMachineGraph>();
        window.titleContent = new GUIContent("State Machine Editor");
    }
    
    // Runtime callback
    private void OnInspectorUpdate()
    {
        if (Application.isPlaying)
        {
            if (Selection.activeGameObject == null) return;
            StateManager stateManager = Selection.activeGameObject.GetComponent<StateManager>();
            if (stateManager)
            {
                OpenGraphEditor(stateManager);
                _graphView?.UpdateNodeStates(stateManager);
            }
        }
        // Remove stylesheets
        else
        {
            _graphView?.ResetStyles();
        }
    }

    public static void OpenGraphEditor(StateManager stateManager)
    {
        // Set open state machine to the selected character
        charData = stateManager.characterData;
        character = charData.character;
        DataManager.Instance.currentCharacterEditorIndex =
            DataManager.Instance.GetCharacterNames().FindIndex(pred => pred.Contains(character.name));

    }

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

        }
        
    }

    private void ConstructGraphView()
    {
        _graphView = new StateMachineGraphView()
        {
            name = "FSM Graph"
        };
        
        _graphView.StretchToParentSize();
        _graphView.InitializeGraph(charData);
        
        rootVisualElement.Add(_graphView);
    }

    private void GenerateToolbar()
    {

        var toolbarTop = new Toolbar();
        var toolbarMid = new Toolbar();
        var toolbarBot = new Toolbar();

        if (DataManager.Instance.currentCharacterEditorIndex > DataManager.Instance.characterData.Count)
            DataManager.Instance.currentCharacterEditorIndex = 0;
        
        var characterSelect = new DropdownField("Character: ", DataManager.Instance.GetCharacterNames(), DataManager.Instance.currentCharacterEditorIndex);

        characterSelect.RegisterValueChangedCallback((evt) =>
        {
            DataManager.Instance.currentCharacterEditorIndex =
                characterSelect.choices.FindIndex(pred => pred.Contains(evt.newValue));
            charData = DataManager.Instance.characterData[DataManager.Instance.currentCharacterEditorIndex];
            character = charData.character;
            _graphView.InitializeGraph(charData);
        });
        

        toolbarTop.Add(characterSelect);
        //.Add(nodeCreateButton);
        
        rootVisualElement.Add(toolbarTop);
       // rootVisualElement.Add(toolbarMid);
        //rootVisualElement.Add(toolbarBot);
    }
    
    void AddStyles()
    {
        StyleSheet styleSheet = (StyleSheet) EditorGUIUtility.Load("StateMachineSystem/FSMVariables.uss");
        
        rootVisualElement.styleSheets.Add(styleSheet);
    }

    private void OnEnable()
    {
        if (DataManager.Instance.characterData == null) DataManager.Instance.ReloadFields();
        if (DataManager.Instance.characterData.Count == 0) return;

        // Initialize character first
        if (DataManager.Instance.currentCharacterEditorIndex > DataManager.Instance.characterData.Count)
            DataManager.Instance.currentCharacterEditorIndex = 0;
        
        charData = DataManager.Instance.characterData[DataManager.Instance.currentCharacterEditorIndex];
        character = charData.character;
        
        ConstructGraphView();
        GenerateToolbar();
    }

    private void OnDisable()
    {
        if (_graphView == null) return;
        rootVisualElement.Remove(_graphView);
        AssetDatabase.SaveAssets();
    }
}