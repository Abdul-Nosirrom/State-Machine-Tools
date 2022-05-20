
using System;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class StatePort : Port
{
    protected StatePort(String _title, Direction direction = Direction.Input, Capacity capacity = Port.Capacity.Single, Orientation orientation = Orientation.Horizontal) : 
        base(orientation, direction, capacity, typeof(int))
    {
        portName = _title;
    }
    
    public static StatePort Create<TEdge>(String _title, Direction direction = Direction.Input, Capacity capacity = Port.Capacity.Single, Orientation orientation = Orientation.Horizontal) where TEdge : Edge, new()
    {
        var connectorListener = new CustomEdgeConnectorListener();
        var port = new StatePort(_title, direction, capacity, orientation)
        {
            m_EdgeConnector = new EdgeConnector<TEdge>(connectorListener),
        };
        port.AddManipulator(port.m_EdgeConnector);
        return port;
    }

    
    public override void Connect(Edge edge)
    {
        // CHECK IF INITIAILIZATION HAS ALREADY BEEN DONE AND ONLY CONNECTION IS NECESSARY

        if (edge.input is StatePort && edge.output is StatePort)
        {
            // Handle Condition Node? Or Maybe Leave This To The Graph View
        }
        // If this check is satisfied, that means a connection previously made was aletered, so update followUp
        else if (edge.output.node is StateNode newStateNode && edge.input.node is ConditionNode condiNode)
        {
            if (newStateNode.stateData.followUps.ContainsKey(condiNode.toState.nodeID))
            {
                base.Connect(edge);
                return;
            }
            
            // Remove info from CondiNode about fromNode
            StateNode prevStateNode = (StateNode) condiNode.fromState;
            prevStateNode.stateData.RemoveFollowUp(condiNode.toState.stateData);
            
            // Add the followup to the new node
            condiNode.fromState = (BaseStateNode) newStateNode;
            newStateNode.stateData.AddFollowUp(condiNode.toState.stateData, condiNode.conditions);

        }
        else if (edge.input.node is BaseStateNode newFollowUp && edge.output.node is ConditionNode prevCondiNode)
        {
            if (prevCondiNode.fromState.stateData.followUps.ContainsKey(newFollowUp.nodeID))
            {
                base.Connect(edge);
                return;
            }
            
            BaseStateNode prevFollowUp = prevCondiNode.toState;
            prevCondiNode.fromState.stateData.RemoveFollowUp(prevFollowUp.stateData);
            prevCondiNode.fromState.stateData.AddFollowUp(newFollowUp.stateData, prevCondiNode.conditions);
            prevCondiNode.toState = newFollowUp;
        }
        
        base.Connect(edge);
    }


}