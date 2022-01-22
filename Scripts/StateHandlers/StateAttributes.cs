using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;


/*
 * To consolidate all FSM into little character containers in order to simplify state management
 */
[System.Serializable]
public class Character
{
    // The lists below are all attributes associated with a character
    // When it comes to AI, we might change what the character attribute is, probably through inheritance
    // and setting similar names where rawInput overrides with goals or something we'll see
    public string name;
    public List<CharacterState> characterStates;
    public List<EventScript> eventScripts;

    public List<MoveList> moveLists;
    
    // SFX and VFX associations
    public List<GameObject> globalPrefabs;
    
    public int currentMoveListIndex;
    
    // Editor Data fields
    [HideInInspector] public int currentScriptIndex;
    [HideInInspector] public int currentStateIndex;
    [HideInInspector] public int currentCommandStateIndex;

    public Character()
    {
        name = "<NEW CHARACTER>";
        characterStates = new List<CharacterState>();
        eventScripts = new List<EventScript>();
        moveLists = new List<MoveList>();
        globalPrefabs = new List<GameObject>();
        
        // Initialize Data
        characterStates.Add(new CharacterState());
        eventScripts.Add(new EventScript());
        moveLists.Add(new MoveList());
        
        // Initialize indices to zero
        currentScriptIndex = 0;
        currentStateIndex = 0;
        currentMoveListIndex = 0;
        currentCommandStateIndex = 0;
    }
}

[System.Serializable]
public class CharacterState
{
    public string stateName;

    [HideInInspector] 
    public int stateIndex;

    public float length;
    
    // Loop for shit like locomotion where it can be interrupted but otherwise remains as is
    public bool loop;
    public float blendRate = 0.1f;

    public bool groundedReq;
    public bool railReq;
    public bool wallReq;
    
    public List<StateEvent> events;
    public List<Interrupt> interrupts;
    public List<Attack> attacks;

    public CharacterState()
    {
        stateName = "<NEW STATE>";
    }

    public bool ConditionsMet(CharacterStateManager character)
    {
        // CHECK DIFFERENT CONDITIONS LIKE COOLDOWNS/SPECIAL METER/ETC
        // SO far only one check for moves that require you to be grounded
        bool groundCheck = !groundedReq || character.myController.movement.isOnGround;
        bool railCheck = !railReq || character.myController.canRailGrind;
        bool wallCheck = !wallReq || character.myController.validWall;

        return groundCheck && railCheck && wallCheck;
    }

    public int CheckInterrupts(CharacterStateManager character)
    {
        foreach (Interrupt interrupt in interrupts)
        {
            switch (interrupt.type)
            {
                case Interrupt.InterruptTypes.GROUND:
                    if (character.myController.movement.isOnGround)
                        return interrupt.state;
                    break;
                case Interrupt.InterruptTypes.RAIL_END:
                    if (!character.myController.canRailGrind)
                        return interrupt.state;
                    break;
                case Interrupt.InterruptTypes.KEY_RELEASE:
                    break;
                case Interrupt.InterruptTypes.WALL_END:
                    if (!character.myController.canWallRun)
                        return interrupt.state;
                    break;
                case Interrupt.InterruptTypes.WALL_START:
                    if (character.myController.canWallRun)
                        return interrupt.state;
                    break;
            }
        }

        return -1;
    }
}

[System.Serializable]
public class Interrupt
{
    [HideInInspector]
    public enum InterruptTypes
    {
        GROUND,
        RAIL_END,
        KEY_RELEASE,
        WALL_END,
        WALL_START
    }

    public InterruptTypes type;

    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int state;
}

[System.Serializable]
public class StateEvent
{

    public float start;
    public float end;

    public bool active = true;
    
    [IndexedItem(IndexedItemAttribute.IndexedItemType.SCRIPTS)]
    public int script;

    
    public List<EventParameter> parameters;

    public StateEvent()
    {
        active = true;
        parameters = new List<EventParameter>();
    }
}

[System.Serializable]
public class ParameterType
{
    public float floatVal;
    public AnimationCurve curveVal;
    public bool boolVal;
    
    public SupportedTypes paramType;

    public ParameterType()
    {
        //floatVal = 0;
        curveVal = new AnimationCurve();
        //boolVal = false;

        //paramType = SupportedTypes.FLOAT;
    }
    
    public enum SupportedTypes
    {
        FLOAT,
        BOOL,
        ANIMATION_CURVE
    }

    public object GetVal()
    {
        switch (paramType)
        {
            case ParameterType.SupportedTypes.FLOAT:
                return floatVal;
            case ParameterType.SupportedTypes.BOOL:
                return boolVal;
            case ParameterType.SupportedTypes.ANIMATION_CURVE:
                return curveVal;
            default:
                return null;
        }
    }
}

[System.Serializable]
public class EventParameter
{
    public string name;

