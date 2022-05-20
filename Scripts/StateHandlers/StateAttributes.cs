using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Random = System.Random;

#region Character Classes
/*
 * To consolidate all FSM into little character containers in order to simplify state management
 */
[System.Serializable]
public class Character
{

    public string name;
    
    // Testing new way of calling in animations, to automate some things
    public RuntimeAnimatorController animator;
    [HideInInspector] public Texture2D characterThumbnail;
    
    public List<CharacterState> characterStates;
    public List<EventScript> eventScripts;

    
    public List<StateMachine> stateMachines;
    
    // SFX and VFX associations
    public List<GameObject> globalPrefabs;
    
    // Editor Data fields
    [HideInInspector] public int currentScriptIndex;
    [HideInInspector] public int currentStateIndex;
    [HideInInspector] public int currentStateMachineIndex;
    
    public Character()
    {
        characterStates = new List<CharacterState>();
        eventScripts = new List<EventScript>();
        stateMachines = new List<StateMachine>();
        globalPrefabs = new List<GameObject>();
        
        // Initialize Data
        characterStates.Add(new CharacterState());
        eventScripts.Add(new EventScript());
        //stateMachines.Add(new StateMachine());

    }

    public StateMachine GetEntryFSM()
    {
        foreach (var FSM in stateMachines)
        {
            if (FSM.isEntryState) return FSM;
        }

        return null;
    }

    public void SetAsEntryFSM(StateMachine entryFSM)
    {
        foreach (var FSM in stateMachines)
        {
            FSM.isEntryState = false;
        }

        entryFSM.isEntryState = true;
    }
}

public class PlayableCharacter : Character
{
    public new List<StateMachine> stateMachines;
    public PlayableCharacter() : base()
    {
        name = "<NEW PLAYABLE CHARACTER>";
    }
}

public class AICharacter : Character
{
    public new List<StateMachine> stateMachines;

    public AICharacter() : base()
    {
        name = "<NEW AI CHARACTER>";
    }
}

#endregion

[System.Serializable]
public class CharacterState
{
    public string stateName;

    /// <summary>
    /// Animation curve associated with a state definition to allow for quick modifications to the animations speed
    /// to get better gameplay feel if need be.
    /// To keep the animation speed as is, keep this animation curve at a constant 1
    /// </summary>
    public AnimationCurve animCurve;

    [HideInInspector] 
    public int stateIndex;

    public float length;
    
    // Loop for shit like locomotion where it can be interrupted but otherwise remains as is
    public bool loop;
    public float blendRate = 0.1f;

    public List<StateEvent> onStateEnterEvents;
    public List<StateEvent> onStateExitEvents;
    public List<StateEvent> events;
    public List<Attack> attacks;

    public CharacterState()
    {
        stateName = "<NEW STATE>";
        events = new List<StateEvent>();
        onStateEnterEvents = new List<StateEvent>();
        onStateExitEvents = new List<StateEvent>();
        attacks = new List<Attack>();
        attacks.Add(new Attack());
        
        // Set default animation curve to constant
        // The timing here is normalized/ in units of 1 "state length"
        animCurve = AnimationCurve.Constant(0, 1, 1);
    }
    
}

#region State Events

[System.Serializable]
public class StateEvent
{

    public float start;
    public float end;

    public bool active = true;
    
    [IndexedItem(IndexedItemAttribute.IndexedItemType.SCRIPTS)]
    public int script;

    public List<GenericValueWrapper> parameters;

    // Add conditions to event, constrain it to 1 condition only as I don't expect any more to be necessary
    // Example of this, perform an event so long as a button is held (e.g jumping)
    public bool hasCondition;
    public Conditions condition;

    public StateEvent()
    {
        active = true;
        hasCondition = false;
        parameters = new List<GenericValueWrapper>();
        condition = null;
    }

    public List<object> GetParamObjectList()
    {
        List<object> objectParams = new List<object>();
        foreach (var param in parameters)
        {
            objectParams.Add(param.GetValue());
        }

        return objectParams;
    }
}


[System.Serializable]
public class EventScript
{
    [HideInInspector] 
    public int eventIndex;

    public string eventName = "< NEW EVENT SCRIPT >";
    
    public StateEventObject eventScript;
    
