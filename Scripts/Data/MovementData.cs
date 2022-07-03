
using System.Collections.Generic;
using KinematicCharacterController.Examples;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementData", menuName = "Character Controller Data/Movement Data", order=1)]
public class MovementData : ScriptableObject
{
    [Header("Acceleration")]
    public float groundLateralAcceleration;
    public float airLateralAcceleration;

    [Header("Acceleration Curves")] // Evaluated with respect to max speed
    public AnimationCurve groundLateralAccelerationCurve;
    public AnimationCurve airLateralAccelerationCurve;

    
    [Header("Speed Limit")]
    public float maxLateralMovementSpeed;
    public float maxVerticalMovementSpeed;
    
    [Header("Gravity")] 
    public Vector3 Gravity = new Vector3(0, -100f, 0);
    public AnimationCurve GravityDamper;

    [Header("Friction")] 
    public float brakingFriction;

    public float groundForwardFriction;
    public float groundTangentFriction;
    public float airForwardFriction;
    public float airTangentFriction;
    public float airVerticalDrag;
    public AnimationCurve groundTangentFrictionCurveOnAngle;
    public AnimationCurve groundTangentFrictionCurveOnSpeed;
    public AnimationCurve airTangentFrictionCurveOnAngle;
    public AnimationCurve airTangentFrictionCurveOnSpeed;

    private Transform InputRotationTransform(Transform character, Vector3 targetForward)
    {
        Transform mimickTransform = character.transform;
        
        Vector3 smoothedLookInputDirection = targetForward.normalized;

        mimickTransform.rotation = Quaternion.LookRotation(smoothedLookInputDirection, mimickTransform.up);

        return mimickTransform;
    }
    
    public Vector3 Friction(CharacterController controller)
    {
        Transform mimick = InputRotationTransform(controller.transform, controller.moveInputVec);
        
        Vector3 localVel = mimick.InverseTransformDirection(controller.Motor.BaseVelocity);
        Vector3 localInput = mimick.InverseTransformDirection(controller.moveInputVec);
        float angle = Vector3.Angle(localInput, localVel.onlyXZ());
        Vector3 tangentVel = localVel.onlyX();
        Vector3 forwardVel = localVel.onlyZ();

        Debug.DrawRay(controller.transform.position, controller.Motor.BaseVelocity.onlyXZ().normalized * 3f, Color.yellow);
        Debug.DrawRay(controller.transform.position, controller.moveInputVec * 2f, Color.magenta);
        
        //if (angle > 145)
        //{
        //    forwardVel = Vector3.zero;
        //    tangentVel = forwardVel + tangentVel;
        //}
        
        Vector3 forwardFric, tangentFric, verticalDrag = Vector3.zero;

        if (controller.Motor.GroundingStatus.IsStableOnGround)
        {
            if (controller.moveInputVec.sqrMagnitude == 0f)
            {
                return -controller.Motor.BaseVelocity.onlyXZ() * brakingFriction;
            }

            forwardFric = forwardVel * groundForwardFriction;
            tangentFric = tangentVel * groundTangentFriction *
                          groundTangentFrictionCurveOnAngle.Evaluate(angle / 180f) *
                          groundTangentFrictionCurveOnSpeed.Evaluate(localVel.onlyXZ().magnitude /
                                                                     maxLateralMovementSpeed);
        }
        else
        {
            forwardFric = forwardVel * airForwardFriction;
            tangentFric = tangentVel * airTangentFriction *
                          airTangentFrictionCurveOnAngle.Evaluate(angle / 180f) *
                          airTangentFrictionCurveOnSpeed.Evaluate(localVel.onlyXZ().magnitude /
                                                                     maxLateralMovementSpeed);
            if (localVel.y < 0)
                verticalDrag = localVel.onlyY() * airVerticalDrag;

        }

        return -mimick.TransformDirection(forwardFric + tangentFric + verticalDrag);
    }

    public float Acceleration(CharacterController controller)
    {
        
        if (controller.Motor.GroundingStatus.IsStableOnGround) 
            return groundLateralAcceleration * groundLateralAccelerationCurve.Evaluate(controller.PlanarVelocity(controller.Motor.BaseVelocity).magnitude / maxLateralMovementSpeed);
        else 
            return airLateralAcceleration * airLateralAccelerationCurve.Evaluate(controller.PlanarVelocity(controller.Motor.BaseVelocity).magnitude / maxLateralMovementSpeed);;
    }

    public Vector3 GetGravity(CharacterController controller)
    {
        return Gravity;
        if (!controller.Motor.GroundingStatus.IsStableOnGround) return Gravity;

        Vector3 groundNormal = controller.Motor.GroundingStatus.GroundNormal;

        float angle = Vector3.Dot(groundNormal.normalized, Gravity.normalized);

        return Gravity * GravityDamper.Evaluate(angle / 360f);
    }


}