using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ECM.Controllers;
using JetBrains.Annotations;
using DG.Tweening;
using UnityEditor;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;


public class CharacterStateManager : MonoBehaviour
{
    
    // Select which character our state belongs to
    [HideInInspector] public int _charIndex;
    [HideInInspector] public Character _character;
    //////////////////////////////////////////////
    

    public bool isGrounded;
    
    [HideInInspector] public ControllerRevamp myController;

    public int currentStateIndex;
    public float currentStateTime;
    public float prevStateTime;

    public GameObject character;
    [HideInInspector] public Animator myAnimator;

    public HitBox hitbox;

    public bool canCancel;
    public int hitConfirm; // Flag for when something is hit

    public int currentAttackIndex;

    // How long to cancel if an attack misses as opposed to hit
    public static float whiffWindow = 8f;

    public InputBuffer inputBuffer = new InputBuffer();
    
    // Movement Direction Input
    private Vector2 stickMove;
    private float cachedSpeed;

    private void OnValidate()
    {
       // _character = EngineData.actionData.characters[_charIndex];
    }

    private void Start()
    {
        myController = GetComponent<ControllerRevamp>();
        myAnimator = GetComponentInChildren<Animator>();
        StartState(0);
        DOTween.Init();
    }

    private void FixedUpdate()
    {
        //Debug.Log("Current Command State " + currentCommandState);
        isGrounded =  myController.movement.isOnGround;
        if (preventStateUpdates) return;
        if (EngineData.hitStop <= 0)
        {
            UpdateInput();
            UpdateState();
        }
        // Else possibly disable physics
        
        UpdateAnimation();
    }

    #region STATE UPDATE CALLS
    public float animSpeed;
    void UpdateAnimation()
    {
        myController.Animate();
        myAnimator.SetFloat("animSpeed", animSpeed);
        if (myController.leftWall)
            myAnimator.SetFloat("wallDirection", 0);
        else if (myController.rightWall)
            myAnimator.SetFloat("wallDirection", 1f);
    }
    
    // List to check follow ups of either current state or the NEUTRAL/BASE STATE of the commandstate
    private int[] cancelStepList = new int[2];

    /// <summary>
    /// Reads through the input buffer and determines the appropriate command-steps
    /// whose conditions are met that are associated with said input - if so, start state
    /// </summary>
    void UpdateInput()
    {
        inputBuffer.Update();

        stickMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        stickMove.Normalize();
        bool startState = false;

        GetCommandState();
        CommandState comState = EngineData.engineData.CurrentMoveList(_charIndex).commandStates[currentCommandState];


        if (currentCommandStep >= comState.commandSteps.Count) { currentCommandStep = 0; } //Change this to state-specific or even commandstep specific variables

        CommandStep curStep = comState.commandSteps[currentCommandStep];
        cancelStepList[0] = currentCommandStep;
        cancelStepList[1] = 0;
        int finalS = -1;
        int finalF = -1;
        int currentPriority = -1;
        
        for (int s = 0; s < cancelStepList.Length; s++)
        {
            if (comState.commandSteps[currentCommandStep].strict && s > 0) { break; }
            if (!comState.commandSteps[currentCommandStep].activated) { break; }

            for (int f = 0; f < comState.commandSteps[cancelStepList[s]].followUps.Count; f++)
            {
                
                CommandStep nextStep = comState.commandSteps[comState.commandSteps[cancelStepList[s]].followUps[f]];
                InputCommand nextCommand = nextStep.command;
                
                //if(inputBuffer.)
                if (CheckInputCommand(nextCommand, curStep))
                {
                    if (canCancel) 
                    {

                        if (_character.characterStates[nextCommand.state].ConditionsMet(this))
                        {
                            if (nextStep.priority > currentPriority)
                            {
                                currentPriority = nextStep.priority;
                                startState = true;
                                finalS = s;
                                finalF = f;
                            }
                        }
                    }
                }
            }
        }
        
        if (startState)
        { 
            CommandStep nextStep = comState.commandSteps[comState.commandSteps[cancelStepList[finalS]].followUps[finalF]];
            InputCommand nextCommand = nextStep.command;
            inputBuffer.UseInput(nextCommand.input);
            if (nextStep.followUps.Count > 0) { currentCommandStep = nextStep.idIndex; }
            else 
            { 
                currentCommandStep = 0;
                currentIndividualStep = nextStep.idIndex; 
            }
            StartState(nextCommand.state);
        }
    }
    
