using UnityEngine;


public abstract class MovementEventObject : StateEventObject
{
    // The execute method should add the EoM function to the character controllers function list

    // Keep reference to state manager and set it when event is invoked, might cause issues when multiple statemanagers 
    // are in the scene, but I think if each is invoking and setting it right when their event is called it shouldn't be
    // an issue no? Maybe EoM isn't called in sync with this however.
    protected StateManager stateManager;

    /// <summary>
    /// For movement related events that interact with the character controller,
    /// define the appropriate equation of motion that governs the event, and it'll be added
    /// (or removed) from the whatever.
    ///
    /// All This method should return is literally the diff eq, so just something like
    /// x'' = sin(x') + b^2 * x':
    /// return sin(vel) + b^2 * vel
    ///
    /// the solver will handle the rest
    /// </summary>
    /// <param name="vel"></param>
    /// <returns></returns>
    public abstract Vector3 EoM(Vector3 vel);
}