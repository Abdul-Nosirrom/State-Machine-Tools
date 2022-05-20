using UnityEngine;


[CreateAssetMenu(fileName = "Not Grounded", menuName = "State Manager/Conditions/Not Grounded", order = 2)]
public class NotGroundedCondition : Condition
{
    
    public override bool CheckCondition(StateManager state)
    {
        return !state.characterController.Motor.GroundingStatus.IsStableOnGround;
    }
}