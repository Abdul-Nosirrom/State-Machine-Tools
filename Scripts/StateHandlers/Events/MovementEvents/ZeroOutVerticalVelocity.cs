using UnityEngine;

[CreateAssetMenu(fileName = "Zero Vertical Velocity", menuName = "State Events/Shared/ZeroVerticalVelocity", order=1)]
public class ZeroOutVerticalVelocity : MovementEventObject
{
    public void Execute(StateManager _stateManager)
    {
        stateManager = _stateManager;
        _stateManager.characterController.AddVelocity(-_stateManager.characterController.Motor.BaseVelocity.onlyY());
    }
    
    
    public override Vector3 EoM(Vector3 vel)
    {
        return -stateManager.characterController.Motor.BaseVelocity.onlyY();
    }
}