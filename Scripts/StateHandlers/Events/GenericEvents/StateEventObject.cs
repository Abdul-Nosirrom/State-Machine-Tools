
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class StateEventObject : ScriptableObject
{
    
    public void Invoke(StateManager _stateManager, List<object> parameters)
    {
        // Execute function has to be at the top of the file if any other functions are there
        MethodInfo eventFunction = GetType().GetMethod("Execute");
        
        parameters.Insert(0, _stateManager);
        
        eventFunction.Invoke(this, parameters.ToArray()); 
    }
    
    public List<(Type, string)> GetParameterTypes()
    {
        // Im not sure if it always picks out the child one first, then the parent...
        MethodInfo eventFunction = GetType().GetMethod("Execute");

        object[] parameters = eventFunction.GetParameters();
        List<(Type, string)> paramTypes = new List<(Type, string)>();

        // First parameter is just the state manager, always true so check for others
        
        foreach (ParameterInfo param in parameters)
        {
            if (param.ParameterType != typeof(StateManager) && param.ParameterType != typeof(PlayerStateManager))
                paramTypes.Add((param.ParameterType, param.Name));
        }

        return paramTypes;
    }
}