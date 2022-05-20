
using System.Collections.Generic;
using KinematicCharacterController.Examples;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementData", menuName = "Character Controller Data/Movement Data", order=1)]
public class MovementData : ScriptableObject
{
    [Header("Stable Movement")]
    public float maxStableMoveSpeed = 10f;
    public float acceleration = 1f;
    
    [Header("Air Movement")]
    public float maxAirMoveSpeed = 15f;
    
    [Header("Misc")]
    public Vector3 Gravity = new Vector3(0, -30f, 0);
    public List<Collider> IgnoredColliders = new List<Collider>();
}