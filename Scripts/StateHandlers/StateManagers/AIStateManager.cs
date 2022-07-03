
using Unity.VisualScripting;
using UnityEngine;

public class AIStateManager : StateManager
{

    public AICharacterController AIController => _aiCharacterController;

    private void Start()
    {
        _aiCharacterController = GetComponent<AICharacterController>();
        base.Start();
    }

    protected override void SetAnimation(string animName)
    {
        return;
    }

    protected override void UpdateStateMachine()
    {
        if (!canCancel) return;

        bool startState = false;

        StateInstance nextState = null;
        StateInstance curEntryState = currentStateMachine.stateInstances[currentStateMachine.entryState];
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
                    
                }
            }
        }

        if (startState)
        {
            StartState(nextState,  nextStateMachine != -1 ? _character.stateMachines[nextStateMachine] : null);
        }

    }
}