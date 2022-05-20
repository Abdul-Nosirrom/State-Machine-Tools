using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "Test Event", menuName = "Event Data/Test Event", order=1)]
public class TestEvent : StateEventObject
{
    public void Execute(StateManager _stateManager, float _power, AnimationCurve _timeEvolution)
    {
        return;
    }

}