    /// <summary>
    /// Checks whether the input has been registers
    /// At any given point, the input buffer holds the frames since an input was held
    /// Once released, the frame in which it was released has value -1 in the input buffer
    /// Hence we check for valid frames in the input buffer here
    /// </summary>
    /// <param name="_in"></param>
    /// <returns></returns>
    public bool CheckInputCommand(InputCommand _in, CommandStep _curStep)
    {
        
        // Checks for steps where button must be held, e.g charge attacks
        if (_curStep.holdButton)
        {
            Debug.Log("NEXT STATE " + _curStep.followUps[0]);
            int lastFrame = inputBuffer.buffer.Count - 1;
            if (inputBuffer.buffer[lastFrame].rawInputs[_curStep.command.input].hold == -1)
            {
                return true;
            }
            
            return false;
        }
        if(inputBuffer.buttonCommandCheck[_in.input] < 0) { return false; }
        if(inputBuffer.motionCommandCheck[_in.motionCommand] < 0) { return false; }
        return true;
    }

    /// <summary>
    /// Updates all state related information
    /// - State Time
    /// - State Events
    /// - Attack related information
    /// - Getting hit, not allowing for other state behaviors
    /// </summary>
    void UpdateState()
    {
        CharacterState myCurrentState = _character.characterStates[currentStateIndex];

        if (hitStun > 0)
        {
            GettingHit();
        }
        else
        {

            UpdateStateEvents();
            UpdateAttacks();

            prevStateTime = currentStateTime;
            currentStateTime++;

            if (currentStateTime >= myCurrentState.length)
            {
                if (myCurrentState.loop) LoopState();
                else EndState();
            }

            int interruptCheck = myCurrentState.CheckInterrupts(this);
            if (interruptCheck != -1) StartState(interruptCheck);
        }
    }

    /// <summary>
    /// Goes through all associated events for a state, and if it is set to active (otherwise is ignored)
    /// we check whether it is a valid time-frame in the state then perform the event if so
    /// </summary>
    void UpdateStateEvents()
    {
        int _curEv = 0;
        foreach (StateEvent _ev in _character.characterStates[currentStateIndex].events)
        {
            if (_ev.active)
            {
                if (currentStateTime >= _ev.start && currentStateTime <= _ev.end)
                {
                    DoEventScript(
                        _ev.script, currentStateIndex,
                        _curEv, _ev.parameters);
                }
            }

            _curEv++;
        }
    }

    public float hitActive;
    /// <summary>
    /// Checks attacks associated with the state, and enables attack hitbox and sets associated
    /// scale and position of hitbox at the set attack time frame and disables it accordingly
    /// Also deals with allowing player to cancel attack after an attack has landed
    /// </summary>
    void UpdateAttacks()
    {
        int _cur = 0;
        foreach (Attack _atk in _character.characterStates[currentStateIndex].attacks)
        {
            if (currentStateTime == _atk.start)
            {
                hitActive = _atk.length;
                hitbox.transform.localPosition = _atk.hitBoxPos;
                hitbox.transform.localScale = _atk.hitBoxScale;
                currentAttackIndex = _cur;
            }

            if (currentStateTime == _atk.start + _atk.length)
            {
                hitActive = 0;
            }

            // Hit Cancel
            float cWindow = _atk.start + _atk.cancelWindow;
            if (currentStateTime >= cWindow) canCancel = (hitConfirm > 0);
            if (currentStateTime >= cWindow + whiffWindow) canCancel = true;

            _cur++;
        }
    }

