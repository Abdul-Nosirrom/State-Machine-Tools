using UnityEngine;

[CreateAssetMenu(fileName = "Apply Impulse", menuName = "State Events/Shared/Impulse", order=1)]
public class ImpulseEvent : MovementEventObject
{
    public Vector3 impulseDirection;
    public float impulseStrength;

    public void Execute(StateManager _stateManager, Vector3 ImpulseDirection, float ImpulseStrength)
    {
        Debug.Log("Calling On State Start Event!");
        stateManager = _stateManager;
        impulseDirection = ImpulseDirection;
        impulseStrength = ImpulseStrength;
        _stateManager.characterController.velocityCallbacks.Add(EoM);
       //stateManager.characterController.Motor.ForceUnground();
       //stateManager.characterController.AddVelocity(impulse);
    }
    
    
    public override Vector3 EoM(Vector3 vel)
    {
        stateManager.characterController.Motor.ForceUnground();
        return impulseDirection.normalized * impulseStrength;
    }
}