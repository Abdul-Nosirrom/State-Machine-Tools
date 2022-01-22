using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CharacterScriptEditorWindow : EditorWindow
{
    [MenuItem("Window/Character Script Editor")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(CharacterScriptEditorWindow), false, "Event Script Editor");
    }
    
    ActionData dataAsset;
    private Character coreData;

    Vector2 scrollView;
    private void OnGUI()
    {
        
        if (dataAsset == null)
        {
            foreach (string guid in AssetDatabase.FindAssets("t: ActionData"))
            {
                dataAsset = AssetDatabase.LoadAssetAtPath<ActionData>(AssetDatabase.GUIDToAssetPath(guid));
            }
        }
        
        // Character Select Label
        coreData = dataAsset.characters[dataAsset.currentCharacterIndex];
        // Character Name
        using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            //GUILayout.Label("");


            GUILayout.BeginHorizontal();
            dataAsset.currentCharacterIndex =
                EditorGUILayout.Popup(dataAsset.currentCharacterIndex, dataAsset.GetCharacterNames());


            coreData = dataAsset.characters[dataAsset.currentCharacterIndex];



            GUILayout.EndHorizontal();
        }
        
        ////////////////////////////////////////////////////////////////

        scrollView = GUILayout.BeginScrollView(scrollView);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Script Index : " + coreData.currentScriptIndex.ToString());
        coreData.currentScriptIndex = EditorGUILayout.Popup(coreData.currentScriptIndex, dataAsset.GetScriptNames());
        if (GUILayout.Button("New Event Script")) { coreData.eventScripts.Add(new EventScript()); coreData.currentScriptIndex = coreData.eventScripts.Count - 1; }
        GUILayout.EndHorizontal();
        
        EventScript currentScript = coreData.eventScripts[coreData.currentScriptIndex];

        currentScript.eventName = EditorGUILayout.TextField("Name : ", currentScript.eventName);

        int deleteParam = -1;

        for(int p = 0; p < currentScript.parameters.Count;p++)
        {
            EventParameter currentParam = currentScript.parameters[p];
            GUILayout.BeginHorizontal();
            currentParam.name = EditorGUILayout.TextField("Parameter Name : ", currentParam.name);
            if (GUILayout.Button("x", GUILayout.Width(25))) { deleteParam = p; }
            GUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            currentParam.val.paramType = (ParameterType.SupportedTypes)EditorGUILayout.EnumPopup("Parameter Type:", currentParam.val.paramType);            EditorGUILayout.EndHorizontal();
            Debug.Log("SEEKING TYPE: " + currentParam.val.paramType);
            switch (currentParam.val.paramType)
            {
                case ParameterType.SupportedTypes.FLOAT:
                    currentParam.val.floatVal = EditorGUILayout.FloatField("Default Value : ", currentParam.val.floatVal);
                    Debug.Log("FLOAT TYPE");
                    break;
                case ParameterType.SupportedTypes.BOOL:
                    currentParam.val.boolVal = EditorGUILayout.Toggle("Default Value : ", currentParam.val.boolVal);
                    break;
                case ParameterType.SupportedTypes.ANIMATION_CURVE:
                    currentParam.val.curveVal = EditorGUILayout.CurveField("Default Value : ", currentParam.val.curveVal);
                    break;
            }
            //currentParam.val = EditorGUILayout.FloatField("Default Value : ", currentParam.val);

            
        }

        if (deleteParam > -1) { currentScript.parameters.RemoveAt(deleteParam); }

        if(GUILayout.Button("+", GUILayout.Width(25)))
        {
            currentScript.parameters.Add(new EventParameter());
        }


        GUILayout.EndScrollView();
        EditorUtility.SetDirty(dataAsset);
    }
}