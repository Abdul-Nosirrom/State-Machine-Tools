using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Adding execute always attribute such that the inspector can get its data from here always

[ExecuteInEditMode]
public class DataManager : EditorSingleton<DataManager>
{
    public List<CharacterData> characterData;
    
    public float deadZone = 0.2f;

    public static float hitStop;
    
    public PlayerStateManager mainCharacter;
    
    // FOR EDITOR USE
    public int currentCharacterEditorIndex;

    void Update()
    {
        hitStop = hitStop > 0 ? hitStop--: hitStop;
    }

    private void OnEnable()
    {
        //AssemblyReloadEvents.afterAssemblyReload += ReloadFields;
        //AssemblyReloadEvents.afterAssemblyReload += Awake;
    }

    private void OnDisable()
    {
        //AssemblyReloadEvents.afterAssemblyReload -= ReloadFields;
        //AssemblyReloadEvents.afterAssemblyReload -= Awake;
    }

    public static void SetHitStop(float _pow)
    {
        hitStop = _pow > hitStop ? _pow : hitStop;
    }

    public void ReloadFields()
    {
#if UNITY_EDITOR
        currentCharacterEditorIndex = 0;
        
        string[] guids = AssetDatabase.FindAssets("t:"+ typeof(CharacterData).Name);  //FindAssets uses tags check documentation for more info
        characterData = new List<CharacterData>(guids.Length);
        for(int i =0;i<guids.Length;i++)         //probably could get optimized 
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            characterData.Add(AssetDatabase.LoadAssetAtPath<CharacterData>(path));
        }

        GetCharacterNames();
#endif
    }

    // Setup Action Data
    private void Start()
    {
        
        ReloadFields();
        // Load UI and Manager Scenes
        if (Application.isPlaying)
        {
            //SceneManager.LoadScene("Debug UI", LoadSceneMode.Additive);
            //SceneManager.LoadScene("Base Test", LoadSceneMode.Additive);
        }
    }

    public List<string> GetCharacterNames()
    {
        List<string> characterNames = new List<string>();
        foreach (CharacterData charData in characterData)
        {
            if (charData != null)
                characterNames.Add(charData.character.name);
        }

        return characterNames;
    }


/*    
    // Prefab generator thing
    public static void GlobalPrefab(int _index, int _char, GameObject _obj, int _state, int _ev)
    {
        GameObject nextPrefab = Instantiate(characterData.characters[_char].globalPrefabs[_index], _obj.transform.position, Quaternion.identity, _obj.transform.root);
        
        foreach (Animator myAnimator in nextPrefab.transform.GetComponentsInChildren<Animator>())
        {
            // Behaviors as a way to do FSMs in mecanim rather than custom? 
            VFXControl[] behaves = myAnimator.GetBehaviours<VFXControl>();

            for (int i = 0; i < behaves.Length; i++)
            {
                behaves[i].vfxRoot = nextPrefab.transform;
            }

            if (_state != -1)
            {
                StateEvent thisEvent = characterData.characters[_char].characterStates[_state].events[_ev];
                myAnimator.speed *= (float)thisEvent.parameters[9].val.GetVal();
            }
            // Uncomment the below if there's a lag in the VFX animation update
            //myAnimator.Update(Time.deltaTime);
        }   
    }
    
*/

}
