using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public abstract class StateManager : MonoBehaviour
{
    #region Identifiers

    // Right now this is primarily to hold event parameters
    public InstanceID ID;

    #endregion
    
    #region Data To Keep Track Of

    [HideInInspector] public StateMachine currentStateMachine;
    [HideInInspector] public CharacterState currentStateDefinition;

    [HideInInspector] public Attack currentAttack;
    // A bit finicky to deal with indices for this
    [HideInInspector] public StateInstance currentStateInstance;

    // Confirmed hit reactors, to avoid hitting things twice in the same attack
    [HideInInspector] public List<string> confirmedHits;

    public Dictionary<string, bool> coolDownContainer;
    public Dictionary<InstanceID, int> numTimesEnteredContainer;
    #endregion

    #region Variables Defining Character
    // Select which character our state belongs to
    public CharacterData characterData;
    [HideInInspector] public int _charIndex;
    [HideInInspector] public Character _character;
    //////////////////////////////////////////////
    
    //[HideInInspector] public CharacterController characterController;

    protected CharacterController _playerCharacterController;
    protected AICharacterController _aiCharacterController;
    
    public CharacterController characterController
    {
        get
        {
            if (this is AIStateManager)
                return _aiCharacterController;
            else
                return _playerCharacterController;
        }
    }
    
    [HideInInspector] public Animator myAnimator;

    [HideInInspector] public HitReactor hitReactor;

    [HideInInspector] public CharacterEnvironmentListener environmentListener;

    public HitBox hitbox;
    #endregion

    #region State Information
    public bool canCancel;
    public bool preventStateUpdates;
    
    public int hitConfirm; // Flag for when something is hit
    public float hitActive;
    public float hitStun; // Stunned so no input can be taken during this

    public float animSpeed;
    
    //public int currentAttackIndex;
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

        environmentListener = GetComponentInChildren<CharacterEnvironmentListener>();

        hitReactor = GetComponent<HitReactor>();
        
        myAnimator = GetComponentInChildren<Animator>();
        myAnimator.runtimeAnimatorController = characterData.character.animator;

        numTimesEnteredContainer = new Dictionary<InstanceID, int>();
        
        // Reset all state machines numTimeEntered fields
        foreach (StateMachine FSM in _character.stateMachines)
        {
            foreach (InstanceID stateID in FSM.stateInstances.Keys) numTimesEnteredContainer[stateID] = 0;
        }

        foreach (CharacterState state in _character.characterStates)
        {
            state.stateUnlocked = false;
        }
        
        var entryFSM = _character.GetEntryFSM();
        var entryState = entryFSM.stateInstances[entryFSM.entryState];


        coolDownContainer = new Dictionary<string, bool>();
        StartState(entryState, entryFSM); // TEMP
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
            StartState(satisfiedFSM.stateInstances[satisfiedFSM.entryState], satisfiedFSM);
        } 
    }
    
    #endregion
    
    #region STATE UPDATE CALLS

    public void ResetStateInstanceCounters()
    {
        foreach (var stateIDs in currentStateMachine.stateInstances)
        {
            numTimesEnteredContainer[stateIDs.Key] = 0;
        }
    }
    
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
            UpdateAttacks(); //Temporarily Removed 

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
        
        foreach (StateEvent _ev in events)
        {
            if (_ev.eventObject == null) continue;
            if (_ev.active && ((currentStateTime >= _ev.start && currentStateTime <= _ev.end) || ignoreTime))
            {
                if (_ev.hasCondition)
                {
                    if (!_ev.condition.condition.CheckCondition(this)) continue;
                }
                // Reached here meaning we can invoke the event, set up Object List
                _ev.eventObject.Invoke(this, _ev.GetParamObjectList());
            }

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
        foreach (Attack _atk in currentStateDefinition.attacks)
        {
            if (currentStateTime == Mathf.Round(_atk.start))
            {
                confirmedHits = new List<string>();
                hitActive = _atk.length;
                hitbox.transform.localPosition = _atk.hitBoxPos;
                hitbox.transform.localScale = _atk.hitBoxScale;
                currentAttack = _atk;
            }

            if (currentStateTime == Mathf.Round(_atk.end))
            {
                confirmedHits.Clear();
                hitActive = 0;
                hitConfirm = 0;
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
        // Create a dictionary key for the cooldown if its not already there
        if (_character.characterStates[_state.command.state].hasCoolDown && !coolDownContainer.ContainsKey(_character.characterStates[_state.command.state].stateName))
            coolDownContainer.Add(_character.characterStates[_state.command.state].stateName, false);
        
        // Check to avoid resetting states with None Input types (which could end up resetting themselves infinitely
        // I dont see any reason why this would cause issue anywhere design-wise BUT NOTE TO SELF IF IT DOES
        if (_state.ID.Equals(currentStateInstance.ID))
        {
            return;
        }

        // If Null then _stateMachine is currentStateMachine
        if (_stateMachine == null) _stateMachine = currentStateMachine;
        if (!_stateMachine.stateName.Equals(currentStateMachine.stateName)) ResetStateInstanceCounters();
        
        // Fetch valid state machine
        //if (!isRunning) StartCoroutine(SetValidStateMachine());
        
        // To avoid resetting animation and data in states that loop, only reset timer so that events
        // continue to play out in the proper timings [Duplicate state changes guard basically]
        if (currentStateInstance.ID == _state.ID && currentStateDefinition.loop)
        {
            return;
        }
        
        // Reset a rotation callback that governed a state
        characterController.rotationCallbacks = null;

        /*
         * NOTE DO NOT FORGET
         * Maybe add another input to this to support starting in a state and updating the command step
         * or do the command step update in a different method, whichever works at the time, now not necessary.
         */
        UpdateStateEvents(currentStateDefinition.onStateExitEvents, true);
        
        // Set state to cooldown
        if (currentStateDefinition.hasCoolDown)
        {
            StartCoroutine(StateCoolDown(currentStateDefinition));
        }
        
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
        characterController.velocityCleanUpCallbacks = null;
        characterController.preventVelocityClamping = false;
        // ================================================================================================ //
        
        SetAnimation(currentStateDefinition.stateName);
        
        UpdateStateEvents(currentStateDefinition.onStateEnterEvents, true);

        numTimesEnteredContainer[_state.ID]++;
    }

    /// <summary>
    /// Basic coroutine that waits a few seconds before resetting combo
    /// </summary>
    /// <returns></returns>
    IEnumerator ResetCommandStep()
    {
        yield return new WaitForSeconds(2f);
        // If im in an entry state
        //if (currentStateIndex == 0) currentStateInstance = currentStateMachine.stateInstances[currentStateMachine.entryStateInstances[0]]; // TEMP
    }

    IEnumerator StateCoolDown(CharacterState stateDef)
    {
        coolDownContainer[stateDef.stateName] = false;
        yield return Helpers.GetWait(stateDef.coolDown);
        coolDownContainer[stateDef.stateName] = true;
    }

    /// <summary>
    /// Reset State parameters and return to neutral state
    /// </summary>
    protected void EndState()
    {
        currentStateTime = 0;
        prevStateTime = -1;
        // If ending state, always return to entry state of current FSM
        StartState(currentStateMachine.stateInstances[currentStateMachine.entryState]); 
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
        // Let state machines with priority == -1 have to be manually transitioned to
        if (currentStateMachine.priority == -1) return currentStateMachine;
        
        StateMachine fetchingFSM = null;
        int curPriority = -1;
        foreach (var FSM in _character.stateMachines)
        {
            // The motive here is if you don't attach a condition to the state machine, you have to manually 
            // setup the transition for it and it wont be auto-selected. Might just make this a bool so you manually 
            // have to do it instead of tying it to lack of conditions.
            if (FSM.FSMCondition.condition == null) continue;
            if (FSM.FSMCondition.condition.CheckCondition(this))
            {
                if (FSM.priority < 0) continue;
                if (FSM.priority > curPriority)
                {
                    curPriority = FSM.priority;
                    fetchingFSM = FSM;
                }
            }
        }

        return currentStateMachine.FSMCondition.condition == null ? currentStateMachine : fetchingFSM;
    }

    /// <summary>
    /// Plays animation state (that must be equivalent to state name) 
    /// </summary>
    /// <param name="animName"></param>
    protected virtual void SetAnimation(string animName)
    {

        foreach (var animCondiPair in currentStateDefinition.animationOverrides)
        {
            if (animCondiPair.condition.CheckCondition(this))
            {
                animName = animCondiPair.animName;
                break;
            }
        }

        myAnimator.CrossFadeInFixedTime(animName, currentStateDefinition.blendRate);
    }

    #endregion

}