    void UpdateTimers()
    {

    }
    

    #endregion
    
    #region STATE CALLS

    private int frameCounter = 0;
    /// <summary>
    /// Takes an event and event related information, and just calls on associated methods
    /// in the order they are in within the scriptable object containing the events list
    /// </summary>
    /// <param name="_index"></param>
    /// <param name="_actIndex"></param>
    /// <param name="_evIndex"></param>
    /// <param name="_params"></param>
    void DoEventScript(int _index, int _actIndex, int _evIndex, List<EventParameter> _params)
    {
        // One event is global prefab


        EventScript eventCall = _character.eventScripts[_index];
        string eventName = eventCall.eventName.Replace(" ", "");
        MethodInfo eventFunction = this.GetType().GetMethod(eventName);
        
        object[] parameters = new object[eventFunction.GetParameters().Length];
        
        int i = 0;

        if (eventFunction.GetParameters().Length != 0)
        {
            foreach (EventParameter par in _params)
            {
                parameters[i] = par.val.GetVal();
                i++;
            }
        }

        eventFunction.Invoke(this, parameters);

    }

    /// <summary>
    /// Given a state index, start the state, reset required parameters that may have been
    /// changed from previous states and start animation
    /// Also reset position in movelist within the resetcommandstep coroutine
    /// </summary>
    /// <param name="_stateIndex"></param>
    void StartState(int _stateIndex)
    {
        prevStateTime = -1;
        currentStateTime = 0;

        // To avoid resetting animation and data in states that loop, only reset timer so that events
        // continue to play out in the proper timings [Duplicate state changes guard basically]
        //if (currentStateIndex == _stateIndex) return;
        
        currentStateIndex = _stateIndex;
        
        // Revert to start of "combo", if adding timers, add them here I suppose
        if (_stateIndex == 0)
            StartCoroutine(ResetCommandStep());
        
        hitActive = 0;
        hitConfirm = 0;
        canCancel = false;

        myController.movement.cachedRigidbody.isKinematic = false;
        myController.isRailGrind = false;
        animSpeed = 1;

        SetAnimation(_character.characterStates[_stateIndex].stateName);
    }

    /// <summary>
    /// Basic coroutine that waits a few seconds before resetting combo
    /// </summary>
    /// <returns></returns>
    IEnumerator ResetCommandStep()
    {
        yield return new WaitForSeconds(2f);
        if (currentStateIndex == 0) currentCommandStep = 0;
    }

    /// <summary>
    /// Reset State parameters and return to neutral state
    /// </summary>
    void EndState()
    {
        currentStateTime = 0;
        currentStateIndex = 0;
        prevStateTime = -1;
        StartState(currentStateIndex);
    }

    /// <summary>
    /// Reset state time measurement to allow for looping
    /// </summary>
    void LoopState()
    {
        currentStateTime = 0;
        prevStateTime = -1;
    }

    public int currentCommandState;
    public int currentCommandStep;
    public int currentIndividualStep;
    /// <summary>
    /// Checks current character state against it's command states and sets them accordingly
    /// such that commands inputted are consisted with the current state
    /// Here we can add different command states such as InWater, OnWall, etc..
    /// Performs basic locomotion checks to iterate our commandstate list
    /// </summary>
    public void GetCommandState()
    {
        // Here, update command states to include onWall, onRail, etc...
        currentCommandState = 0;
        
        foreach (CommandState cs in EngineData.engineData.CurrentMoveList(_charIndex).commandStates)
        {
            if (cs.aerial && !myController.movement.isOnGround) currentCommandState = 1;

            if (cs.onRail && myController.isRailGrind) currentCommandState = 2;

            if (cs.onWall && myController.canWallRun) currentCommandState = 3;

            // Add more flags accordingly

            //currentCommandState++;
        }
    }

