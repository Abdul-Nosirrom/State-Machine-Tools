using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(IndexedItemAttribute))]
public class IndexedItemDrawer : PropertyDrawer
{
    public CharacterData coreData;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (DataManager.Instance.characterData.Count == 0) return;
        
        IndexedItemAttribute indexedItem = attribute as IndexedItemAttribute;

        coreData = DataManager.Instance.characterData[DataManager.Instance.currentCharacterEditorIndex];

        switch (indexedItem.type)
        {
            case IndexedItemAttribute.IndexedItemType.SCRIPTS:
                property.intValue = EditorGUI.IntPopup(position, property.intValue, coreData.GetScriptNames(), null);
                break;

            case IndexedItemAttribute.IndexedItemType.STATES:
                property.intValue = EditorGUI.IntPopup(position, property.intValue, coreData.GetStateNames(), null);
                break;
            case IndexedItemAttribute.IndexedItemType.RAW_INPUTS:
                //property.intValue = EditorGUI.Popup(position, property.intValue, coreData.GetRawInputNames(), EditorStyles.miniButtonLeft);
                property.intValue = EditorGUI.IntPopup(position, property.intValue, InputManager.Instance.inputData.GetRawInputNames(), null);
                break;
            case IndexedItemAttribute.IndexedItemType.MOTION_COMMAND:
                property.intValue =
                    EditorGUI.IntPopup(position, property.intValue, InputManager.Instance.inputData.GetMotionCommandNames(), null);
                break;
            case IndexedItemAttribute.IndexedItemType.CHAIN_COMMAND:
                //property.intValue = EditorGUI.IntPopup(position, property.intValue, coreData.GetChainCommandNames(), null);
                break;
            case IndexedItemAttribute.IndexedItemType.COMMAND_STATES:
                property.intValue = EditorGUI.IntPopup(position, property.intValue, coreData.GetCommandStateNames(), null);
                break;
            
        }
        //coreData.SetDirty();
    }

}



[CustomPropertyDrawer(typeof(InputCommand))]
public class InputCommandDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {


        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        position.x -= 100;
        // Calculate rects
        var inputRect = new Rect(position.x, position.y, 100, position.height);
        var stateRect = new Rect(position.x + 105, position.y, 100, position.height);
        var nextAdd = new Rect(position.x + 220, position.y, 20, position.height);
        
        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(inputRect, property.FindPropertyRelative("motionCommand"), new GUIContent("Motion Command","tool tip me, father"));
        EditorGUI.PropertyField(inputRect, property.FindPropertyRelative("input"), new GUIContent("Input","tool tip me, father"));
        EditorGUI.PropertyField(stateRect, property.FindPropertyRelative("state"), GUIContent.none);
        //EditorGUI.PropertyField(nextAdd, property.FindPropertyRelative("next"), GUIContent.none, true);
        
        //if (GUI.Button(nextAdd, "+")) { property.}
        //EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;
        

        EditorGUI.EndProperty();
    }
}

