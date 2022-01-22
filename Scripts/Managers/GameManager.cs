using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    /// <summary>
    ///  State change event to notify listeners of any changes to the game state whether before or after,
    /// allowing for the listeners to perform whatever action they need to for said state change
    /// </summary>
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<GameState> OnBeforeStateChanged;
    public static event Action<GameState> OnAfterStateChanged;

    private void Start()
    {
        ChangeState(GameState.Start);
    }

    public void ChangeState(GameState newState)
    {
        // A small guard to protect against null errors if event has no subscribers
        OnBeforeStateChanged?.Invoke(newState);

        switch (newState)   
        {
            case GameState.Start      : break;
            case GameState.InPause    : break;
            case GameState.InGame     : break;
            case GameState.PlayerDeath: break;
        }
    }
}

/// <summary>
/// Described a variety of game states - very basic now just for the sake of setting everything up
/// </summary>
public enum GameState
{
    Start,
    InPause,
    InGame,
    PlayerDeath,
}