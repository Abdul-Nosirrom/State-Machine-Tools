using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// You attach a component of the appropriate EventListener type to the game Object, and select
/// what function call it makes in response to the event being raised
/// T - Type, data that's passed around
/// E - Event
/// UER - Unity Event Response
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="E"></typeparam>
/// <typeparam name="UER"></typeparam>
public abstract class BaseGameEventListener<T, E, UER> : MonoBehaviour, 
IGameEventListener<T> where E : BaseGameEvent<T> where UER : UnityEvent<T>
{
    [SerializeField] private E _gameEvent;
    
    [SerializeField] UER _unityEventResponse;

    public E gameEvent
    {
        get 
        {
            return _gameEvent;
        }
        set
        {
            _gameEvent = value;
        }
    }

    private void OnEnable()
    {
        if (_gameEvent == null) return;

        gameEvent.RegisterListener(this);
    }

    private void OnDisable()
    {
        if (_gameEvent == null) return;

        gameEvent.UnregisterListener(this);
    }

    public void OnEventRaised(T item)
    {
        // Null check operator (?)
        _unityEventResponse?.Invoke(item);
    }
}