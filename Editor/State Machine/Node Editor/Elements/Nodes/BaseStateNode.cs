using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class BaseStateNode : Node
{
    public InstanceID nodeID;
    public StateMachineGraphView graphView;
    public StateInstance stateData;

    public FSMGroup group;
    public StyleSheet extensionStyle;

    protected CharacterData _localCharData;
    
    public StatePort inputPort;
    public StatePort outputPort;

    public List<ConditionNode> fromConditionNodes;
    public List<Edge> fromInterruptPorts;

    // This is temporary since im getting issues with some references
    public void SetCharacterData()
    {
        _localCharData = graphView.charData;
    }

    public virtual void Initialize(StateMachineGraphView graphView_, Vector2 position)
    {
        graphView = graphView_;
        SetCharacterData();

        stateData = new StateInstance(new InstanceID());
        group = graphView_.groupedNodes.ContainsKey(this) ? graphView_.groupedNodes[this] : null;
        nodeID = stateData.ID;

        fromConditionNodes = new List<ConditionNode>();
        fromInterruptPorts = new List<Edge>();
        
        SetPosition(new Rect(position, Vector2.zero));
    }

    public virtual void Initialize(StateMachineGraphView graphView_, StateInstance state)
    {
        graphView = graphView_;
        stateData = state;
        nodeID = stateData.ID;
        group = graphView_.groupedNodes.ContainsKey(this) ? graphView_.groupedNodes[this] : null;
        fromConditionNodes = new List<ConditionNode>();
        fromInterruptPorts = new List<Edge>();

        SetPosition(new Rect(state.graphPosition, Vector2.zero));
    }
    
    public virtual void Draw()
    {
        extensionStyle = Resources.Load<StyleSheet>("ExtensionContainerColor");
        if (!graphView.stateNodeLookUp.ContainsKey(nodeID)) graphView.stateNodeLookUp[nodeID] = this;
        /* Input Port Initialization, passes through an int of its previous (Might only be needed for output ports) */

        inputPort = ElementUtilities.CreateStatePort("",Port.Capacity.Multi, Direction.Input);
        //inputPort.AddManipulator(new EdgeConnector<Edge>(new StateEdgeConnectorListener()));

        inputPort.OnStopEdgeDragging();
        
        inputContainer.Add(inputPort);
        
    }

    public virtual void DrawExtension()
    {
        styleSheets.Add(extensionStyle);
        /* Extensions Container - For the Base Node it's the Button/Direction Input & State */

        VisualElement customInputContainer = new VisualElement();

        Foldout inputFoldout = ElementUtilities.CreateFoldout("Input");

        // Don't draw inputs since we're currently not associated with a group
        if (@group == null) return;

        if (@group.stateMachineData.inputData == null)
        {
            @group.stateMachineData.inputData = Helpers.GetInputData();
        }
        
        if (graphView.charData is PlayableCharacterData)
        {
            DropdownField buttonField = new DropdownField()
            {
                label = "Button",
                choices = @group.stateMachineData.inputData.GetRawInputNames().ToList(),
                index = stateData.command.input
            };

            DropdownField directionField = new DropdownField()
            {
                label = "Direction",
                choices = @group.stateMachineData.inputData.GetMotionCommandNames().ToList(),
                index = stateData.command.motionCommand
            };

            buttonField.RegisterValueChangedCallback((evt) =>
            {
                stateData.command.input = buttonField.choices.FindIndex(pred => pred.Contains(evt.newValue));
                if (_localCharData == null && graphView.charData != null) _localCharData = graphView.charData;
                EditorUtility.SetDirty(_localCharData);
            });
            directionField.RegisterValueChangedCallback((evt) =>
            {
                stateData.command.motionCommand =
                    directionField.choices.FindIndex(pred => pred.Equals(evt.newValue));
                Debug.Log("Motion Command Index!: " + stateData.command.motionCommand);
                EditorUtility.SetDirty(_localCharData);
            });

            //SerializedObject input = new SerializedObject(stateData.command.input);
            //directionField.bindingPath = "stateData.command.motionCommand";
            //buttonField.Bind((SerializedObject) stateData.command.input);

            inputFoldout.Add(buttonField);
            if (@group.stateMachineData.inputData.GetMotionCommandNames().Length != 0)
                inputFoldout.Add(directionField);

            customInputContainer.Add(inputFoldout);
        }
        else if (graphView.charData is AICharacterData AIData)
        {
            FloatField probability = new FloatField()
            {
                label = "Probability",
                value = stateData.probability
            };
            
            probability.RegisterValueChangedCallback((evt) =>
            {
                stateData.probability = Math.Clamp(evt.newValue, 0, 1);
                probability.value = stateData.probability;
                EditorUtility.SetDirty(_localCharData);
            });

            customInputContainer.Add(probability);

        }

        extensionContainer.Add(customInputContainer);
    }

    public virtual void UnDraw()
    {
        extensionContainer.Clear();
        styleSheets.Remove(extensionStyle);
    }

}

public class StateEdgeConnectorListener : IEdgeConnectorListener
{
    public void OnDrop(GraphView graphView, Edge edge)
    {
        if (edge.output.node is StateNode stateNode && edge.input.node is BaseStateNode toNode)
        {
            if (stateNode.group != toNode.group || stateNode.group == null)
            {
                stateNode.outputPort.Disconnect(edge);
                toNode.inputPort.Disconnect(edge);
            }
            else
                stateNode.ConnectNode((BaseStateNode) edge.input.node, edge);
            
            EditorUtility.SetDirty(stateNode.graphView.charData);
        }
        else Debug.Log("Interrupt Port Connection Recognized!");

    }
    
    public void OnDropOutsidePort(Edge edge, Vector2 pos)
    {
        Debug.Log("Edge Dropped Outside");
    }
}

