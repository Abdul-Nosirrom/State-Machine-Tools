using UnityEditor;
using UnityEngine;
/*

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
            
            case IndexedItemAttribute.IndexedItemType.STATES:
                property.intValue = EditorGUI.IntPopup(position, property.intValue, coreData.GetStateNames(), null);
                break;

            case IndexedItemAttribute.IndexedItemType.COMMAND_STATES:
                property.intValue = EditorGUI.IntPopup(position, property.intValue, coreData.GetCommandStateNames(), null);
                break;
            
        }
        //coreData.SetDirty();
    }

}
*/

