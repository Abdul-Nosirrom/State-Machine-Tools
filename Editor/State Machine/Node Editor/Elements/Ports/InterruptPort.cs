using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class InterruptPort : Port
{
    public Interrupts interrupt;
    
    protected InterruptPort(Interrupts _interrupt, String _title, Direction direction = Direction.Input, Capacity capacity = Port.Capacity.Single, Orientation orientation = Orientation.Horizontal) : 
        base(orientation, direction, capacity, typeof(int))
    {
        portName = _title;
        interrupt = _interrupt;
    }
    
    public static InterruptPort Create<TEdge>(Interrupts _interrupt, String _title, Direction direction = Direction.Output, Capacity capacity = Port.Capacity.Single, Orientation orientation = Orientation.Horizontal) where TEdge : Edge, new()
    {
        var connectorListener = new InterruptEdgeConnectorListener();
        var port = new InterruptPort(_interrupt, _title, direction, capacity, orientation)
        {
            m_EdgeConnector = new EdgeConnector<TEdge>(connectorListener),
        };
        port.AddManipulator(port.m_EdgeConnector);
        return port;
    }

    
    public override void Connect(Edge edge)
    {

        if (edge.input.node is BaseStateNode interruptFollowUpNode)
        {
            // Add followup to interrupt thing
            BaseStateNode stateNode = (BaseStateNode) edge.input.node;
            stateNode.fromInterruptPorts.Add(edge);
            interrupt.SetFollowUp(stateNode.stateData.ID);
            base.Connect(edge);
        }
        else
        {
            Debug.Log("Do I ever reach this?");
            Remove(edge);
        }
    }

    public override void Disconnect(Edge edge)
    {
        BaseStateNode stateNode = (BaseStateNode) edge.input.node;
        stateNode.fromInterruptPorts.Remove(edge);
        edge.input.Disconnect(edge);
        InstanceID nullID = new InstanceID();
        nullID.ID = "";
        interrupt.SetFollowUp(nullID);
        base.Disconnect(edge);
        m_GraphView.RemoveElement(edge);
    }

    public override void OnStopEdgeDragging()
    {
        base.OnStopEdgeDragging();
    }
}

public class InterruptEdgeConnectorListener : IEdgeConnectorListener
{
    public void OnDrop(GraphView graphView, Edge edge)
    {
        if (edge.output.node is StateNode stateNode && edge.input.node is BaseStateNode toNode)
        {
            if (stateNode.group != toNode.group || stateNode.group == null)
            {
                edge.output.Disconnect(edge);
                toNode.inputPort.Disconnect(edge);
            }
            else
            {
                Debug.Log("Interrupt Connection Being Made!");
                graphView.AddElement(edge);
                edge.output.Connect(edge);
                edge.input.Connect(edge);
            }
            
            EditorUtility.SetDirty(stateNode.graphView.charData);
        }
        Debug.Log("Interrupt Port Connection Recognized!");

    }
    
    public void OnDropOutsidePort(Edge edge, Vector2 pos)
    {
        Debug.Log("Edge Dropped Outside");
    }
}