using System;
using KinematicCharacterController;
using KinematicCharacterController.Examples;
using Unity.VisualScripting;
using UnityEngine;



public class PlayerStateManager : StateManager
{
    #region Animator Parameters

    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    #endregion
    
    // Used to update Debug UI
    [Header("Events Raised")]
    [SerializeField] public StringEvent OnStateChangedEvent;
    [SerializeField] public StringEvent OnStateMachineChangedEvent;
    
    [Header("Managers Required")]
    [HideInInspector] public GameObject characterObject;

    #region Monobehaviors

    protected override void Start()
    {
        characterObject = this.gameObject;
        DataManager.Instance.mainCharacter = this;
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
        // Should make it that if cancellible, can't auto-go to NONE input states?
        // Could also make this return a bool whether or not a new state was found
        if (!canCancel) return;
        
        bool startState = false;

        //GetStateMachine();
        StateInstance nextState = null;
        StateInstance curEntryState = currentStateMachine.stateInstances[currentStateMachine.entryStateInstances[0]];
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
                    // Check NumTimesEntered condition
                    if (followUpState.limitTimesToEnter &&
                        followUpState.numTimesEntered == followUpState.numTimesToEnter) continue;
                    
                    TransitionCondition followUpCondition = followUp.Value;
                    
                    if (InputManager.Instance.CheckInputCommand(followUpState.command) && (followUpCondition == null || followUpCondition.CheckConditions(this)))
                    {
                        if (!(followUpState.priority > currentPriority)) continue;

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
        OnStateChangedEvent.Raise(_character.characterStates[_state.command.state].stateName);
        OnStateMachineChangedEvent.Raise(currentStateMachine.stateName);
    }

    private float groundingLerp = 0f;
    private int lerpSpeed = 10;
    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();
        myAnimator.SetFloat(Speed, characterController.Motor.BaseVelocity.onlyXZ().magnitude / characterController.movementData.maxStableMoveSpeed);

        if (characterController.Motor.GroundingStatus.IsStableOnGround)
        {
            myAnimator.SetFloat(Grounded, groundingLerp = Mathf.Lerp(groundingLerp, 1, lerpSpeed*Time.fixedDeltaTime));
        }
        else 
            myAnimator.SetFloat(Grounded, groundingLerp = Mathf.Lerp(groundingLerp, 0, lerpSpeed*Time.fixedDeltaTime));

        //myAnimator.SetFloat(Grounded, characterController.Motor.GroundingStatus.IsStableOnGround ? 1 : 0);
    }
    

    void UpdateTimers()
    {

    }
    

    #endregion

}
