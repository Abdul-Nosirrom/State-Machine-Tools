
using UnityEngine;

[CreateAssetMenu(fileName = "Random Interrupt", menuName = "State Manager/Interrupts/Random", order = 2)]
public class RandomInterrupt : Interrupt
{
    [Range(0, 1)] public float probability;
    public override bool CheckInterrupt(StateManager stateManager)
    {
        var val = Random.Range(0f, 1f);
        return probability > val;
    }
}