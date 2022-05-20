using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StateUISelector : MonoBehaviour
{
    private Label stateLabel;
    private Label stateMachineLabel;
    private void OnEnable()
    {
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;

        stateLabel = rootVisualElement.Q<Label>("CurrentState");
        stateMachineLabel = rootVisualElement.Q<Label>("CurrentFSM");
    }

    public void ChangeStateLabel(string stateName)
    {
        stateLabel.text = stateName;
    }

    public void ChangeFSMLabel(string FSMName)
    {
        stateMachineLabel.text = FSMName;
    }
}
