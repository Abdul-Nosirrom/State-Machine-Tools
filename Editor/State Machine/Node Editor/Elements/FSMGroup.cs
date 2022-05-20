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
    private float defaultBorderWidth;

    public FSMGroup(StateMachine stateMachine, Vector2 position)
    {
        defaultBorderWidth = contentContainer.style.borderBottomWidth.value;
        stateMachineData = stateMachine;
        SetPosition(new Rect(position, Vector2.zero));

        stateMachineData.graphPosition = position;
        
        SetCharacterData();
        Draw();
    }
    
    public void SetCharacterData()
    {
        _localCharData = DataManager.Instance.characterData[DataManager.Instance.currentCharacterEditorIndex];
    }

    public void UpdateStateMachine(bool isActive)
    {
        if (isActive) AddToClassList("running");
        else AddToClassList("notrunning");
    }
    
    public void Draw()
    {
        VisualElement ConditionContainer = new VisualElement();

        ObjectField conditionsField = new ObjectField()
        {
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
            label = "Priority",
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

        entryFSMButton.clicked += () =>
        {
            _localCharData.character.SetAsEntryFSM(stateMachineData);
            EditorUtility.SetDirty(_localCharData);
            entryFSMButton.text = "Current Entry FSM";
        };

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
            stateMachineData.CleanUpBaseState();
        }
    }

    public void RemoveStateFromGroup(BaseStateNode node)
    {
        if (node is BaseStateNode stateNode)
        {
            stateMachineData.RemoveState(stateNode.stateData);
            stateMachineData.CleanUpBaseState();
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