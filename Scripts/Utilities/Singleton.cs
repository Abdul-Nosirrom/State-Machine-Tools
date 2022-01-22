/*
 *  Singleton usage is a design pattern to ensure that only a single instance of the class is running
 * at any given time. Below are a few different usages of a singleton. The static instance ensures
 * that a static instance is instantiated. Singleton ensures only one instance is running at any given time
 * while the script is running and persistent ensures persistent ensures only one is running in the GAMES
 * lifetime
 */

using System;
using UnityEngine;

/// <summary>
/// A static instance is similar to a singleton, but instead of destroying any new instances,
/// it overrides the current instance. This is handy for resetting the state and saves you doing
/// it manually
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }
    protected virtual void Awake() => Instance = this as T;

    protected void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}

/// <summary>
/// This transforms the static instance into a basic singleton. This will destroy any new versions
/// created, leaving the original instance intact. An example would be a game manager which handles
/// the loading of a new area, and setting up the player, enemies and the environment - managing the state
/// of the game. Also good for enemy managers for example.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        base.Awake();   
    }
}

/// <summary>
/// Finally we have a persistent version of the singleton. This will survive through scene loads. Perfect
/// for system classes which require stateful, persistent data. Or audio sources where music plays through
/// loading screens, etc...
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}