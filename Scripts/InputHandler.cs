using System;
using UnityEngine;
using ECM.Common;
using ECM.Controllers;
using UnityEngine.UI;

public class InputHandler : MonoBehaviour
{
    #region EDITOR EXPOSED FIELDS

    [Header("Locomotion Direction")] 
    [SerializeField]
    protected bool relativeToSelf;
    [SerializeField] 
    protected bool relativeToCamera;
    
    #endregion
    
    #region PLAYERCOMPONENTS

    protected ControllerRevamp _controller;
    protected PlayerInput input;
    
    #endregion
    
    #region INPUT HANDLERS

    /// <summary>
    /// Struct that acts as container for player input
    /// </summary>
    public struct PlayerInput
    {
        public Vector3 moveDirection;
        public bool jump;
        public bool punch;
    }

    /// <summary>
    /// Method to fill the input struct
    /// </summary>
    private void InputCollector()
    {
        input.moveDirection = new Vector3
        {
            x = Input.GetAxisRaw("Horizontal"),
            y = 0,
            z = Input.GetAxisRaw("Vertical")
            
        };
        if (relativeToSelf)
        {
            input.moveDirection = input.moveDirection.relativeTo(this.transform);
            
        }
        else if (relativeToCamera)
        {
            input.moveDirection = input.moveDirection.relativeTo(Camera.main.transform);
        }

        input.moveDirection =
            input.moveDirection.sqrMagnitude > 1 ? input.moveDirection.normalized : input.moveDirection;

        input.jump = Input.GetButton("Jump");
    }
    #endregion

    #region MONOBEHAVIORS

    private void Start()
    {
        _controller = GetComponent<ControllerRevamp>();
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
        if (_controller.locomotionStates == ControllerRevamp.LocomotionStates.Grounded)
        {
        }
        else if (_controller.locomotionStates == ControllerRevamp.LocomotionStates.IsSliding)
        {
        }
        else if (_controller.locomotionStates == ControllerRevamp.LocomotionStates.InAir)
        {
           // _controller.AirCall();
        }
    }
    
    #endregion
    
}
