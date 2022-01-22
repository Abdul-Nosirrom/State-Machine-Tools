using DG.Tweening;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;



public class EnvironmentBehavior : MonoBehaviour
{
    public List<BehaviorAttributes> behaviors;

    private Transform originalTransform;
    private Rigidbody rigidBody;

    private void Start()
    {
        originalTransform = transform;
        rigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        foreach (var behave in behaviors)
        {
            if (!behave.inProcess)
                StartCoroutine(PerformBehavior(behave));
        }
    }

    void ReadBehavior(BehaviorAttributes beh)
    {
        switch (beh.behavior)
        {
            case BehaviorAttributes.Behavior.NONE: break;
                    
            case BehaviorAttributes.Behavior.MOVE:
                beh.start = transform.position;
                MoveBehavior(beh);
                break;
                    
            case BehaviorAttributes.Behavior.ROTATE:
                beh.start = transform.rotation.eulerAngles;
                RotationBehavior(beh);
                break;
        }


    }

    void RotationBehavior(BehaviorAttributes behave)
    {

        behave.inProcess = true;
        rigidBody.DORotate(behave.target, behave.rate).SetEase(behave._moveEase);
        

    }

    void MoveBehavior(BehaviorAttributes behave)
    {
        
        behave.inProcess = true;
        rigidBody.DOMove(behave.target, behave.rate).SetEase(behave._moveEase);
        
    }

    IEnumerator PerformBehavior(BehaviorAttributes behave)
    {
        ReadBehavior(behave);
        yield return new WaitForSeconds(behave.rate + behave.cooldownTime);
        Vector3 cache = behave.target;
        behave.target = behave.start;
        behave.start = cache;
        behave.inProcess = false;
        yield return 0;
    }
}

[System.Serializable]
public class BehaviorAttributes
{
    [HideInInspector]
    public enum Behavior
    {
        NONE,
        MOVE,
        ROTATE,
        FOLLOWPATH
    }
    
    //[HideInInspector]
    public bool inProcess;
    
    [HideInInspector]
    public Vector3 start;
        
    public Behavior behavior;

    public bool loop;

    public Ease _moveEase = Ease.Linear;
    
    [Tooltip("Time to complete motion")]
    [Range(0, 20f)]
    public float rate;

    [Tooltip("Time to wait before looping motion")] 
    [Range(0, 20f)]
    public float cooldownTime;
        
    public Vector3 target;
        
    [Tooltip("Relative to self or world?")] 
    public bool relative;
}

