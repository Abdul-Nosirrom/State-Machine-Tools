using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.Common.FsNodeReaders.Watcher;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public enum NodeType
{
    State,
    StateMachine
};

public class StateMachineGraphView : GraphView
{
    public CharacterData charData;
    public Character character;
    public List<StateMachine> stateMachines;
    private MiniMap miniMap;
    
    // Keep track of relations between groups and nodes using dictionaries
    public Dictionary<BaseStateNode, FSMGroup> groupedNodes;
    public Dictionary<InstanceID, BaseStateNode> stateNodeLookUp;
    public List<BaseStateNode> ungroupedNodes;

    public StateManager stateManager;
    
    // Runtime stylesheets
    private StyleSheet activeNode, inActiveNode;

    private StyleSheet activeGroup, inActiveGroup;

    public StyleSheet entryState;
    // General Info

    /* Stuff that the user of the editor will be able to do
        - Create State Machine
        - Create State Nodes within the state machine & outside, outside statenodes are not saved
        - Delete State Machines
        - Delete State Nodes
        - Connect two State Nodes in the same state machine
        - Delete connections between two state nodes
        - Connect an interrupt of a state to another state
        - Delete an interrupt connection
    */

    /* Initialization
        - Maybe just start up the state machine groups, look through its entryStateList and build up the graph like that iteratively
    */

    /* When Adding A State Node
        - If it's not added within a group, push it to the list of ungroupedNodes
        - State Instances in ungroupedNodes should not be saved
        - If it's added within a group, push it in the groupedNodes with its group
    */

    /* When deleting a state node
        - If it's ungrouped, just delete the node itself, no clean up would be necessary
        - If it's grouped, we want to ensure the following
            1.) If it's a follow up to anything, we want to be sure to remove that data
            2.) From the node itself, we want to remove the condition node itself as well that's associated w/ the followup
            3.) If any interrupt node connects to it, deal with that
        - A lot of these could be managed by creating a DeleteState function in the StateMachine class
    */

    /* When Adding a state machine
        - Literally just create it and be sure to link the FSMGroups state machine data field with the one in the asset file
    */

    /* When Deleting a state machine
        - Just straight up delete it in the asset file
        - Delete the graph elements and nodes
    */

    /* When connecting two state nodes
        - First check if they're apart of the same group, if they're not, ignore the request
        - If they are, intercept the request and create a condition node between them
    */

    /* When deleting an interrupt edge

    */

    /* When deleting a connection between two states
        - Three possible ways 
            1.) Deleting the edge from FromNode to Condition
            2.) Deleting the condition node
            3.) Deleting the edge from condition node to ToNode
        - When deleting this connection, be sure to go into the state machine data its apart of, and check if you need to add it 
        to the entryStateList
        - However, the RemoveFollowUp may do this automatically I don't remember at the moment.
    */

    public StateMachineGraphView()
    {
        AddManipulators();
        AddGridBackground();
        AddMiniMap();
        
        
        AddStyles();

        AddMiniMapStyles();

        
        // Call Backs
        OnElementsDeleted();
        OnGroupElementsAdded();
        OnGroupElementsRemoved();
        OnGroupRenamed();

        graphViewChanged = OnGraphChange;
    }

    public void InitializeGraph(CharacterData characterData)
    {
        activeNode = Resources.Load<StyleSheet>("ActiveNode");
        inActiveNode = Resources.Load<StyleSheet>("InActiveNode");
        activeGroup = Resources.Load<StyleSheet>("ActiveGroup");
        inActiveGroup = Resources.Load<StyleSheet>("InActiveGroup");

        entryState = Resources.Load<StyleSheet>("EntryState");

        
        if (characterData == null)
        {
            Debug.Log("You Passed in a null character data field");
        }
        
        charData = characterData;
        character = charData.character;
        // Clear the graph if it's not already cleared
        ClearGraph();
        // Set Up Groups First
        
        if (character.stateMachines.Count == 0) return;
        foreach (StateMachine FSM in character.stateMachines)
        {
            AddExistingStateMachine(FSM);
        }
    }

    public void ResetStyles()
    {
        foreach (FSMGroup FSM in groupedNodes.Values)
        {
            if (FSM.styleSheets.Contains(inActiveGroup)) FSM.styleSheets.Remove(inActiveGroup);
            if (FSM.styleSheets.Contains(activeGroup)) FSM.styleSheets.Remove(activeGroup);
        }

        foreach (BaseStateNode stateNode in groupedNodes.Keys)
        {
            if (stateNode.styleSheets.Contains(inActiveNode)) stateNode.styleSheets.Remove(inActiveNode);
            if (stateNode.styleSheets.Contains(activeNode)) stateNode.styleSheets.Remove(activeNode);
        }
    }

