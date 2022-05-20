using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using KinematicCharacterController;
using KinematicCharacterController.Examples;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController : MonoBehaviour, ICharacterController
{
    [Header("State Event Movement Callbacks")]
    [Tooltip("Velocity Callbacks take in current velocity - passed to integration function")]
    public List<Func<Vector3, Vector3>> velocityCallbacks;
    
    
    [Header("Movement Data")]
    public MovementData movementData;
    [Header("Movement Sharpness")]
    public float StableMovementSharpness;
    public float OrientationSharpness;
    
    [Header("Camera")]
    public ExampleCharacterCamera camera;
    
    [HideInInspector]
    public KinematicCharacterMotor Motor;
    
    #region Specific Inputs
    // Get these from InputManager, and process them here 
    [HideInInspector] public Vector3 lookInputVec;
    [HideInInspector] public Vector3 moveInputVec;   
    #endregion 

    #region Additions
    private Vector3 _internalVelocity;
    #endregion
    
    #region Initialize Motor And Camera

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        Motor = GetComponent<KinematicCharacterMotor>();
        Motor.CharacterController = this;
        camera.SetFollowTransform(transform);
        
        camera.IgnoredColliders.Clear();
        camera.IgnoredColliders.AddRange(GetComponentsInChildren<Collider>());
        
        // Initialize event callbacks
        velocityCallbacks = new List<Func<Vector3, Vector3>>();
    }

    #endregion

    #region CAMERA

    private void Update()
    {
        Quaternion CameraRotation = camera.transform.rotation;
        moveInputVec = InputManager.Instance.GetNormalizedStickInput();

        Vector3 cameraPlanarDirection =
            Vector3.ProjectOnPlane(CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;

        if (cameraPlanarDirection.sqrMagnitude == 0f)
            cameraPlanarDirection = Vector3.ProjectOnPlane(CameraRotation * Vector3.up, Motor.CharacterUp);
        
        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

        lookInputVec = cameraPlanarDirection;
        moveInputVec = cameraPlanarRotation * moveInputVec;

    }

    void LateUpdate()
    {
        if (camera.RotateWithPhysicsMover && Motor.AttachedRigidbody != null)
        {
            camera.PlanarDirection =
                Motor.AttachedRigidbody.GetComponent<PhysicsMover>().RotationDeltaFromInterpolation * camera.PlanarDirection;
            camera.PlanarDirection = Vector3.ProjectOnPlane(camera.PlanarDirection, Motor.CharacterUp).normalized;
        }

        lookInputVec = InputManager.Instance.GetLookInput();

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            lookInputVec = Vector3.zero;
        }
        
        camera.UpdateWithInput(Time.deltaTime, 0, lookInputVec);
    }

    #endregion
    
    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its rotation should be right now. 
    /// This is the ONLY place where you should set the character's rotation
    ///
    /// Our rotation script should function by rotating the character towards velocity direction
    /// </summary>
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        Vector3 curVelocity = Motor.BaseVelocity;
                
        // Project current velocity to gravity plane
        curVelocity = Vector3.ProjectOnPlane(curVelocity, -movementData.Gravity);
        curVelocity.y = 0;
                
        Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, curVelocity, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

        // Set the current rotation (which will be used by the KinematicCharacterMotor)
        currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its velocity should be right now. 
    /// This is the ONLY place where you can set the character's velocity
    /// </summary>
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        // Process Movement, Called Via Events Which Do DiffEQ Shit on Input
        float currentVelocityMagnitude = currentVelocity.magnitude;

        Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;

        // Reorient velocity on slope
        if (Motor.GroundingStatus.IsStableOnGround)
            currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

        // Calculate target velocity
        Vector3 inputRight = Vector3.Cross(moveInputVec, Motor.CharacterUp);
        Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * moveInputVec.magnitude;
        Vector3 targetMovementVelocity = reorientedInput * movementData.maxStableMoveSpeed;
        moveInputVec = reorientedInput;

        // Smooth movement Velocity
        //currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-StableMovementSharpness * deltaTime));

        // THIS APPROACH CAUSES PROBLEMS W/ ONSTART AND EXIT EVENTS BECAUSE THE LIST IS CLEARED BEFORE
        // SEMI-FIXED WITH ADDING IMPULSES WHICH MAKES SENSE SINCE ONSTATESTART AND EXIT SHOULD HAVE EVENTS 
        // THAT DO NOT HAVE ANY TIME-EVOLUTION SO QUICK AND DIRTY IMPULSES WORK
        // POSSIBLE SOLUTION TO ANOTHER ISSUE REGARDING DIFF EQS IS TO DECOMPOSE THEM INTO COMPONENTS
        // Example: Two EoM that both apply drag, in this case you'll end up getting double the drag coefficient, 
        // so why not remove the drag from both and have a different event EoM that applies drag itself
        foreach (var velFunc in velocityCallbacks)
        {
            currentVelocity = Integration.Euler(currentVelocity, velFunc, deltaTime);
        }

        // Force ungrounding when adding external forces (accumulates in _internalVelocity)
        if (_internalVelocity.sqrMagnitude > 0f)
        {
            Motor.ForceUnground();
            currentVelocity += _internalVelocity;
            _internalVelocity = Vector3.zero;
        }

        Vector3 currentVelocityTangent = Vector3.ClampMagnitude(currentVelocity.onlyXZ(), movementData.maxStableMoveSpeed);
        currentVelocity = currentVelocityTangent + currentVelocity.onlyY();

    }

    public void AddVelocity(Vector3 velocity) => _internalVelocity += velocity;


    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called before the character begins its movement update
    /// </summary>
    public void BeforeCharacterUpdate(float deltaTime)
    {
        
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called after the character has finished its movement update
    /// </summary>
    public void AfterCharacterUpdate(float deltaTime)
    {
        
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        if (movementData.IgnoredColliders.Count == 0)
        {
            return true;
        }

        if (movementData.IgnoredColliders.Contains(coll))
        {
            return false;
        }

        return true;    
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
        Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
        
    }
}
