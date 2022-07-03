using System.Collections.Generic;
using System.Web.UI.WebControls;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class FSMGroup : Group
{
    public StateMachine stateMachineData;
    public CharacterData _localCharData;
    private Foldout conditionFoldout;
    private StateMachineGraphView _graphView;

    public FSMGroup(StateMachine stateMachine, Vector2 position, StateMachineGraphView graphView)
    {

        stateMachineData = stateMachine;
        _graphView = graphView;
        
        SetPosition(new Rect(position, Vector2.zero));

        stateMachineData.graphPosition = position;
        
        SetCharacterData();
        Draw();
    }
    
    public void SetCharacterData()
    {
        _localCharData = _graphView.charData;
    }

    
    public void Draw()
    {
        VisualElement ConditionContainer = new VisualElement();
        

        ObjectField inputMapField = new ObjectField()
        {
            label = "Input Map:",
            objectType = typeof(InputData),
            value = stateMachineData.inputData
        };

        inputMapField.RegisterValueChangedCallback(evt =>
        {
            // Assign our input map
            if (stateMachineData.inputData == null || !evt.newValue.Equals(stateMachineData.inputData.inputActionMap))
            {
                stateMachineData.inputData = evt.newValue as InputData;
                stateMachineData.ResetStateInputs();
                foreach (var stateNodeID in stateMachineData.stateInstances.Keys)
                {
                    var stateNode = _graphView.stateNodeLookUp[stateNodeID];
                    stateNode.UnDraw();
                    stateNode.DrawExtension();
                }
            }
            EditorUtility.SetDirty(_localCharData);
        });

        ObjectField conditionsField = new ObjectField()
        {
            label = "State Machine Condition:",
            objectType = typeof(Condition),
            value = stateMachineData.FSMCondition.condition
        };

        conditionsField.RegisterValueChangedCallback((evt) =>
        {
            stateMachineData.FSMCondition.condition = (Condition) evt.newValue;
            EditorUtility.SetDirty(_localCharData);
        });

        IntegerField priorityField = new IntegerField()
        {
            label = "Priority:",
            value = stateMachineData.priority
        };
        
        priorityField.RegisterValueChangedCallback((evt) =>
        {
            stateMachineData.priority = evt.newValue;
            EditorUtility.SetDirty(_localCharData);
        });

        ToolbarButton entryFSMButton = new ToolbarButton()
        {
            text = stateMachineData.isEntryState ? "Current Entry FSM" : "Set As Entry FSM"
        };
        
        if (stateMachineData.isEntryState) entryFSMButton.style.color = Color.red;

        entryFSMButton.clicked += () =>
        {
            entryFSMButton.style.color = Color.red;
            _localCharData.character.SetAsEntryFSM(stateMachineData);
            EditorUtility.SetDirty(_localCharData);
            entryFSMButton.text = "Current Entry FSM";
        };

        if (_localCharData is PlayableCharacterData)
            ConditionContainer.Add(inputMapField);
        ConditionContainer.Add(conditionsField);
        ConditionContainer.Add(priorityField);
        ConditionContainer.Add(entryFSMButton);
        this.headerContainer.Add(ConditionContainer);
        
    }


    public void AddStateToGroup(BaseStateNode node)
    {
        if (node is BaseStateNode stateNode)
        {
            stateMachineData.AddExistingState(stateNode.stateData);
        }
    }

    public void RemoveStateFromGroup(BaseStateNode node)
    {
        if (node is BaseStateNode stateNode)
        {
            stateMachineData.RemoveState(stateNode.stateData);
        }
    }

    public void UpdateElementPositions()
    {
        foreach (var element in this.containedElements)
        {
            if (element is BaseStateNode stateNode)
            {
                stateNode.stateData.graphPosition = stateNode.GetPosition().position;
            }
            else if (element is ConditionNode condiNode)
            {
                condiNode.conditions.graphPos = condiNode.GetPosition().position;
            }
        }
    }
}