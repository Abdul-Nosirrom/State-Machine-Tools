using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using UnityEngine;


[CreateAssetMenu(fileName = "Action Data", menuName = "Core Data/Action Data", order = 1)]
[System.Serializable]
public class ActionData : ScriptableObject
{
    public List<Character> characters;
    
    // Inputs should be shared and not character dependent
    public List<RawInput> rawInputs;
    public List<MotionCommand> motionCommands;

    [HideInInspector] public int currentCharacterIndex;



    public string[] GetCharacterNames()
    {
        string[] _names = new string[characters.Count];
        for (int i = 0; i < _names.Length; i++)
        {
            _names[i] = characters[i].name;
        }

        return _names;
    }

    public string[] GetScriptNames()
    {
        List<EventScript> eventScripts = characters[currentCharacterIndex].eventScripts;
        string[] _names = new string[eventScripts.Count];
        for (int i = 0; i < _names.Length; i++)
        {
            _names[i] = eventScripts[i].eventName;
        }

        return _names;
    }
        
    public string[] GetStateNames()
    {
        List<CharacterState> characterStates = characters[currentCharacterIndex].characterStates;
        string[] _names = new string[characterStates.Count];
        for (int i = 0; i < _names.Length; i++)
        {
            _names[i] = characterStates[i].stateName;
        }

        return _names;
    }
    
    public string[] GetPrefabNames()
    {
        List<GameObject> globalPrefabs = characters[currentCharacterIndex].globalPrefabs;
        string[] _names = new string[globalPrefabs.Count];
        for (int i = 0; i < _names.Length; i++)
        {
            _names[i] = globalPrefabs[i].name;
        }

        return _names;
    }
    

    public string[] GetFollowUpNames(int _commandState, bool _deleteField)
    {
        List<MoveList> moveLists = characters[currentCharacterIndex].moveLists;
        int currentMoveListIndex = characters[currentCharacterIndex].currentMoveListIndex;
        
        int nameCount = moveLists[currentMoveListIndex].commandStates[_commandState].commandSteps.Count;
        if (_deleteField) { nameCount += 2; }
        string[] _names = new string[nameCount];
        for (int i = 0; i < _names.Length; i++)
        {
            if (i < _names.Length - 2)
            {
                _names[i] = moveLists[currentMoveListIndex].commandStates[_commandState].commandSteps[i].idIndex.ToString();
            }
            else if(i < _names.Length - 1)
            {
                _names[i] = "+ ADD";
            }
            else
            {
                _names[i] = "- DELETE";
            }
        }
        
        return _names;
    }
    
    public string[] GetRawInputNames()
    {
        string[] _names = new string[rawInputs.Count];
        for (int i = 0; i < _names.Length; i++)
        {
            _names[i] = rawInputs[i].name;
        }
        return _names;
    }
    
    public string[] GetMotionCommandNames()
    {
        string[] _names = new string[motionCommands.Count];
        for (int i = 0; i < _names.Length; i++)
        {
            _names[i] = motionCommands[i].name;
        }
        return _names;
    }

    public string[] GetCommandStateNames()
    {
        List<MoveList> moveLists = characters[currentCharacterIndex].moveLists;
        int currentMoveListIndex = characters[currentCharacterIndex].currentMoveListIndex;
        
        string[] _names = new string[moveLists[currentMoveListIndex].commandStates.Count];
        for (int i = 0; i < _names.Length; i++)
        {
            _names[i] = moveLists[currentMoveListIndex].commandStates[i].stateName.ToString();
        }
        return _names;
    }
    
    public string[] GetMoveListNames()
    {
        List<MoveList> moveLists = characters[currentCharacterIndex].moveLists;
        
        string[] _names = new string[moveLists.Count];
        for (int i = 0; i < _names.Length; i++)
        {
            _names[i] = moveLists[i].name.ToString();
        }
        return _names;
    }
    
    
}
