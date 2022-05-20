using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public abstract class StateManager : MonoBehaviour
{
    #region Data To Keep Track Of

    [HideInInspector] public StateMachine currentStateMachine;
    [HideInInspector] public CharacterState currentStateDefinition;
    
    // A bit finicky to deal with indices for this
    [HideInInspector] public StateInstance currentStateInstance;

    #endregion

    #region Variables Defining Character
    // Select which character our state belongs to
    public CharacterData characterData;
    [HideInInspector] public int _charIndex;
    [HideInInspector] public Character _character;
    //////////////////////////////////////////////
    
    [HideInInspector] public CharacterController characterController;

    [HideInInspector] public Animator myAnimator;

    [HideInInspector] public HitReactor hitReactor;

    public HitBox hitbox;
    #endregion

    #region State Information
    public bool canCancel;
    public bool preventStateUpdates;
    
    public int hitConfirm; // Flag for when something is hit
    public float hitActive;
    public float hitStun; // Stunned so no input can be taken during this

    public float animSpeed;
    
    public int currentAttackIndex;
    public int currentStateIndex;
    public float currentStateTime;
    public float prevStateTime;

    #endregion

    #region Modifiable Parameters

    public int stateMachineChangeGrace;
    public static float whiffWindow = 8f;
    
    #endregion
    // How long to cancel if an attack misses as opposed to hit
    
    
    #region MONOBEHAVIORS

    private void OnValidate()
    {
        if (characterData == null) return;
        _character = characterData.character;
    }

    protected virtual void Start()
    {
        _character = characterData.character;
        characterController = GetComponent<CharacterController>();

        hitReactor = GetComponent<HitReactor>();
        
        myAnimator = GetComponentInChildren<Animator>();
        myAnimator.runtimeAnimatorController = characterData.character.animator;
        
        currentStateMachine = _character.GetEntryFSM();
        currentStateInstance = currentStateMachine.stateInstances[currentStateMachine.entryStateInstances[0]];
        StartState(currentStateInstance, currentStateMachine); // TEMP
    }

    private void FixedUpdate()
    {
        // Check Move List
        
        //Debug.Log("Current Command State " + currentCommandState);
        if (preventStateUpdates) return;
        if (DataManager.hitStop <= 0 || true) // TEMP
        {
            UpdateStateMachine();
            UpdateState();
        }
        // Else possibly disable physics
        
        UpdateAnimation();
    }
    
    private void LateUpdate()
    {
        // ========================= Update To Valid State Machine Last =================================== //
        StateMachine satisfiedFSM = GetStateMachine();
        if (!currentStateMachine.stateName.Equals(satisfiedFSM.stateName) && currentStateTime > stateMachineChangeGrace)
        {
            StartState(satisfiedFSM.stateInstances[satisfiedFSM.entryStateInstances[0]], satisfiedFSM);
        } 
    }
    
    #endregion
    
    #region STATE UPDATE CALLS
    
    protected virtual void UpdateAnimation()
    {
        // Here handle updating animation speed
        float t = currentStateTime / currentStateDefinition.length;
        animSpeed = currentStateDefinition.animCurve.Evaluate(t);
        myAnimator.speed = animSpeed;
    }
    
    /// <summary>
    /// Reads through the input buffer and determines the appropriate command-steps
    /// whose conditions are met that are associated with said input - if so, start state
    /// </summary>
    protected abstract void UpdateStateMachine();
    

    /// <summary>
    /// Updates all state related information
    /// - State Time
    /// - State Events
    /// - Attack related information
    /// - Getting hit, not allowing for other state behaviors
    /// </summary>
    void UpdateState()
    {
        
        if (hitStun > 0)
        {
            hitReactor.GettingHit();
        }
        else
        {

            UpdateStateEvents(currentStateDefinition.events);
            //UpdateAttacks(); Temporarily Removed 

            prevStateTime = currentStateTime;
            currentStateTime++;

            if (currentStateTime >= currentStateDefinition.length)
            {
                if (currentStateDefinition.loop) LoopState();
                else EndState();
            }

            foreach (Interrupts interruptList in currentStateInstance.interrupts)
            {
                StateInstance interruptState;
                if (interruptList.interrupt == null) continue;
                if (interruptList.CheckInterrupts(this))
                {
                    interruptState = currentStateMachine.stateInstances[interruptList.followUpState];
                    int nextStateMachine = -1;
                    if (interruptState.toOtherCommandState)
                    {
                        nextStateMachine = interruptState.stateMachineTransition;
                        interruptState = _character.stateMachines[nextStateMachine]
                            .stateInstances[interruptState.otherStateMachineInstanceID];
                    }
                    
                    StartState(interruptState, nextStateMachine == -1 ? null : _character.stateMachines[nextStateMachine]);
                    break;
                }
            }
            
        }
    }

    /// <summary>
    /// Goes through all associated events for a state, and if it is set to active (otherwise is ignored)
    /// we check whether it is a valid time-frame in the state then perform the event if so
    /// </summary>
    void UpdateStateEvents(List<StateEvent> events, bool ignoreTime = false)
    {
        characterController.velocityCallbacks.Clear();  // Remove all EOMs so they respect state time definitions
        int _curEv = 0;
        foreach (StateEvent _ev in events)
        {
            if (_ev.active && ((currentStateTime >= _ev.start && currentStateTime <= _ev.end) || ignoreTime))
            {
                if (_ev.hasCondition)
                {
                    if (!_ev.condition.condition.CheckCondition(this)) continue;
                }
                // Reached here meaning we can invoke the event, set up Object List
                _character.eventScripts[_ev.script].eventScript.Invoke(this, _ev.GetParamObjectList());
            }

            _curEv++;
        }
    }

    /// <summary>
    /// Checks attacks associated with the state, and enables attack hitbox and sets associated
    /// scale and position of hitbox at the set attack time frame and disables it accordingly
    /// Also deals with allowing player to cancel attack after an attack has landed
    /// </summary>
    void UpdateAttacks()
    {
        int _cur = 0;
        foreach (Attack _atk in _character.characterStates[currentStateIndex].attacks)
        {
            if (currentStateTime == _atk.start)
            {
                hitActive = _atk.length;
                hitbox.transform.localPosition = _atk.hitBoxPos;
                hitbox.transform.localScale = _atk.hitBoxScale;
                currentAttackIndex = _cur;
            }

            if (currentStateTime == _atk.start + _atk.length)
            {
                hitActive = 0;
            }

            // Hit Cancel
            float cWindow = _atk.start + _atk.cancelWindow;
            if (currentStateTime >= cWindow) canCancel = (hitConfirm > 0);
            if (currentStateTime >= cWindow + whiffWindow) canCancel = true;

            _cur++;
        }
    }

    void UpdateTimers()
    {

    }

    #endregion
    
    #region STATE CALLS

    /// <summary>
    /// Given a state index, start the state, reset required parameters that may have been
    /// changed from previous states and start animation
    /// Also reset position in movelist within the resetcommandstep coroutine
    /// </summary>
    /// <param name="_stateIndex"></param>
    protected virtual void StartState(StateInstance _state, StateMachine _stateMachine = null)
    {
        // If Null then _stateMachine is currentStateMachine
        if (_stateMachine == null) _stateMachine = currentStateMachine;
        if (!_stateMachine.stateName.Equals(currentStateMachine.stateName)) currentStateMachine.ResetStateInstanceCounters();
        
        // Fetch valid state machine
        //if (!isRunning) StartCoroutine(SetValidStateMachine());
        
        // To avoid resetting animation and data in states that loop, only reset timer so that events
        // continue to play out in the proper timings [Duplicate state changes guard basically]
        if (currentStateInstance.ID == _state.ID && currentStateDefinition.loop)
        {
            return;
        }

        /*
         * NOTE DO NOT FORGET
         * Maybe add another input to this to support starting in a state and updating the command step
         * or do the command step update in a different method, whichever works at the time, now not necessary.
         */
        Debug.Log("Current State: " + currentStateDefinition.stateName + "\n" + " Number of exit events: " + _character.characterStates[currentStateDefinition.stateIndex].onStateExitEvents.Count);
        UpdateStateEvents(currentStateDefinition.onStateExitEvents, true);
        
        prevStateTime = -1;
        currentStateTime = 0;


        currentStateInstance = _state;
        currentStateDefinition = _character.characterStates[_state.command.state];
        currentStateMachine = _stateMachine ?? currentStateMachine;

        // Revert to start of "combo", if adding timers, add them here I suppose
        // This is kinda hacky, come back and figure out a better way to approach this later
        //if (currentStateMachine.entryStateInstances.Contains(_state.ID))
        //    StartCoroutine(ResetCommandStep());
        
        // -- Reset any parameters that might've changed to their default values, might do this elsewhere -- //
        hitActive = 0;
        hitConfirm = 0;
        canCancel = false;
        animSpeed = 1;
        // ================================================================================================ //
        
        SetAnimation(currentStateDefinition.stateName);
        
        UpdateStateEvents(currentStateDefinition.onStateEnterEvents, true);

        _state.numTimesEntered++;
    }

    /// <summary>
    /// Basic coroutine that waits a few seconds before resetting combo
    /// </summary>
    /// <returns></returns>
    IEnumerator ResetCommandStep()
    {
        yield return new WaitForSeconds(2f);
        if (currentStateIndex == 0) currentStateInstance = currentStateMachine.stateInstances[currentStateMachine.entryStateInstances[0]]; // TEMP
    }

    /// <summary>
    /// Reset State parameters and return to neutral state
    /// </summary>
    protected void EndState()
    {
        currentStateTime = 0;
        currentStateIndex = 0;
        prevStateTime = -1;
        // If ending state, always return to entry state of current FSM
        StartState(currentStateMachine.stateInstances[currentStateMachine.entryStateInstances[0]]); 
    }

    /// <summary>
    /// Reset state time measurement to allow for looping
    /// </summary>
    protected void LoopState()
    {
        currentStateTime = 0;
        prevStateTime = -1;
    }


    /// <summary>
    /// Checks current character state against it's command states and sets them accordingly
    /// such that commands inputted are consisted with the current state
    /// Here we can add different command states such as InWater, OnWall, etc..
    /// Performs basic locomotion checks to iterate our commandstate list
    /// </summary>
    public StateMachine GetStateMachine()
    {
        StateMachine fetchingFSM = null;
        int curPriority = -1;
        foreach (var FSM in _character.stateMachines)
        {
            if (FSM.FSMCondition.condition == null || FSM.FSMCondition.condition.CheckCondition(this))
            {
                if (FSM.priority >= curPriority) fetchingFSM = FSM;
            }
        }

        return fetchingFSM;
    }

    /// <summary>
    /// Plays animation state (that must be equivalent to state name) 
    /// </summary>
    /// <param name="animName"></param>
    protected virtual void SetAnimation(string animName)
    {
        myAnimator.CrossFadeInFixedTime(animName, _character.characterStates[currentStateIndex].blendRate);
    }

    #endregion

}
