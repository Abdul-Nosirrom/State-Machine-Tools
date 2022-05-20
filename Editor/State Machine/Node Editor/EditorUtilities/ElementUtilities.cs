using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;


    public static class ElementUtilities
    {
        public static Button CreateButton(string text, Action onClick = null)
        {
            Button button = new Button(onClick)
            {
                text = text
            };

            return button;
        }

        public static Foldout CreateFoldout(string title, bool collapsed = false)
        {
            Foldout foldout = new Foldout()
            {
                text = title,
                value = !collapsed
            };

            return foldout;
        }
        
        public static Port CreatePort(this Node node, string portName = "", Port.Capacity capacity = Port.Capacity.Single, Direction direction = Direction.Output, Orientation orientation = Orientation.Horizontal)
        {
            Port port = node.InstantiatePort(orientation, direction, capacity, typeof(int));

            port.portName = portName;

            return port;
        }
        
        public static StatePort CreateStatePort(string portName = "", Port.Capacity capacity = Port.Capacity.Single, Direction direction = Direction.Output, Orientation orientation = Orientation.Horizontal)
        {
            StatePort port = StatePort.Create<Edge>(portName, direction, capacity, orientation);

            port.portName = portName;

            return port;
        }
        
        public static InterruptPort CreateInterruptPort(Interrupts interrupts, string portName = "", Port.Capacity capacity = Port.Capacity.Single, Direction direction = Direction.Output, Orientation orientation = Orientation.Horizontal)
        {
            InterruptPort port = InterruptPort.Create<Edge>(interrupts, portName, direction, capacity, orientation);

            port.portName = portName;

            return port;
        }
        
        public static ConditionPort CreateConditionPort(string portName = "", Port.Capacity capacity = Port.Capacity.Single, Direction direction = Direction.Output, Orientation orientation = Orientation.Horizontal)
        {
            ConditionPort port = ConditionPort.Create<Edge>(portName, direction, capacity, orientation);

            port.portName = portName;

            return port;
        }

        public static TextField CreateTextField(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textField = new TextField()
            {
                value = value,
                label = label
            };

            if (onValueChanged != null)
            {
                textField.RegisterValueChangedCallback(onValueChanged);
            }

            return textField;
        }

        public static TextField CreateTextArea(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textArea = CreateTextField(value, label, onValueChanged);

            textArea.multiline = true;

            return textArea;
        }
    }
