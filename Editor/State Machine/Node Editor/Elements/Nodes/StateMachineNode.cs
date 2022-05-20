using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;



public class StateMachineNode : BaseStateNode
{
    public List<string> availableStates;
    public List<InstanceID> availableStateInstanceIDs;
    public List<string> availableStateMachines;

    public override void Initialize(StateMachineGraphView graphView_, Vector2 position)
    {
        SetCharacterData();
        base.Initialize(graphView_, position);
        stateData.toOtherCommandState = true;
        
    }

    public override void Draw()
    {
        //styleSheets.Add();
        SetCharacterData();
        
        UpdateAvailableStateMachineChoices();
        
        DropdownField FSMSelectionField = new DropdownField()
        {
            choices = availableStateMachines,
            index = stateData.stateMachineTransition
        };

        UpdateAvailableStateChoices();

        var curStateTransition = GetIDIndex(stateData.otherStateMachineInstanceID);
        DropdownField stateSelectionField = new DropdownField()
        {
            choices = availableStates,
            index = curStateTransition == -1 ? 0 : curStateTransition
        };

        stateSelectionField.RegisterValueChangedCallback((evt) =>
        {
            UpdateAvailableStateMachineChoices();
            UpdateAvailableStateChoices();
            int toIndex = stateSelectionField.choices.FindIndex(pred => pred.Contains(evt.newValue));
            stateData.otherStateMachineInstanceID = _localCharData.character
                .stateMachines[stateData.stateMachineTransition].stateInstances.Keys.ToList()[toIndex];
            stateData.command.state = _localCharData.character
                .stateMachines[stateData.stateMachineTransition].stateInstances[stateData.otherStateMachineInstanceID]
                .command.state;
            
            stateSelectionField.choices = availableStates;
            Debug.Log("Ignore");

            EditorUtility.SetDirty(_localCharData);
        });
        
        // Wanna also update state selection field here, and update available choices
        FSMSelectionField.RegisterValueChangedCallback((evt) =>
        {
            UpdateAvailableStateMachineChoices();
            stateData.stateMachineTransition = FSMSelectionField.choices.FindIndex(pred => pred.Contains(evt.newValue));
            UpdateAvailableStateChoices();
            
            FSMSelectionField.choices = availableStateMachines;
            stateSelectionField.choices = availableStates;
            stateSelectionField.index = 0;
            stateSelectionField.value = availableStates[0];
            
            EditorUtility.SetDirty(_localCharData);
        });
        
        titleContainer.Insert(0, FSMSelectionField);
        titleContainer.Insert(1, stateSelectionField);
        
        
        base.Draw();
        
        
        RefreshExpandedState();
        
    }

    public void UpdateAvailableStateChoices()
    {
        StateMachine newStateMachine = _localCharData.character.stateMachines[stateData.stateMachineTransition];
        
        availableStates = new List<string>(newStateMachine.stateInstances.Count);
        availableStateInstanceIDs = new List<InstanceID>();
        int i = 0;
        foreach (StateInstance step in newStateMachine.stateInstances.Values)
        {
            availableStates.Add(i + ": " + graphView.charData.GetStateNames()[step.command.state]);
            availableStateInstanceIDs.Add(step.ID);
            i++;
        }
    }

    public int GetIDIndex(InstanceID ID)
    {
        int i = 0;
        if (ID == null) return -1;
        foreach (InstanceID storedID in availableStateInstanceIDs)
        {
            if (ID.Equals(storedID)) return i;
            i++;
        }

        return -1;
    }

    public void UpdateAvailableStateMachineChoices()
    {
        availableStateMachines = _localCharData.GetCommandStateNames().ToList();
        // Remove choice corresponding to current state machine? No wait that messes up indices uuuugh
        // Maybe set up an error color? End of the day this doesnt really affect anything and it should work fine 
        // Even if the transition points to the current state machine though.
    }
}