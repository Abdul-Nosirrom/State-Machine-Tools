using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class PlayerEventObject : StateEventObject
{
    
    public void Invoke(PlayerStateManager _stateManager, List<object> parameters)
    {
        MethodInfo eventFunction = GetType().GetMethod("Execute");
        
        parameters.Insert(0, _stateManager);
        
        eventFunction.Invoke(this, parameters.ToArray()); 
    }
}