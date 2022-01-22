using System;
using UnityEditor;
using UnityEngine;


public class AttackEditorWindow : EditorWindow
{

    [MenuItem("Window/Attack Editor")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(AttackEditorWindow),false, "Attack Editor");
    }
    

    ActionData dataAsset;
    private Character coreData;
    private int characterStateIndex;
    private CharacterState currentState;
    private Attack currentAttack;
    private HitBox hitBox;
    private HitBox[] hitBoxes;
    
    Vector2 scrollView;
    int sizer = 0;
    int sizerStep = 30;
    Vector2 xButton = new Vector2(20, 20);



    bool attackFold;


    private void OnGUI()
    {
        // Misc GUI Style
        // Quick set up of guilayout button
        var buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.hover.textColor = Color.red;
        buttonStyle.fontSize = 44;
        buttonStyle.fixedWidth = 280;
        // Label Style
        var labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontStyle = FontStyle.BoldAndItalic;
        labelStyle.fontSize = 18;
        // Get all hitboxes
        hitBoxes = FindObjectsOfType<HitBox>();



        string guid = AssetDatabase.FindAssets("t: ActionData")[0];

        dataAsset = AssetDatabase.LoadAssetAtPath<ActionData>(AssetDatabase.GUIDToAssetPath(guid));

        // Character Select Label
        coreData = dataAsset.characters[dataAsset.currentCharacterIndex];
        // Character Name
        using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
        {

            GUILayout.Label("Selected Character: ", labelStyle);

            GUILayout.BeginHorizontal();
            dataAsset.currentCharacterIndex =
                EditorGUILayout.Popup(dataAsset.currentCharacterIndex, dataAsset.GetCharacterNames());


            coreData = dataAsset.characters[dataAsset.currentCharacterIndex];



            GUILayout.EndHorizontal();
        }

        // Assign HitBox Reference for selected character
        foreach (var hitBoxRef in hitBoxes)
        {
            CharacterStateManager hitBoxParent = hitBoxRef.GetComponentInParent<CharacterStateManager>();
            if (hitBoxParent != null)
            {
                if (hitBoxParent._character.name.Equals(coreData.name))
                {
                    hitBox = hitBoxRef;
                }
            }
        }

        scrollView = GUILayout.BeginScrollView(scrollView);
        ////////////////////////////////////////////////////////////////
        /*                  State Select                              */
        ////////////////////////////////////////////////////////////////
        using (new GUILayout.HorizontalScope(EditorStyles.label))
        {
            GUILayout.Label("Character State: ", labelStyle);
            coreData.currentStateIndex = EditorGUILayout.Popup(coreData.currentStateIndex, dataAsset.GetStateNames());
            currentState = coreData.characterStates[coreData.currentStateIndex];
        }
        
        ///////////////////////////////////////////////////////////////
        /*                 Draw Attack Settings                      */
        ///////////////////////////////////////////////////////////////
        attackFold = EditorGUILayout.Foldout(attackFold, "Attacks");
        if (attackFold)
        {
            int deleteAttack = -1; 
            for (int i = 0; i < currentState.attacks.Count; i++)
            {
                currentAttack = currentState.attacks[i];
                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(30))) {deleteAttack = i; }
                EditorGUILayout.LabelField("Attack Parameters", labelStyle);
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    GUILayout.BeginHorizontal();
                    
                    GUILayout.Label(i.ToString() + " : ", GUILayout.Width(25));
                    EditorGUILayout.MinMaxSlider(ref currentAttack.start, ref currentAttack.end, 0f, currentState.length, GUILayout.Width(400));
                    GUILayout.Label(Mathf.Round(currentAttack.start).ToString() + " ~ " + Mathf.Round(currentAttack.end).ToString(), GUILayout.Width(75));
                    currentAttack.length = currentAttack.end - currentAttack.start;

                    GUILayout.EndHorizontal();
                    
                    GUILayout.BeginHorizontal();

                    currentAttack.knockback =
                        EditorGUILayout.Vector3Field("Knockback Direction: ", currentAttack.knockback, GUILayout.MaxWidth(500));

                    currentAttack.cancelWindow =
                        EditorGUILayout.FloatField("Cancel Window: ", currentAttack.cancelWindow, GUILayout.MaxWidth(300));

                    GUILayout.EndHorizontal();
                    
                }
                
                EditorGUILayout.LabelField("Hitbox Information", labelStyle);
                using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    GUILayout.BeginVertical();

                    currentAttack.hitAnim =
                        EditorGUILayout.Vector2Field("Animation Direction Parameter: ", currentAttack.hitAnim);
                    currentAttack.hitStun = EditorGUILayout.FloatField("Hit Stun", currentAttack.hitStun);
                    
                    GUILayout.EndVertical();
                    

                    if (GUILayout.Button("Apply Hitbox", buttonStyle))
                    {
                        currentAttack.hitBoxPos = hitBox.transform.localPosition;
                        currentAttack.hitBoxScale = hitBox.transform.localScale;
                    }
                    
                    
                    
                }
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }
            
            
            
            if (deleteAttack > -1) currentState.attacks.RemoveAt(deleteAttack);
            if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(35))) { currentState.attacks.Add(new Attack()); }
            GUILayout.Label("");

        }
        
        GUILayout.EndScrollView();

        EditorUtility.SetDirty(dataAsset);

    }

    void ApplyHitbox()
    {
        throw new NotImplementedException();
    }
}
