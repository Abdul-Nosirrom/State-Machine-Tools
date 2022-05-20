
using UnityEngine;

[CreateAssetMenu(fileName = "Apply Gravity", menuName = "State Events/Shared/Gravity", order=1)]
public class GravityEvent : MovementEventObject
{
    public void Execute(StateManager _stateManager)
    {
        stateManager = _stateManager;
        _stateManager.characterController.velocityCallbacks.Add(EoM);
    }
    
    
    public override Vector3 EoM(Vector3 vel)
    {
        Debug.Log("Current Gravity: " + stateManager.characterController.movementData.Gravity.y);
        return stateManager.characterController.movementData.Gravity;
    }
}