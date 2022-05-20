using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Random", menuName = "State Manager/Conditions/Random", order = 2)]
public class SomeCondition : Condition
{
    public Condition pairCondition;
    public float randomVal;
    public override bool CheckCondition(StateManager state)
    {
        bool valueofthiscondition = true;

        return valueofthiscondition && pairCondition.CheckCondition(state);
    }
}
