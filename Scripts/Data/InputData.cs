using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Input Data", menuName = "Core Data/Input Data", order=1)]
[System.Serializable]
public class InputData : ScriptableObject
{
    public List<RawInput> rawInputs;
    public List<MotionCommand> motionCommands;
    
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
}