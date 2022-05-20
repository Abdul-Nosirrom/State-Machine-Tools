using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HitBox))] 
public class InspectorToolsHB : UnityEditor.Editor
{
    public int attackEventIndex;
    public CharacterData characterData;
    public CharacterState state;
    public int stateIndex;
    public override void OnInspectorGUI()
    {
        HitBox h = (HitBox)target;
        DrawDefaultInspector ();
/*
        if (characterData == null)
        {
            foreach (string guid in AssetDatabase.FindAssets("t: ActionData"))
            {
                characterData = AssetDatabase.LoadAssetAtPath<CharacterData>(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        //t.position = EditorGUILayout.Vector3Field("Position", t.position);
        if (GUILayout.Button ("Apply HitBox"))
        {
            //AttackEvent atk = act.attacks[attackEventIndex];
            foreach (var _character in characterData.characters)
            {
                state = _character.characterStates[h.stateIndex];
                for (int i = 0; i < state.attacks.Count; i++)
                {
                    Attack atk = state.attacks[i];
                    atk.hitBoxPos = h.transform.localPosition;
                    atk.hitBoxScale = h.transform.localScale;

                    //atk.attackBoxPos = h.transform.localPosition;
                    //atk.attackBoxSca = h.transform.localScale;
                }

                EditorUtility.SetDirty(characterData);
                AssetDatabase.SaveAssets();
            }

        } */
    }
}
