using UnityEngine;

[CreateAssetMenu(fileName = "Basic Input Movement", menuName = "State Events/Player/InputMovement", order=1)]
public class BasicMovementEvent : MovementEventObject
{
    private float drag;
    private float noInputDrag;
    private float inputForce;
    
    public void Execute(PlayerStateManager _stateManager, float InputForce, float Drag, float NoInputDrag)
    {

        stateManager = _stateManager;
        inputForce = InputForce;
        drag = Drag;
        noInputDrag = NoInputDrag;
        
        _stateManager.characterController.velocityCallbacks.Add(EoM);
    }
    
    /// <summary>
    /// dV/dt = Finput - b * v
    /// Linear Drag
    /// </summary>
    /// <param name="vel"></param>
    /// <returns></returns>
    public override Vector3 EoM(Vector3 vel)
    {
        Vector3 stickInput = stateManager.characterController.moveInputVec;
        if (stickInput.sqrMagnitude == 0f) return -1 * noInputDrag * vel.onlyXZ();
        return stickInput * inputForce - drag * vel.onlyXZ();
    }
}