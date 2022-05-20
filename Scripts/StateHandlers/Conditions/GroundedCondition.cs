using UnityEngine;


[CreateAssetMenu(fileName = "Grounded", menuName = "State Manager/Conditions/Grounded", order = 2)]
public class GroundedCondition : Condition
{
    
    public override bool CheckCondition(StateManager state)
    {
        Debug.Log("Checking Grounded Condition: Result = " + state.characterController.Motor.GroundingStatus.IsStableOnGround);
        return state.characterController.Motor.GroundingStatus.IsStableOnGround;
    }
}
