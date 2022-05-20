using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class ConditionPort : Port
{
    protected ConditionPort(String _title, Direction direction = Direction.Input, Capacity capacity = Port.Capacity.Single, Orientation orientation = Orientation.Horizontal) : 
        base(orientation, direction, capacity, typeof(int))
    {
        portName = _title;
    }
    
    public static ConditionPort Create<TEdge>(String _title, Direction direction = Direction.Input, Capacity capacity = Port.Capacity.Single, Orientation orientation = Orientation.Horizontal) where TEdge : Edge, new()
    {
        var connectorListener = new CustomEdgeConnectorListener();
        var port = new ConditionPort(_title, direction, capacity, orientation)
        {
            m_EdgeConnector = new EdgeConnector<TEdge>(connectorListener),
        };
        port.AddManipulator(port.m_EdgeConnector);
        return port;
    }

    
    public override void Connect(Edge edge)
    {
        base.Connect(edge);
    }

    public override void Disconnect(Edge edge)
    {
        base.Disconnect(edge);
    }

    public override void OnStopEdgeDragging()
    {
        base.OnStopEdgeDragging();
    }

}