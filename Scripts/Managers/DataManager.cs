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
public class DataManager : Singleton<DataManager>
{
    public List<CharacterData> characterData;
    
    public static float hitStop;
    
    public PlayerStateManager mainCharacter;
    

    void Update()
    {
        hitStop = hitStop > 0 ? hitStop--: hitStop;
    }

    public static void SetHitStop(float _pow)
    {
        hitStop = _pow > hitStop ? _pow : hitStop;
    }

    public void ReloadFields()
    {
#if UNITY_EDITOR
        
        string[] guids = AssetDatabase.FindAssets("t:"+ typeof(CharacterData).Name);  //FindAssets uses tags check documentation for more info
        characterData = new List<CharacterData>(guids.Length);
        for(int i =0;i<guids.Length;i++)         //probably could get optimized 
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            characterData.Add(AssetDatabase.LoadAssetAtPath<CharacterData>(path));
        }

#endif
    }

    public void ResetStateMachineInputData()
    {
        foreach (var character in characterData)
        {
            foreach (var FSM in character.character.stateMachines) FSM.ResetInputData();
        }
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

