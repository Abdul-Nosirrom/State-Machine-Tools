using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StateUISelector : MonoBehaviour
{
    private Label stateLabel;
    private Label stateMachineLabel;
    private Label currentSpeedLabel;
    private Label stateTimeLabel;
    private void OnEnable()
    {
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;

        stateLabel = rootVisualElement.Q<Label>("CurrentState");
        stateMachineLabel = rootVisualElement.Q<Label>("CurrentFSM");
        currentSpeedLabel = rootVisualElement.Q<Label>("CurrentSpeed");
        stateTimeLabel = rootVisualElement.Q<Label>("CurrentStateTime");
    }

    public void ChangeStateLabel(string stateName)
    {
        stateLabel.text = stateName;
    }

    public void ChangeFSMLabel(string FSMName)
    {
        stateMachineLabel.text = FSMName;
    }

    public void ChangeSpeedLabel(string speed)
    {
        currentSpeedLabel.text = speed + " m/s";
    }

    public void ChangeStateTimeLabel(string time)
    {
        stateTimeLabel.text = time;
    }
}
