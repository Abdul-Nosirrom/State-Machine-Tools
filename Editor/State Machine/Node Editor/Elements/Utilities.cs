using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CustomEdgeConnectorListener : IEdgeConnectorListener
{
    private GraphViewChange m_GraphViewChange;
    private List<Edge> m_EdgesToCreate;
    private List<GraphElement> m_EdgesToDelete;

    public CustomEdgeConnectorListener()
    {
        m_EdgesToCreate = new List<Edge>();
        m_EdgesToDelete = new List<GraphElement>();

        m_GraphViewChange.edgesToCreate = m_EdgesToCreate;
    }

    public void OnDropOutsidePort(Edge edge, Vector2 position) {}
    public void OnDrop(GraphView graphView, Edge edge)
    {
        m_EdgesToCreate.Clear();
        m_EdgesToCreate.Add(edge);

        // We can't just add these edges to delete to the m_GraphViewChange
        // because we want the proper deletion code in GraphView to also
        // be called. Of course, that code (in DeleteElements) also
        // sends a GraphViewChange.
        m_EdgesToDelete.Clear();
        if (edge.input.capacity == Port.Capacity.Single)
            foreach (Edge edgeToDelete in edge.input.connections)
                if (edgeToDelete != edge)
                    m_EdgesToDelete.Add(edgeToDelete);
        if (edge.output.capacity == Port.Capacity.Single)
            foreach (Edge edgeToDelete in edge.output.connections)
                if (edgeToDelete != edge)
                    m_EdgesToDelete.Add(edgeToDelete);
        if (m_EdgesToDelete.Count > 0)
            graphView.DeleteElements(m_EdgesToDelete);

        var edgesToCreate = m_EdgesToCreate;
        if (graphView.graphViewChanged != null)
        {
            edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;
        }

        foreach (Edge e in edgesToCreate)
        {
            graphView.AddElement(e);
            edge.input.Connect(e);
            edge.output.Connect(e);
        }
    }
}