    /// <summary>
    /// Plays animation state (that must be equivalent to state name) 
    /// </summary>
    /// <param name="animName"></param>
    void SetAnimation(string animName)
    {
        myAnimator.CrossFadeInFixedTime(animName, _character.characterStates[currentStateIndex].blendRate);
    }

    #endregion
    
    #region EVENT FUNCTIONS

    /// <summary>
    /// Manually sets the Can Cancel parameter through an event
    /// </summary>
    /// <param name="_val"></param>
    public void CanCancel(bool _val)
    {
        canCancel = _val;
    }


    public float hitStun; // Stunned so no input can be taken during this

    public void GetHit(CharacterStateManager attacker)
    {
        Attack curAtk = _character.characterStates[attacker.currentStateIndex]
            .attacks[attacker.currentAttackIndex];
        // Apply Knockback
        // Change State
        // Change Material if needed
        // Any status effects
        hitStun = curAtk.hitStun;
        EngineData.SetHitStop(curAtk.hitStun);
        attacker.hitConfirm++;
        // We can use a hashmap and add enemies to it, check if theyre there as to not hit them twice

        // Call global prefab for getting hit effect
        // We can set attack VFX to appear on a different camera thats the child of the player or whatever
        // Set its layer to something different in main, and set it only to attackVFX in camera
        // Setup explained in part 6 at 30 minutes
    }

    void GettingHit()
    {
        hitStun--;
    }

    /// <summary>
    /// Generates effects for different states, like slash effects, foot stuff etc...
    /// </summary>
    /// <param name="_index"></param>
    /// <param name="_act"></param>
    /// <param name="_ev"></param>
    void GlobalPrefab(int _index, int _act, int _ev)
    {
        EngineData.GlobalPrefab(_index, _charIndex, gameObject, _act, _ev);
    }

    public void StickMove()
    {

        myController.moveDirection = new Vector3
        {
            x = stickMove.x,
            y = 0,
            z = stickMove.y
        };
        myController.moveDirection = VectorExtensions.relativeTo(myController.moveDirection, Camera.main.transform);
        myController.GroundCall();
    }
    

    public void Jump()
    {
        //myController.movement.ApplyVerticalImpulse(_pow);
        //myController.movement.DisableGrounding();
        CommandStep comStep = EngineData.engineData.CurrentMoveList(_charIndex).commandStates[currentCommandState].commandSteps[currentIndividualStep];
        int input = comStep.command.input;

        int count = 0;
        bool jumpDetected = false;
        /*
        foreach (var val in inputBuffer.buffer)
        {
            if (val.rawInputs[input].hold != 0)
            {
                jumpDetected = true;
                break;
            }
        }
    */
        jumpDetected = (inputBuffer.buffer[24].rawInputs[input].hold != 0);
        myController.AirCall(jumpDetected);
    }

    public void WallRun()
    {
        myController.WallRun();
        Debug.Log("Is Wall on the left? " + myController.leftWall);
    }

    public void WallJump(float jumpStrength)
    {
        Vector3 sideDir = myController.leftWall ? transform.right : -transform.right;
        Vector3 jumpDir = (sideDir + transform.up).normalized;
        
        myController.movement.ApplyImpulse(jumpDir * jumpStrength);
    }

    public void ForwardImpulse(AnimationCurve _curve, float _pow, bool _grav)
    {
       /* if (myController.movement.isOnPlatform)
        {
            myController.movement.velocity += transform.forward * _pow;
        }*/
        float normalizedStateTime = currentStateTime / _character.characterStates[currentStateIndex].length;
        float curveVal = _curve.Evaluate(normalizedStateTime);
        myController.movement.Move(transform.forward * _pow * curveVal, _pow);
        return;
        Vector3 tangent = Vector3.ProjectOnPlane(transform.forward, myController.movement.groundNormal);
        Vector3 velocity = tangent.normalized;
        //myController.movement.ApplyForce(transform.forward * _pow);
        if (_grav) 
        {
            //velocity += myController.movement.gravity.normalized;
            /*
            Vector3 normal = myController.movement.groundNormal;
            Vector3 tangent = Vector3.Cross( normal, transform.forward );

            if( tangent.magnitude == 0 )
            {
                tangent = Vector3.Cross( normal, Vector3.up );
            } 
            myController.movement.velocity = tangent.normalized * _pow;
            */
            
        }
        myController.movement.velocity = velocity.normalized * _pow;

    }


