using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class CharacterManagerEditor : EditorWindow
{
    [MenuItem("State Machine/Character Manager")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(CharacterManagerEditor), false, "Character Manager Editor");
    }


    public CharacterData dataAsset;
    string characterName = "Enter Character Name Here";
    public Texture2D border;


    private void OnGUI()
    {
        var labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontStyle = FontStyle.BoldAndItalic;
        labelStyle.fontSize = 18;
        
        using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            GUILayout.BeginHorizontal();
            characterName = GUILayout.TextField(characterName); 
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("New Playable Character")) 
            { 
                PlayableCharacterData newChar = ScriptableObject.CreateInstance<PlayableCharacterData>(); 
                AssetDatabase.CreateAsset(newChar, $"Assets/Data/Characters/Playable/{characterName}.asset");
                dataAsset = newChar;
                dataAsset.character.name = characterName;
                DataManager.Instance.characterData.Add(dataAsset);
                DataManager.Instance.ReloadFields();

                AssetDatabase.SaveAssets();
            }
                
            if (GUILayout.Button("New AI Character")) 
            { 
                AICharacterData newChar = ScriptableObject.CreateInstance<AICharacterData>(); 
                AssetDatabase.CreateAsset(newChar, $"Assets/Data/Characters/AI/{characterName}.asset"); 
                dataAsset = newChar; 
                dataAsset.character.name = characterName; 
                DataManager.Instance.characterData.Add(dataAsset);
                DataManager.Instance.ReloadFields();

                AssetDatabase.SaveAssets();
            } 
            GUILayout.EndHorizontal(); 
        }
        
        if (DataManager.Instance.characterData.Count == 0) return;
        
        using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            
            GUILayout.BeginHorizontal();
            
            GUILayout.BeginHorizontal(labelStyle);
            DataManager.Instance.currentCharacterEditorIndex =
                EditorGUILayout.Popup(DataManager.Instance.currentCharacterEditorIndex, DataManager.Instance.GetCharacterNames().ToArray());
            dataAsset = DataManager.Instance.characterData[DataManager.Instance.currentCharacterEditorIndex];
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("Delete Character"))
            {
                string path;
                if (dataAsset is PlayableCharacterData)
                {
                    path = $"Assets/Data/Characters/Playable/{dataAsset.name}.asset";
                }
                else
                {
                    path = $"Assets/Data/Characters/AI/{dataAsset.name}.asset";
                }
                DataManager.Instance.characterData.Remove(dataAsset);
                if( System.IO.File.Exists(path))
                {
                    if( AssetDatabase.DeleteAsset(path) )
                        Debug.Log("deleted");
                    else
                        Debug.LogError(string.Format("Can not deleted '{0}'. Unknown error.", path));
 
                }
                else
                {
                    Debug.LogError( string.Format("Can not deleted '{0}'. File does not exist.", path) );
                }
                DataManager.Instance.currentCharacterEditorIndex = 0;
                DataManager.Instance.ReloadFields();
                AssetDatabase.Refresh();
            }

            GUILayout.EndHorizontal(); 
        }

        if (dataAsset != null)
        {
            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.BeginVertical();
                dataAsset.character.animator = EditorGUILayout.ObjectField("Animator: ", dataAsset.character.animator, typeof(AnimatorController)) as AnimatorController;
                
                dataAsset.character.characterThumbnail = EditorGUILayout.ObjectField("Character Thumbnail: ", dataAsset.character.characterThumbnail, typeof(Texture2D)) as Texture2D;

                GUILayout.EndVertical();
            }

            Texture2D image;
            if (dataAsset.character.characterThumbnail != null)
            {
                image = dataAsset.character.characterThumbnail;
                Rect areaRect = new Rect(0, 150, image.width , image.height );
                Rect imageRect = new Rect(10, 10, areaRect.width - 20, areaRect.height - 20);
                using (new GUILayout.AreaScope(areaRect, "", EditorStyles.helpBox))
                {
                    GUI.DrawTexture(imageRect, image, ScaleMode.ScaleAndCrop);
                }
            }
        }
        
    }

}