    public void UpdateNodeStates(StateManager _stateManager)
    {
        stateManager = _stateManager;

        ResetStyles();
        
        foreach (FSMGroup FSM in groupedNodes.Values)
        {
            if (stateManager.currentStateMachine.stateName.Equals(FSM.stateMachineData.stateName))
            {
                FSM.styleSheets.Add(activeGroup);            
            }
            else
            {
                FSM.styleSheets.Add(inActiveGroup);
            }
        }

        foreach (BaseStateNode stateNode in groupedNodes.Keys)
        {
            if (stateNode.nodeID == stateManager.currentStateInstance.ID)
            {
                stateNode.styleSheets.Add(activeNode);
            }
            else
            {
                stateNode.styleSheets.Add(inActiveNode);
            }
        }
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts = new List<Port>();

        ports.ForEach(port =>
        {
            if (startPort.node == port.node || startPort.direction == port.direction) return;
            if (startPort is InterruptPort && port is ConditionPort) return;
            if (port is InterruptPort && startPort is ConditionPort) return;
            // Add Check that they must be apart of the same state machine ? How to get that data
            
            compatiblePorts.Add(port);
        });
        
        
        return compatiblePorts;
    }

    void AddManipulators()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        UpdateViewTransform(Vector3.zero, new Vector3(1,1,1));
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new ContentZoomer());
        
        this.AddManipulator(CreateNodeContextualMenu("Add State Node", NodeType.State));
        this.AddManipulator(CreateNodeContextualMenu("Add State Machine Transition", NodeType.StateMachine));
        
        this.AddManipulator(CreateStateMachineContextualMenu());
        
    }

    void AddMiniMap()
    {
        miniMap = new MiniMap()
        {
            anchored = true
        };
        
        miniMap.SetPosition(new Rect(15, 50, 200, 180));
        Add(miniMap);
    }

    #region General Editor Menus & Style Initialization
    private IManipulator CreateNodeContextualMenu(string actionTitle, NodeType nodeType)
    {
        ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
            menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent =>
            {
                AddElement(CreateNode(nodeType, actionEvent.eventInfo.mousePosition));
            })
            );

        return contextualMenuManipulator;
    }

    private IManipulator CreateStateMachineContextualMenu()
    {
        ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
            menuEvent => menuEvent.menu.AppendAction("Create State Machine", actionEvent =>
            {
                AddElement(CreateStateMachine(actionEvent.eventInfo.mousePosition));
            })
        ); 
        
        return contextualMenuManipulator;
    }
    
    void AddGridBackground()
    {
        GridBackground gridBackground = new GridBackground();
        gridBackground.StretchToParentSize();
        Insert(0, gridBackground);
    }

    void AddStyles()
    {
        StyleSheet styleSheet = Resources.Load<StyleSheet>("GraphViewStyles");
        
        styleSheets.Add(styleSheet);
    }

    void AddMiniMapStyles()
    {
        StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
        StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));

        miniMap.style.backgroundColor = backgroundColor;
        miniMap.style.borderTopColor = borderColor;
        miniMap.style.borderBottomColor = borderColor;
        miniMap.style.borderRightColor = borderColor;
        miniMap.style.borderLeftColor = borderColor;

    }
    
    #endregion

    #region Element Creation
    public GraphElement CreateStateMachine(Vector2 localMousePos)
    {
        StateMachine newFSM = new StateMachine();
        character.stateMachines.Add(newFSM);
        FSMGroup group = new FSMGroup(newFSM, localMousePos, this);
        newFSM.UpdateGraphPosition(localMousePos);
        
        List<GraphElement> elementsToAdd = new List<GraphElement>();

        foreach (var selectedElement in selection)
        {
            if (selectedElement is BaseStateNode nodeS)
            {
                elementsToAdd.Add(nodeS);
            }
            else if (selectedElement is ConditionNode nodeT)
            {
                elementsToAdd.Add(nodeT);
            }
            else if (selectedElement is FSMGroup) continue;
            
            // When adding stuff here, have to explicitly add element in group
            if (selectedElement is GraphElement GE) group.AddElement(GE);
        }

        // Let the call back of this handle creation and everything to avoid repeated code
        elementsAddedToGroup.Invoke(group, elementsToAdd);
        
        return group;
    }

    public BaseStateNode CreateNode(NodeType nodeType, Vector2 mousePos)
    {
        Type nodeInfo = Type.GetType($"{nodeType}Node");
        BaseStateNode stateNode = (BaseStateNode) Activator.CreateInstance(nodeInfo);
        
        stateNode.Initialize(this, mousePos);
        stateNode.Draw();
        
        AddElement(stateNode);

        if (!groupedNodes.ContainsKey(stateNode)) ungroupedNodes.Add(stateNode);
        
        return stateNode;
    }

    private Port GeneratePort(StateNode node, Direction portDirection, Port.Capacity capacity=Port.Capacity.Multi)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(Void));
    }
    
    #endregion
    
    #region Element Deletion
    public void DeleteStateMachine(FSMGroup group)
    {
        character.stateMachines.Remove(group.stateMachineData);

        RemoveElement(group);
    }

    /// <summary>
    /// Called whenever a connection edge or node is deleted
    /// ConditionNode holds both follow up and previous node information as well as edges that need to be deleted
    /// so it's the only input required to handle all the clean up
    /// </summary>
    /// <param name="condiNode"></param>
    public void DeleteConnection(ConditionNode condiNode)
    {
        // First, collect all relevant data
        BaseStateNode fromNode = (BaseStateNode)condiNode.fromState;
        BaseStateNode toNode = (BaseStateNode)condiNode.toState;

        // Disconnect & Delete Edges
        fromNode.outputPort.Disconnect(condiNode.inputPort.connections.ElementAt(0));
        toNode.inputPort.Disconnect(condiNode.outputPort.connections.ElementAt(0));
        RemoveElement(condiNode.inputPort.connections.ElementAt(0));
        RemoveElement(condiNode.outputPort.connections.ElementAt(0));

        condiNode.inputPort.DisconnectAll();
        condiNode.outputPort.DisconnectAll();
        
        // Clean Up State Data, remove followup
        fromNode.stateData.RemoveFollowUp(toNode.nodeID);
        
        //groupedNodes[fromNode]?.stateMachineData.CleanUpBaseState();
        toNode.fromConditionNodes.Remove(condiNode);
        // Might need to remove edges as well from the graphview
        RemoveElement(condiNode);
    }

    // Meant to pass in an edge outgoing from an interruptPort
    public void DeleteConnection(Edge interruptEdge)
    {
        // Collect relevant data
        //BaseStateNode fromNode = (BaseStateNode) interruptEdge.output.node;
        //BaseStateNode toNode = (BaseStateNode) interruptEdge.input.node;
        
        interruptEdge.output.Disconnect(interruptEdge);
        return;
        // Remove FollowUp ID
        //foreach (Interrupts interrupt in fromNode.stateData.interrupts)
        //{
        //    if (interrupt.followUpState == toNode.nodeID)
        //    {
        //        interrupt.SetFollowUp(null);
        //    }
        //}

    }
    
    
    #endregion

    #region CallBacks

    /// <summary>
    /// Let This Method Handle Any Creations - Specifically State Node Creation or movement (to update data)
    /// </summary>
    /// <param name="change"></param>
    /// <returns></returns>
    private GraphViewChange OnGraphChange(GraphViewChange change)
    {
        List<Edge> edgesToDelete = new List<Edge>();
        
        
        
        if (change.edgesToCreate != null)
        {
            
            foreach (Edge edge in change.edgesToCreate)
            {

                if (edge.input.node is BaseStateNode toNode && edge.output.node is BaseStateNode fromNode)
                {
                    // Dont let modifications be made outside a group
                    if (!(groupedNodes.ContainsKey(toNode) && groupedNodes.ContainsKey(fromNode)))
                    {
                        edgesToDelete.Add(edge);
                        continue;
                    }
                    if (groupedNodes[toNode] != groupedNodes[fromNode])
                    {
                        edgesToDelete.Add(edge);
                        continue;
                    }

                    edgesToDelete.Add(edge);
                }
                
            }

            foreach (Edge edge in edgesToDelete) change.edgesToCreate.Remove(edge);
        }
        
        // Update Stored Node and Group Positions In State Machine Data (For Initialization Later On)
        if (change.movedElements != null)
        {
            foreach (GraphElement e in change.movedElements)
            {
                if (e is FSMGroup stateMachineGroup)
                {
                    stateMachineGroup.stateMachineData.graphPosition = e.GetPosition().position;
                    stateMachineGroup.UpdateElementPositions();
                }
                else if (e is BaseStateNode stateNode) stateNode.stateData.graphPosition = e.GetPosition().position;
                else if (e is ConditionNode condiNode) condiNode.conditions.graphPos = e.GetPosition().position;
                else if (e is Edge edge)
                {
                    Debug.Log("Attempting To Modify An Edge! - Disconnect Condition Node and delete and manage that!!!");
                }
            }
        }

        return change;
    }
    
    
    private void OnElementsDeleted()
    {
        deleteSelection = (operationName, AskUser) =>
        {
            for (int i = selection.Count - 1; i >= 0; i--)
            {
                BaseStateNode stateNode;
                BaseStateNode followUp;
                ConditionNode conditionNode;
                
                if (selection[i] is BaseStateNode node)
                {
                    //currentStateMachineMachine.RemoveState(node.stateData);
                    // Do some cleanup on the DataAsset
                    if (!ungroupedNodes.Contains(node))
                    {
                        for (int c = node.fromConditionNodes.Count - 1; c >= 0; c--)
                        {
                            DeleteConnection(node.fromConditionNodes[c]);
                        }

                        if (node.fromInterruptPorts != null)
                        {
                            for (int j = node.fromInterruptPorts.Count - 1; j >= 0; j--)
                            {
                                // Catch null reference exception if interrupt was already deleted itself
                                try
                                {
                                    DeleteConnection(node.fromInterruptPorts[j]);
                                }
                                catch (NullReferenceException e)
                                {
                                    Debug.Log("Interrupt Already Deleted");
                                    node.fromInterruptPorts.RemoveAt(j);
                                }
                            }                            
                        }

                        foreach (var myInterrupts in node.stateData.interrupts)
                        {
                            if (myInterrupts.followUpState.ID == "") continue;
                            var toNode = stateNodeLookUp[myInterrupts.followUpState];
                            for (int j = toNode.fromInterruptPorts.Count - 1; j >= 0; j--)
                            {
                                BaseStateNode fromNode = toNode.fromInterruptPorts[j].output.node as BaseStateNode;
                                if (fromNode.nodeID.Equals(node.nodeID)) DeleteConnection(toNode.fromInterruptPorts[j]);
                            }
                        }

                        groupedNodes[node]?.stateMachineData.RemoveState(node.stateData);
                        groupedNodes.Remove(node);
                    }
                    RemoveElement(node);
                }
                else if (selection[i] is FSMGroup group)
                {
                    DeleteStateMachine(group);
                }
                
                else if (selection[i] is ConditionNode condiNode)
                {
                    DeleteConnection(condiNode);
                }
                // This is specifically for when you right click and delete from contextual menu btw
                else if (selection[i] is Edge edge)
                {
                    if (edge.input.node is ConditionNode || edge.output.node is ConditionNode)
                    {
                        conditionNode = (ConditionNode) (edge.input.node is ConditionNode ? edge.input.node : edge.output.node);
                        DeleteConnection(conditionNode);
                    }

                    if (edge.output is InterruptPort interruptPort)
                    {
                        // Handle Deletion of interrupt port
                        interruptPort.Disconnect(edge);
                        
                    }
                }
            }
        };
    }

    #region Group CallBacks
    private void OnGroupRenamed()
    {
        groupTitleChanged = (group, newTitle) =>
        {
            FSMGroup stateMachineGroup = (FSMGroup) group;

            stateMachineGroup.title = newTitle;
            stateMachineGroup.stateMachineData.stateName = newTitle;
            
            AssetDatabase.SaveAssets();
        };
    }


    private void OnGroupElementsAdded()
    {
        elementsAddedToGroup = (group, elements) =>
        {
            foreach (var element in elements)
            {
                // If State Node, add to state machine
                if (element is BaseStateNode stateNode)
                {
                    stateNode.group = (FSMGroup) group;
                    stateNode.DrawExtension();
                    groupedNodes[stateNode] = (FSMGroup) group;
                    groupedNodes[stateNode].AddStateToGroup(stateNode);
                    ungroupedNodes.Remove(stateNode);
                }
            }
        };
    }


    /// <summary>
    /// DONT FORGET TO HANDLE INTERRUPTS HERE, DATA ALREADY EXISTS SO JUST CALL ON DISCONNECT
    /// </summary>
    private void OnGroupElementsRemoved()
    {
        List<ConditionNode> stuffToDelete = new List<ConditionNode>();
        List<ConditionNode> stuffDeleted = new List<ConditionNode>();
        
        elementsRemovedFromGroup = (group, elements) =>
        {
            foreach (var element in elements)
            {
                if (element is BaseStateNode stateNode && groupedNodes.ContainsKey(stateNode))
                {

                    // Delete Connections
                    if (stateNode.inputPort.connected)
                    {
                        foreach (var edges in stateNode.inputPort.connections)
                        {
                            if (edges.output.node is ConditionNode condiNode) stuffToDelete.Add(condiNode);
                        }
                    }

                    if (stateNode is StateNode stateNodeExplicit && stateNodeExplicit.outputPort.connected)
                    {
                        foreach (var edges in stateNode.outputPort.connections)
                        {
                            if (edges.input.node is ConditionNode condiNode) stuffToDelete.Add(condiNode);
                        }
                    }

                    if (stuffToDelete.Count != 0)
                    {
                        foreach (ConditionNode condiNode in stuffToDelete)
                        {
                            if (stuffDeleted.Contains(condiNode)) continue;
                            DeleteConnection(condiNode);
                            stuffDeleted.Add(condiNode);
                        }
                    }

                    groupedNodes[stateNode].RemoveStateFromGroup(stateNode);
                    groupedNodes.Remove(stateNode);
                    stateNode.group = null;
                    stateNode.UnDraw();

                    ungroupedNodes.Add(stateNode);
                }
            }
        };
    }
    
    #endregion
    
    #endregion

    #region Existing Group Additions And Removals

    private void AddExistingStateMachine(StateMachine stateMachine)
    {
        FSMGroup group = new FSMGroup(stateMachine, stateMachine.graphPosition, this);
        List<BaseStateNode> nodesInThisGroup = new List<BaseStateNode>();
        groupTitleChanged.Invoke(group, stateMachine.stateName);

        if (stateMachine.stateInstances.Count == 0) goto SKIP_SETUP;
        
        // Create Nodes for all states apart of this FSM
        
        foreach (StateInstance state in stateMachine.stateInstances.Values)
        {
            BaseStateNode stateNode = AddExistingState(state);
            
            if(!group.ContainsElement(stateNode)) group.AddElement(stateNode);

            groupedNodes[stateNode] = group;
            if (ungroupedNodes.Contains(stateNode)) ungroupedNodes.Remove(stateNode);
            stateNodeLookUp[state.ID] = stateNode;
            stateNode.group = group;
            stateNode.DrawExtension();
            

            nodesInThisGroup.Add(stateNode);
        }
        
        // Now set up followups
        foreach (BaseStateNode stateNode in nodesInThisGroup)
        {
            if (stateNode is StateNode && stateNode.stateData.followUps.Count > 0)
                SetUpExistingStateFollowUps((StateNode)stateNode);
            
            // For Setting Up Interrupts
            if (stateNode is StateNode node) SetUpExistingInterrupts(node);

        }
        
        SKIP_SETUP:
        
        AddElement(group);
    }

    private BaseStateNode AddExistingState(StateInstance state)
    {
        BaseStateNode stateNode;
        
        if (state.toOtherCommandState) stateNode = new StateMachineNode();
        else stateNode = new StateNode();
        
        stateNode.Initialize(this, state);
        stateNode.Draw();
        AddElement(stateNode);
        
        return stateNode;
    }

    private void SetUpExistingStateFollowUps(StateNode stateNode)
    {
        // Keys might not be the same in reference, but they are in value - so be sure to pass in the right version to the dictionary
        
        foreach (InstanceID dataFollowUpID in stateNode.stateData.followUps.Keys)
        {
            BaseStateNode followUpNode = stateNodeLookUp[dataFollowUpID];
            ConditionNode condiNode = stateNode.ConnectNode(followUpNode, null, stateNode.stateData.followUps[dataFollowUpID]);
            
            // Set Up CondiNode position
            stateNode.@group.AddElement(condiNode);
            //groupedNodes[stateNode].AddElement(condiNode);
            condiNode.SetPosition(new Rect(condiNode.conditions.graphPos, Vector2.zero));
        }
    }

    private void SetUpExistingInterrupts(StateNode stateNode)
    {
        stateNode.ConnectInterrupts();
    }

    #endregion
    
    #region Graph Cleanup

    public void ClearGraph()
    {
        graphElements.ForEach(graphElements => RemoveElement(graphElements));
        groupedNodes = new Dictionary<BaseStateNode, FSMGroup>();
        stateNodeLookUp = new Dictionary<InstanceID, BaseStateNode>();
        ungroupedNodes = new List<BaseStateNode>();
    }
    
    #endregion
}