    public void UpwardImpulse(float _pow)
    {
        //myController.movement.velocity = transform.up * _pow;
        myController.movement.ApplyVerticalImpulse(_pow);
        myController.moveDirection = Vector3.zero;
        
        if (_pow > 0)
            myController.movement.DisableGrounding();
    }

    public void ZeroOutVelocity()
    {
        myController.movement.velocity = new Vector3
        {
            x = 0,
            y = myController.movement.velocity.y,
            z = 0
        };
        
        
    }

    public void DisableGravity()
    {
        myController.movement.velocity = myController.movement.velocity.onlyXZ();
    }

    public void SetAnimSpeed(float _val)
    {
        animSpeed = _val;
    }

    public void ResetJumpCounter()
    {
        myController.midAirJumpCount = 0;
    }

    public void RailGrind()
    {
        myController.movement.cachedRigidbody.isKinematic = true;
        myController.isRailGrind = myController.canRailGrind;
        //myController.GrindRail();
    }

    #region LEDGECLIMB AND WALLRUN

    [Header("Ledge Testing")]
    public Vector3 ledgeCheckPos1;
    public Vector3 ledgeCheckPos2;
    public float tweenUpRate;
    public float tweenForwardRate;
    private bool preventStateUpdates = false;
    public LayerMask ledgeMask;
    private bool canClimbLedge = false;
    private bool currentlyClimbing = false;
    public void LedgeClimb()
    {
        RaycastHit hit1;
        Ray ray1 = new Ray(transform.position + ledgeCheckPos1, transform.forward);
        bool check1 = Physics.Raycast(ray1, 1f);
        Ray ray2 = new Ray(transform.position + ledgeCheckPos2, transform.forward);
        RaycastHit hitInfo;
        bool check2 = Physics.Raycast(ray2, out hitInfo, 1f, ledgeMask);

        canClimbLedge = !check1 && check2;
        bool shouldClimb = Vector3.Dot(myController.movement.velocity, myController.movement.gravity) >=0 ;
        
        if (canClimbLedge && !isGrounded)// && shouldClimb)
        {
            if (!currentlyClimbing)
            {
                transform.DOMove(transform.position + transform.forward *0.5f*hitInfo.distance, 0.3f);
                Quaternion targetRot = Quaternion.LookRotation(hitInfo.normal * -1, transform.up);
                transform.DORotate(targetRot.eulerAngles, 0.3f);
                StartCoroutine(ClimbLedge());
            }
        }
    }

    IEnumerator ClimbLedge()
    {
        preventStateUpdates = true;
        currentlyClimbing = true;
        GetComponent<CapsuleCollider>().enabled = false;
        SetAnimation("Ledge Climb");
        myController.movement.cachedRigidbody.isKinematic = true;
        yield return new WaitForSeconds(tweenUpRate/2);
        transform.DOMove(transform.position + ledgeCheckPos2, tweenUpRate/2);
        yield return new WaitForSeconds(tweenUpRate/3);
        transform.DOMove(transform.position + transform.forward, tweenForwardRate);
        yield return new WaitForSeconds(tweenForwardRate);
        currentlyClimbing = false;
        myController.movement.cachedRigidbody.isKinematic = false;
        preventStateUpdates = false;
        GetComponent<CapsuleCollider>().enabled = true;
        StartState(0);
    }
    

    private void Update()
    {
        Debug.DrawRay(transform.position + ledgeCheckPos1, transform.forward, Color.green);
        Debug.DrawRay(transform.position + ledgeCheckPos2, transform.forward, Color.red);

    }

    #endregion

    #endregion
}
