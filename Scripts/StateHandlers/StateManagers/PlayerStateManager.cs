using System;
using Cinemachine;
using KinematicCharacterController;
using KinematicCharacterController.Examples;
using Unity.VisualScripting;
using UnityEngine;



public class PlayerStateManager : StateManager
{
    #region Animator Parameters

    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int LeftWall = Animator.StringToHash("Left Wall");
    private static readonly int RightWall = Animator.StringToHash("Right Wall");

    #endregion
    
    // Used to update Debug UI
    [Header("Events Raised")]
    [SerializeField] public StringEvent OnStateChangedEvent;
    [SerializeField] public StringEvent OnStateMachineChangedEvent;
    [SerializeField] public StringEvent OnSpeedChangedEvent;
    [SerializeField] public StringEvent OnStateTimeChangeEvent;
    [Header("Managers Required")]
    [HideInInspector] public GameObject characterObject;

    [SerializeField] public CinemachineFreeLook _camera;

    #region Monobehaviors

    protected override void Start()
    {
        characterObject = this.gameObject;
        DataManager.Instance.mainCharacter = this;
        _playerCharacterController = GetComponent<CharacterController>();
        base.Start();
    }

    #endregion
    
    
    #region STATE UPDATE CALLS

    /// <summary>
    /// Reads through the input buffer and determines the appropriate command-steps
    /// whose conditions are met that are associated with said input - if so, start state
    /// </summary>
    protected override void UpdateStateMachine()
    {
        // DEBUG PASS IN SPEED
        OnStateTimeChangeEvent.Raise(currentStateTime.ToString());
        OnSpeedChangedEvent.Raise(Mathf.RoundToInt(characterController.Motor.BaseVelocity.magnitude).ToString());
        
        // Should make it that if cancellible, can't auto-go to NONE input states?
        // Could also make this return a bool whether or not a new state was found
        if (!canCancel) return;
        
        bool startState = false;

        //GetStateMachine();
        StateInstance nextState = null;
        StateInstance curEntryState = currentStateMachine.stateInstances[currentStateMachine.entryState];
        InputCommand nextCommand = null;
        int nextStateMachine = -1;
        
        int currentPriority = -1;
        
        var possibleFollowUps = (currentStateInstance.followUps, curEntryState.followUps);
        
        // Start by looking through current states followups, if none are valid, check entry states - 2 Iterations of this loop
        for (int start = 0; start < 2; start++)
        {
            /*
             *   if (currentStateMachine.stateInstances[currentStateInstance].strict && s > 0) { break; }
             *   if (!currentStateMachine.stateInstances[currentStateInstance].activated) { break; }
             */
            var followUpChecks = start == 0 ? possibleFollowUps.Item1 : possibleFollowUps.Item2;

            // If we found valid followups to our current state, dont check followups to entry state
            if (start == 1 && startState) continue;
            
            if (followUpChecks.Count > 0)
            {
                foreach (var followUp in followUpChecks)
                {
                    StateInstance followUpState = currentStateMachine.stateInstances[followUp.Key];
                    
                    // Ensure dictionary properly setup
                    if (!coolDownContainer.ContainsKey(_character.characterStates[followUpState.command.state].stateName)) 
                        coolDownContainer.Add(_character.characterStates[followUpState.command.state].stateName, true);
                    
                    // Check NumTimesEntered condition
                    if (followUpState.limitTimesToEnter &&
                        numTimesEnteredContainer[followUpState.ID] == followUpState.numTimesToEnter) continue;
                    if (_character.characterStates[followUpState.command.state].hasCoolDown && !coolDownContainer[_character.characterStates[followUpState.command.state].stateName]) continue;
                    
                    // Check if state is unlocked
                    if (_character.characterStates[followUpState.command.state].isUnlockable &&
                        !_character.characterStates[followUpState.command.state].stateUnlocked) continue;
                    
                    TransitionCondition followUpCondition = followUp.Value;

                    if (currentStateDefinition.strict &&
                        InputManager.Instance.IsInputNone(followUpState.command)) continue;

                        if (InputManager.Instance.CheckInputCommand(followUpState.command) && (followUpCondition == null || followUpCondition.CheckConditions(this)))
                    {
                        if (!(followUpState.priority >= currentPriority)) continue;

                        if (followUpState.toOtherCommandState)
                        {
                            // Set next state to instance in other FSM
                            nextState = _character.stateMachines[followUpState.stateMachineTransition]
                                .stateInstances[followUpState.otherStateMachineInstanceID];
                            nextStateMachine = followUpState.stateMachineTransition;
                        }
                        else
                        {
                            nextState = followUpState;
                            nextStateMachine = -1;
                        }
                        
                        // Take the command and priority of the instance in our current FSM
                        currentPriority = followUpState.priority;
                        startState = true;
                        nextCommand = followUpState.command;
                        
                    }
                }
            }
        }

        if (startState)
        { 
            InputManager.Instance.UseInput(nextCommand.input);
            StartState(nextState,  nextStateMachine != -1 ? _character.stateMachines[nextStateMachine] : null);
        }
    }
    
    // Temp Override to debug 
    protected override void StartState(StateInstance _state, StateMachine _stateMachine = null)
    {
        base.StartState(_state, _stateMachine);
        // Update Input Managers current input data
        if (InputManager.Instance == null) Debug.Log("Null Instance!");
        InputManager.Instance.SwitchActionMap(currentStateMachine.inputData.inputActionMap);
        
        OnStateChangedEvent.Raise(_character.characterStates[_state.command.state].stateName);
        OnStateMachineChangedEvent.Raise(currentStateMachine.stateName);
    }

    private float groundingLerp = 0f;
    private int lerpSpeed = 10;
    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();

        float rawSpeed = Mathf.Abs(characterController.Motor.BaseVelocity.onlyXZ().magnitude);
        float animSpeed = 0;
        if (rawSpeed < 1) animSpeed = 0;
        else animSpeed = rawSpeed / 25;
        
        myAnimator.SetFloat(Speed, animSpeed);
        myAnimator.SetBool(LeftWall, environmentListener.wallListener.leftWallEncountered);
        myAnimator.SetBool(RightWall, environmentListener.wallListener.rightWallEncountered);
        
        
        if (characterController.Motor.GroundingStatus.IsStableOnGround)
        {
            myAnimator.SetFloat(Grounded, 1);
        }
        else 
            myAnimator.SetFloat(Grounded, 0);

        //myAnimator.SetFloat(Grounded, characterController.Motor.GroundingStatus.IsStableOnGround ? 1 : 0);
    }
    

    void UpdateTimers()
    {

    }
    

    #endregion

}
