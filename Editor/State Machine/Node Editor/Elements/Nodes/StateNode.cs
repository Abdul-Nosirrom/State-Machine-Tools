﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class StateNode : BaseStateNode
{
    public override void Initialize(StateMachineGraphView graphView_, StateInstance state)
    {
        base.Initialize(graphView_, state);
    }

    private Foldout interruptFoldout;
    private List<InterruptPort> interruptPorts;

    public override void Draw()
    {
        interruptPorts = new List<InterruptPort>();
        // This is apparently being a bit of a bitch?
        DropdownField stateSelectionField = new DropdownField()
        {
            label = "State Select",
            choices = graphView.charData.GetStateNames().ToList(),
            index = stateData.command.state
        };
        
        stateSelectionField.RegisterValueChangedCallback((evt) =>
        {
            stateData.command.state = stateSelectionField.choices.FindIndex(pred => pred.Contains(evt.newValue));
        });

        titleContainer.Insert(0, stateSelectionField);
        
        /* Set Up Output Port, Only For This Node Type */

        outputPort = ElementUtilities.CreateStatePort("Transition", Port.Capacity.Multi);
        outputPort.AddManipulator(new EdgeConnector<Edge>(new StateEdgeConnectorListener()));


        outputContainer.Add(outputPort);

        // Only State Nodes should be limited
        /* Container Title Is State Name - Set Up by children*/
        Toggle toggleLimitorField = new Toggle()
        {
            label = "Limit State Enter",
            value = stateData.limitTimesToEnter
        };

        IntegerField numTimesEnterField = new IntegerField()
        {
            label = "",
            value = stateData.numTimesToEnter
        };

        numTimesEnterField.RegisterValueChangedCallback(evt =>
        {
            stateData.numTimesToEnter = evt.newValue;
        });

        toggleLimitorField.RegisterValueChangedCallback(evt =>
        {
            stateData.limitTimesToEnter = evt.newValue;
            toggleLimitorField.Add(numTimesEnterField);
            if (evt.newValue) toggleLimitorField.Add(numTimesEnterField);
            else if (toggleLimitorField.Contains(numTimesEnterField)) toggleLimitorField.Remove(numTimesEnterField);
        });

        //extensionContainer.style.flexDirection = FlexDirection.Row;
        if (toggleLimitorField.value) toggleLimitorField.Add(numTimesEnterField);
        extensionContainer.Insert(0, toggleLimitorField);
        //extensionContainer.style.flexDirection = FlexDirection.Column;
        
        base.Draw();
        
        /* SKIP THIS FOR NOW WHILE SETTING UP INITIALIZATION */
        //  Set Up Interrupts Drop Down 
        interruptFoldout = ElementUtilities.CreateFoldout("Interrupts");
        
        // Set default value to closed if no interrupts
        interruptFoldout.value = stateData.interrupts.Count > 0;
        

            Button addInterruptButton = ElementUtilities.CreateButton("Add Interrupt", () =>
        {
            VisualElement interruptTo = CreateInterruptPort(new Interrupts());
            interruptFoldout.Add(interruptTo);
        });
        
        interruptFoldout.Add(addInterruptButton);

        if (stateData.interrupts == null) stateData.interrupts = new List<Interrupts>();
        
        for (int i = stateData.interrupts.Count - 1; i >= 0; i--)
        {
            VisualElement interruptContainer = AddExistingInterrupt(stateData.interrupts[i]);
            if (interruptContainer == null) continue;
            interruptFoldout.Add(interruptContainer);
        }
        
        extensionContainer.Insert(2, interruptFoldout);
        
            
        RefreshExpandedState();
    }

    private VisualElement CreateInterruptPort(Interrupts interrupts)
    {
        stateData.interrupts.Add(interrupts);

        return AddExistingInterrupt(interrupts);
    }

    public VisualElement AddExistingInterrupt(Interrupts interrupts)
    {
        VisualElement interruptContainer = new VisualElement();
        interruptContainer.style.flexDirection = FlexDirection.Row;
        
        InterruptPort interruptPort = ElementUtilities.CreateInterruptPort(interrupts);
        interruptPorts.Add(interruptPort);
        
        Button deleteInterrupt = ElementUtilities.CreateButton("X", () =>
        {
            if (interruptPort.connected)
            {
                graphView.DeleteElements(interruptPort.connections);
            }

            interruptPorts.Remove(interruptPort);
            stateData.interrupts.Remove(interrupts);
            interruptFoldout.Remove(interruptContainer);
        });
        
        ObjectField interruptField = new ObjectField()
        {
            //name = interrupts.interrupt == null ? "Add Interrupt" : interrupts.interrupt.description,
            objectType = typeof(Interrupt),
            value = interrupts.interrupt
        };

        interruptField.RegisterValueChangedCallback(evt =>
        {
            interruptPort.interrupt.interrupt = evt.newValue as Interrupt;
            EditorUtility.SetDirty(graphView.charData);
        });
        
        interruptContainer.Insert(0, deleteInterrupt);
        interruptContainer.Insert(1, interruptField);
        interruptContainer.Insert(2, interruptPort);

        return interruptContainer;
    }

    public ConditionNode ConnectNode(BaseStateNode node, Edge edge = null, TransitionCondition condition = null)
    {

        ConditionNode condiNode = new ConditionNode();
        Vector2 position = condition == null ? (GetPosition().center + node.GetPosition().center) / 2 : condition.graphPos;
        condiNode.Initialize(this, node, position);
        condiNode.Draw();
                    
        graphView.AddElement(condiNode);
        
        Edge newEdge1 = condiNode.inputPort.ConnectTo(outputPort);
        
        Edge newEdge2 = node.inputPort.ConnectTo(condiNode.outputPort);
                    
        graphView.AddElement(newEdge1);
        graphView.AddElement(newEdge2);
        
        // Clean Up Entry List - Under the assumption that connections can only be made between states apart of the same group
        graphView.groupedNodes[this].stateMachineData.CleanUpBaseState();
        if (!graphView.groupedNodes.ContainsKey(this)) graphView.groupedNodes[this].AddElement(condiNode);
        
        
        if (edge != null)
        {
            this.outputPort.Disconnect(edge);
            node.inputPort.Disconnect(edge);
        }
        
        // Store Condition Node In FollowUp For Data CleanUp When Deleted
        node.fromConditionNodes.Add(condiNode);
        
        return condiNode;
    }

    public void ConnectInterrupts()
    {
        foreach (InterruptPort interruptPort in interruptPorts)
        {
            if (interruptPort.interrupt.followUpState.ID != "")
            {
                BaseStateNode toNode = graphView.stateNodeLookUp[interruptPort.interrupt.followUpState];

                Edge edge = interruptPort.ConnectTo(toNode.inputPort);
                toNode.fromInterruptPorts.Add(edge);
                graphView.AddElement(edge);
            }
        }
    }
}




