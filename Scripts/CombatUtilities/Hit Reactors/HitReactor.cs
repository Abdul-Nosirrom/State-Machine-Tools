
using System;
using UnityEngine;

public abstract class HitReactor : MonoBehaviour
{
    protected readonly string _reactorID = Guid.NewGuid().ToString();
    
    public abstract void GetHit(StateManager _stateManager);

    public abstract void GettingHit();
}