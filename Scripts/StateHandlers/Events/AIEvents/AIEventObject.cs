using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class AIEventObject : StateEventObject
{
    protected AIStateManager _aiStateManager;
    public void Invoke(AIStateManager _stateManager, List<object> parameters)
    {
        MethodInfo eventFunction = GetType().GetMethod("Execute");
        
        parameters.Insert(0, _stateManager);
        
        eventFunction.Invoke(this, parameters.ToArray()); 
    }
}