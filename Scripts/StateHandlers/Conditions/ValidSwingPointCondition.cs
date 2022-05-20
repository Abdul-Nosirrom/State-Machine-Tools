using UnityEngine;


[CreateAssetMenu(fileName = "Swing Point Found", menuName = "State Manager/Conditions/SwingCheck", order = 2)]
public class ValidSwingPointCondition : Condition
{
    public float MaxDistance;
    public override bool CheckCondition(StateManager state)
    {
        RaycastHit hit;
        return Physics.Raycast(state.transform.position, state.transform.up, out hit, MaxDistance);
    }
}