    public ParameterType val = new ParameterType();
    
}

[System.Serializable]
public class EventScript
{
    [HideInInspector] 
    public int eventIndex;

    public string eventName = "< NEW EVENT SCRIPT >";
    
    public List<EventParameter> parameters = new List<EventParameter>();
    
}
/// <summary>
/// Associates an input with a state
/// So "Space" is an input command for the state "Jump"
/// </summary>
[System.Serializable]
public class InputCommand
{

    [IndexedItem(IndexedItemAttribute.IndexedItemType.RAW_INPUTS)]
    public int input;
    
    [IndexedItem(IndexedItemAttribute.IndexedItemType.MOTION_COMMAND)]
    public int motionCommand;

    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int state;
    
    
    public List<int> inputs;
}

/// <summary>
/// Self fucking explanatory
/// Different move lists for different characters or weapons/items
/// A list of command states basically
/// </summary>
[System.Serializable]
public class MoveList
{
    public string name;
    public List<CommandState> commandStates;

    public MoveList()
    {
        name = "<NEW MOVE LIST>";
        commandStates = new List<CommandState>();
        commandStates.Add(new CommandState());
    }
}


/// <summary>
/// Defines the states a command will be accepted and conditions
/// Example - a set of command states that will be accepted when grounded, when aerial, etc...
/// CommandStates are "sub-states" in a way to the character state, so if state is grounded, we have
/// commands that can be performed and different ones when in air, on walls, etc...
/// </summary>
[System.Serializable]
public class CommandState
{
    public string stateName;

    //Flags
    public bool aerial;
    public bool onRail;
    public bool onWall;

    //Explicit State
    public bool explicitState;
    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int state;
    

    public List<CommandStep> commandSteps;

    [HideInInspector]
    public List<int> omitList;

    [HideInInspector]
    public List<int> nextFollowups;

    public CommandState()
    {
        commandSteps = new List<CommandStep>();
        stateName = "<NEW COMMAND STATE>";
    }

    public CommandStep AddCommandStep()
    {
        
        foreach(CommandStep s in commandSteps)
        {
            if (!s.activated) { s.activated = true; return s; }
        }
        CommandStep nextStep = new CommandStep(commandSteps.Count);
        nextStep.activated = true;
        commandSteps.Add(nextStep);
        return nextStep;
    }

    public void RemoveChainCommands(int _id)
    {
        if (_id == 0) { return; }
        commandSteps[_id].activated = false;
        commandSteps[_id].followUps = new List<int>();
    }

    public void CleanUpBaseState()
    {
        
        omitList = new List<int>();

        for (int s = 1; s < commandSteps.Count; s++)
        {
            for (int f = 0; f < commandSteps[s].followUps.Count; f++)
            {
                omitList.Add(commandSteps[s].followUps[f]);
            }
        }

        nextFollowups = new List<int>();
        for (int s = 1; s < commandSteps.Count; s++)
        {
            bool skip = false;
            for (int m = 0; m < omitList.Count; m++)
            {
                if (omitList[m] == s) { skip = true; }
                if (omitList[m] >= commandSteps.Count) { skip = true; }
                if (!commandSteps[s].activated) { skip = true;}
            }
            if (!skip) { nextFollowups.Add(s); }

        }

        commandSteps[0].followUps = nextFollowups;

    }

    public void CleanUpFollowups()
    {
        for (int s = 0; s < commandSteps.Count; s++)
        {
            omitList = new List<int>();
        }
            
    }

}

/// <summary>
/// Sub state of the command state
/// </summary>
[System.Serializable]
public class CommandStep
{
    public int idIndex;

    public InputCommand command;

    public bool holdButton;

    public Conditions conditions;
    
    public List<int> followUps;

    [Tooltip("Strict refers to a step that cannot be cancelled")]
    public bool strict;

    [HideInInspector] 
    public Rect myRect;

    public bool activated;
    // Priority is so if we have directional attacks, we don't instantly move to the state w/
    // the button press and check if the directional input belongs to a valid state
    public int priority;

    public void AddFollowUp(int _nextID)
    {
        if (_nextID == 0 || idIndex == _nextID) return;

        for (int i = 0; i < followUps.Count; i++)
        {
            if (followUps[i] == _nextID) return;
        }
        followUps.Add(_nextID);
    }

    public CommandStep(int _index)
    {
        idIndex = _index;
        followUps = new List<int>();
        command = new InputCommand();
        conditions = new Conditions();
        myRect = new Rect(50, 50, 200, 200);
    }
}

/// <summary>
/// A series of boolean conditions to specify what must be satisfied to enter followup command step
/// </summary>
[System.Serializable]
public class Conditions
{
    public bool
        grounded,
        distToTargetValid,
        onRail,
        inAir,
        onWall,
        holdButton,
        test1,
        test2,
        test3,
        test4,
        test5,
        test6;
    
}
