using UnityEngine;

[CreateAssetMenu(fileName = "Zero Planar Velocity", menuName = "State Events/Shared/ZeroPlanarVelocity", order=1)]
public class ZeroOutPlanarVelocity : MovementEventObject
{
    public void Execute(StateManager _stateManager)
    {
        stateManager = _stateManager;
        _stateManager.characterController.AddVelocity(-_stateManager.characterController.Motor.BaseVelocity.onlyXZ());
    }
    
    
    public override Vector3 EoM(Vector3 vel)
    {
        return -stateManager.characterController.Motor.BaseVelocity.onlyXZ();
    }
}