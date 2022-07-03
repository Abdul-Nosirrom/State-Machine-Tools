using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataManager))]
public class DataManagerInspectorGUI : Editor
{
    private DataManager _target;

    private void OnEnable()
    {
        _target = target as DataManager;
    }

    public override void OnInspectorGUI()
    {
        Undo.RecordObject(_target, "Data Manager Change");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reload Character Data"))
        {
            _target.ReloadFields();
        }
        if (GUILayout.Button("Reset State Machine Input Data"))
        {
            _target.ResetStateMachineInputData();
        }
        
        GUILayout.EndHorizontal();

        Undo.RecordObject(_target, "Data Manager Change Done");
        DrawDefaultInspector();
    }
}