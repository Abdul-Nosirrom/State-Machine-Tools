using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using UnityEngine;



[System.Serializable]
public class CharacterData : ScriptableObject
{
    public Character character;
    

    public string GetCharacterName()
    {
        return character.name;
    }
    
    public string[] GetStateNames()
    {
        List<CharacterState> characterStates = character.characterStates;
        string[] _names = new string[characterStates.Count];
        for (int i = 0; i < _names.Length; i++)
        {
            _names[i] = characterStates[i].stateName;
        }

        return _names;
    }
    
    public string[] GetPrefabNames()
    {
        List<GameObject> globalPrefabs = character.globalPrefabs;
        string[] _names = new string[globalPrefabs.Count];
        for (int i = 0; i < _names.Length; i++)
        {
            _names[i] = globalPrefabs[i].name;
        }

        return _names;
    }
    
    public string[] GetCommandStateNames()
    {
        List<StateMachine> stateMachines = character.stateMachines;
        
        string[] _names = new string[stateMachines.Count];
        for (int i = 0; i < _names.Length; i++)
        {
            _names[i] = stateMachines[i].stateName.ToString();
        }
        return _names;
    }


}
