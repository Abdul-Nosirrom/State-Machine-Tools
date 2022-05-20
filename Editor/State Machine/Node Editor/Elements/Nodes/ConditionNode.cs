using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


public class ConditionNode : Node
{
    public CharacterData _localCharData;
    
    public InstanceID nodeID;
    public StateMachineGraphView graphView;
    public BaseStateNode fromState;
    public BaseStateNode toState;
    public TransitionCondition conditions;

    public ConditionPort inputPort;
    public ConditionPort outputPort;

    private Foldout conditionFoldout;
    
    public int drawCount;

    // This is temporary since im getting issues with some references
    public void SetCharacterData()
    {
        _localCharData = DataManager.Instance.characterData[DataManager.Instance.currentCharacterEditorIndex];
    }
    
    public void Initialize(BaseStateNode from, BaseStateNode to, Vector2 position)
    {
        SetCharacterData();
        
        fromState = from;
        toState = to;
        
        from.stateData.AddFollowUp(to.stateData);

        // Proper Indexing Needed Here
        foreach (var followUp in from.stateData.followUps)
        {

            if (followUp.Key.Equals(to.nodeID))
            {
                conditions = followUp.Value;
                break;
            }
        }

        conditions.graphPos = position;
        
        SetPosition(new Rect(position, Vector2.zero));
        title = "Conditions";
        drawCount = 0;
    }

    public void Draw()
    {
        /* Container Title Is State Name - Set Up by children*/
        //TextField title = ElementUtilities.CreateTextField("Condition");
        //titleContainer.Insert(0, title);
        
        
        /* Input Port Initialization, passes through an int of its previous (Might only be needed for output ports) */
        inputPort = ElementUtilities.CreateConditionPort("", Port.Capacity.Single, Direction.Input);
        inputContainer.Add(inputPort);
        
        outputPort = ElementUtilities.CreateConditionPort("", Port.Capacity.Single, Direction.Output);
        outputContainer.Add(outputPort);
        
        //inputPort.AddManipulator(new EdgeConnector<Edge>(new StateEdgeConnectorListener()));
        //outputPort.AddManipulator(new EdgeConnector<Edge>(new StateEdgeConnectorListener()));

        
        /* Set Up Interrupts Drop Down */
        conditionFoldout = ElementUtilities.CreateFoldout("Conditions");
        // Initialize foldout to not be folded
        conditionFoldout.value = false;
        
        Button addInterruptButton = ElementUtilities.CreateButton("Add Condition", () =>
        {
            VisualElement conditionContainer = CreateConditionElement(new Conditions());
            conditionFoldout.Add(conditionContainer);
        });
        
        conditionFoldout.Add(addInterruptButton);

        for (int i = conditions.conditionsList.Count - 1; i >= 0; i--)
        {
            VisualElement conditionContainer = CreateConditionElement(conditions.conditionsList[i]);
            if (conditionContainer == null) continue;
            conditionFoldout.Add(conditionContainer);
        }
        
        extensionContainer.Insert(0, conditionFoldout);
 
        RefreshExpandedState();
        RefreshPorts();

    }

    private VisualElement CreateConditionElement(Conditions condition)
    {
        VisualElement conditionContainer = new VisualElement();

        conditionContainer.style.flexDirection = FlexDirection.Row;
        
        if (!conditions.conditionsList.Contains(condition)) conditions.conditionsList.Add(condition);
        
        Button deleteCondition = ElementUtilities.CreateButton("X", () =>
        {
            conditions.conditionsList.Remove(condition);
            
            conditionFoldout.Remove(conditionContainer);
            
            EditorUtility.SetDirty(_localCharData);
        });
        
        ObjectField conditionsField = new ObjectField()
        {
            objectType = typeof(Condition), 
            value = condition.condition
        };

        conditionsField.RegisterValueChangedCallback((evt) =>
        {
            condition.condition = (Condition) evt.newValue;
            EditorUtility.SetDirty(_localCharData);
        });
        
        conditionContainer.Add(deleteCondition);
        conditionContainer.Add(conditionsField);
        


        return conditionContainer;
    }

}