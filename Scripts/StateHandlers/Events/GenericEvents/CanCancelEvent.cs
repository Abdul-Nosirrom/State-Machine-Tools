using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "Can Cancel", menuName = "State Events/Shared/Can Cancel", order=1)]
public class CanCancelEvent : StateEventObject
{
    public void Execute(StateManager _stateManager, bool canCancel)
    {
        _stateManager.canCancel = canCancel;
    }

}