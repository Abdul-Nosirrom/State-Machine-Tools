using System;
using ECM.Controllers;
using PathCreation;
using UnityEngine;


public class Rail : MonoBehaviour
{
    [HideInInspector] public PathCreator railPath;
    public EndOfPathInstruction end;
    Transform _collider;
    Transform _player;
    private CharacterStateManager[] container;
    public bool dynamicCollider;
    public float boxSpeed;

    private void Start()
    {
        railPath = GetComponent<PathCreator>();
        if (dynamicCollider) _collider = GetComponentInChildren<EnvironmentBehavior>().transform;
        container = GameObject.FindObjectsOfType<CharacterStateManager>();
        Debug.Log("CONTAINER " + container.Length);
        foreach (var fsm in container)
        {
            if (fsm != null)
            {
                _player = fsm.transform;
                break;
            
            }
        }
    }

    public Vector3 GetPathPos(float dist, bool reverse)
    {
        end = reverse ? EndOfPathInstruction.Reverse : EndOfPathInstruction.Stop;
        return railPath.path.GetPointAtDistance(dist, end);
    }

    public Quaternion GetPathRot(float dist)
    {
        return railPath.path.GetRotationAtDistance(dist, end);
    }

    public bool ReachedEnd(Vector3 playerPos, bool forwardDir)
    {
        int endTime = forwardDir ? 1 : 0;

        bool endPath;
        if (forwardDir)
            endPath = railPath.path.GetClosestTimeOnPath(playerPos) >= 0.99;
        else
            endPath = railPath.path.GetClosestTimeOnPath(playerPos) <= 0.01;
        Debug.Log("END GRIND " + endPath);
        return endPath;
    }

    private float distance;
    private void Update()
    {
        if (dynamicCollider)
        {
            Vector3 newPos = railPath.path.GetClosestPointOnPath(_player.transform.position).relativeTo(transform);
            _collider.transform.position = newPos;
            
        }
    }
}