    //[HideInInspector, SerializeReference] public List<object> baseParamList;
    [HideInInspector] public List<GenericValueWrapper> baseParamList;
}

#endregion

/// <summary>
/// Associates an input with a state
/// So "Space" is an input command for the state "Jump"
/// </summary>
[System.Serializable]
public class InputCommand
{

    [IndexedItem(IndexedItemAttribute.IndexedItemType.RAW_INPUTS)]
    [HideInInspector] public int input;
    
    [IndexedItem(IndexedItemAttribute.IndexedItemType.MOTION_COMMAND)]
    [HideInInspector] public int motionCommand;

    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int state;
    
}


/// <summary>
/// Defines the states a command will be accepted and conditions
/// Example - a set of command states that will be accepted when grounded, when aerial, etc...
/// CommandStates are "sub-states" in a way to the character state, so if state is grounded, we have
/// commands that can be performed and different ones when in air, on walls, etc...
/// </summary>
[System.Serializable]
public class StateMachine
{
    public string stateName;
    public Vector2 graphPosition;

    /// <summary>
    /// How to handle these is the tricky part. Should keep it to 1 condition per-FSM, but also they shouldn't
    /// overlap (e.g Grounded Vs Air obviously don't overlap)
    /// </summary>
    public Conditions FSMCondition;

    public int priority;    // Priority is set to ensure proper selection of FSM in case of overlapping conditions

    public bool isEntryState;
    //Explicit State
    //public bool explicitState;
    //[IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    //public int state;

    public GenericDictionary<InstanceID, StateInstance> stateInstances;

    // Use this to avoid using the zero index of the above list as a dummy state
    // Put states that arent followups of other states here
    public List<InstanceID> entryStateInstances;

    // Could add a "From Any State" that always exists

    [HideInInspector]
    public GenericDictionary<InstanceID, StateInstance> statesFollowedUp;

    public StateMachine()
    {
        stateInstances = new GenericDictionary<InstanceID, StateInstance>();
        entryStateInstances = new List<InstanceID>();
        stateName = "<NEW COMMAND STATE>";
        graphPosition = Vector2.zero;
        FSMCondition = new Conditions();
        priority = 0;
        isEntryState = false;
    }

    public StateInstance AddState()
    {
        StateInstance nextStep = new StateInstance(new InstanceID());
        nextStep.activated = true;
        stateInstances.Add(nextStep.ID, nextStep);
        return nextStep;
    }

    public void AddExistingState(StateInstance state)
    {
        stateInstances.Add(state.ID, state);
        // New state presumably not connected to anything else at the instance it's added, so:
        entryStateInstances.Add(state.ID);

        state.activated = true;
    }

    public void RemoveState(StateInstance state)
    {
        if (stateInstances.ContainsKey(state.ID))
        {
            stateInstances.Remove(state.ID);
        }

        if (entryStateInstances.Contains(state.ID))
        {
            entryStateInstances.Remove(state.ID);
        }

        foreach (StateInstance states in stateInstances.Values)
        {
            if (states.followUps.ContainsKey(state.ID))
            {
                states.RemoveFollowUp(state.ID);
            }
        }

        state.activated = false;
    }

    public void ResetStateInstanceCounters()
    {
        foreach (var state in stateInstances.Values) state.numTimesEntered = 0;
    }
    
    /// <summary>
    /// Clean up and verify entry state instances
    /// </summary>
    public void CleanUpBaseState()
    {
        // Check for which states are independent (e.g no state has it as a followup) - add those to the entry list
        statesFollowedUp = new GenericDictionary<InstanceID, StateInstance>();

        if (stateInstances.Count == 0)
        {
            if (entryStateInstances.Count != 0) entryStateInstances.Clear();
            return;
        }
        foreach (StateInstance state in stateInstances.Values)
        {
            foreach (InstanceID transition in state.followUps.Keys)
            {
                if (!stateInstances.ContainsKey(transition))
                {
                    Debug.Log("ID Not Found in: " + stateName);
                    Debug.Log("State ID: " + transition.ID);
                }
                if (statesFollowedUp.ContainsKey(stateInstances[transition].ID)) continue;
                
                statesFollowedUp.Add(stateInstances[transition].ID, stateInstances[transition]);
            }
        }

        foreach (StateInstance entryState in stateInstances.Values)
        {
            if (statesFollowedUp.ContainsKey(entryState.ID)) entryStateInstances.Remove(entryState.ID);
            else if (!entryStateInstances.Contains(entryState.ID)) entryStateInstances.Add(entryState.ID);
        }
    }
    
