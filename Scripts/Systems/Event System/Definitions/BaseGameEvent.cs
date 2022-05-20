using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Create this scriptable object and attach it to whatever object will be responsible for raising this event
/// Then create the appropriate listeners who will listen for the event and do whatever when it's raised
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseGameEvent<T> : ScriptableObject
{
    private readonly List<IGameEventListener<T>> _eventListeners = new List<IGameEventListener<T>>();

    public void Raise(T item)
    {
        // Loop backwards just in case an event destroys itself so you don't get an index error
        for (int i = _eventListeners.Count - 1; i >= 0; i--)
        {
            _eventListeners[i].OnEventRaised(item);
        }
    }

    public void RegisterListener(IGameEventListener<T> listener)
    {
        if (!_eventListeners.Contains(listener)) _eventListeners.Add(listener);
    }

    public void UnregisterListener(IGameEventListener<T> listener)
    {
        if (_eventListeners.Contains(listener)) _eventListeners.Remove(listener);
    }
}