    public void UpdateGraphPosition(Vector2 pos)
    {
        graphPosition = pos;
    }

}

/// <summary>
/// Sub state of the command state
/// </summary>
[System.Serializable]
public class StateInstance
{
    [HideInInspector] public InstanceID ID;
    public Vector2 graphPosition;

    public bool toOtherCommandState;
    public int stateMachineTransition;
    public InstanceID otherStateMachineInstanceID;

    public InputCommand command;

    public bool activated;

    public List<Interrupts> interrupts;

    // A way to limit the number of times a state can be entered while in the state machine
    // Sort of a transient condition
    public bool limitTimesToEnter = false;
    public int numTimesToEnter = 1;
    [HideInInspector] public int numTimesEntered = 0;
    
    // AI Specific parameter
    [Range(0,1)]
    [HideInInspector] public float probability;
    
    public GenericDictionary<InstanceID, TransitionCondition> followUps;

    [Tooltip("Strict refers to a step that cannot be cancelled")]
    public bool strict;
    
    
    // Priority is so if we have directional attacks, we don't instantly move to the state w/
    // the button press and check if the directional input belongs to a valid state
    public int priority;
    public bool AddFollowUp(StateInstance _nextState, TransitionCondition _condition = null)
    {
        if (this == _nextState || followUps.ContainsKey(_nextState.ID)) return false;

        followUps.Add(_nextState.ID, _condition ?? new TransitionCondition());

        return true;
    }

    public void RemoveFollowUp(StateInstance _followUp)
    {
        followUps.Remove(_followUp.ID);
    }

    public void RemoveFollowUp(InstanceID _ID)
    {
        followUps.Remove(_ID);
    }

    public StateInstance(InstanceID _id)
    {
        ID = _id;
        followUps = new GenericDictionary<InstanceID, TransitionCondition>();
        
        command = new InputCommand();
        probability = 1;
        interrupts = new List<Interrupts>();
        
        toOtherCommandState = false;
    }

    public void UpdateGraphPosition(Vector2 pos)
    {
        graphPosition = pos;
    }
    
}

#region State Conditions Classes

public abstract class Condition : ScriptableObject
{
    public string description;
    public abstract bool CheckCondition(StateManager state);
}

[System.Serializable]
public class TransitionCondition
{
    public List<Conditions> conditionsList;
    public Vector2 graphPos;
    
    public TransitionCondition()
    {
        conditionsList = new List<Conditions>();
    }
    
    public bool CheckConditions(StateManager stateManager)
    {
        foreach (Conditions condi in conditionsList)
        {
            if (!condi.condition.CheckCondition(stateManager)) return false;
        }

        return true;
    }
}

[System.Serializable]
public class Conditions
{
    public Condition condition;
}
#endregion

#region State Interrupt Classes

public abstract class Interrupt : ScriptableObject
{
    public string description;
    public abstract bool CheckInterrupt(StateManager stateManager);
}

/// <summary>
/// Interrupts are ABSOLUTE, meaning they supersede everything, if interrupts are satisfied, will transition to its
/// followup state regardless. This should suffice, and hopefully no issues arise due to this
/// </summary>
[System.Serializable] //SERIALIZATION ISSUE FIX PLEASE
public class Interrupts
{
    public Interrupt interrupt;
    public InstanceID followUpState;

    public Interrupts()
    {
        followUpState = new InstanceID();
        followUpState.ID = "";
    }

    public void SetFollowUp(InstanceID followUp)
    {
        followUpState = followUp;
    }

    public bool CheckInterrupts(StateManager stateManager)
    {
        return interrupt.CheckInterrupt(stateManager) || interrupt == null;
    }
}

#endregion


#region Utilities

[System.Serializable]
public class InstanceID : IEquatable<InstanceID>
{
    public string ID;
    public InstanceID()
    {
        ID = Guid.NewGuid().ToString();
    }

    public bool Equals(InstanceID eq)
    {
        return ID.Equals(eq.ID);
    }

    public override int GetHashCode()
    {
        return 0;
    }
}

#endregion

