using System;
using UnityEngine;
using UnityEngine.UI;

// Adding execute always attribute such that the inspector can get its data from here always

[ExecuteAlways]
public class EngineData : MonoBehaviour
{
    public ActionData actionDataObject;
    public static ActionData actionData;
    public float deadZone = 0.2f;

    public static float hitStop;

    public static EngineData engineData;

    public InputBuffer playerInputBuffer;

    public CharacterStateManager mainCharacter;

    public int globalMoveListIndex;

    void Update()
    {
        hitStop = hitStop > 0 ? hitStop--: hitStop;
    }

    public MoveList CurrentMoveList(int _char)
    {
        return actionData.characters[_char].moveLists[globalMoveListIndex];
    }

    public static void SetHitStop(float _pow)
    {
        hitStop = _pow > hitStop ? _pow : hitStop;
    }
    //private void Start()
    private void OnValidate()
    {
        // To save when edits are made
        actionData = actionDataObject;
        engineData = this;
    }
    
    // Prefab generator thing
    public static void GlobalPrefab(int _index, int _char, GameObject _obj, int _state, int _ev)
    {
        GameObject nextPrefab = Instantiate(actionData.characters[_char].globalPrefabs[_index], _obj.transform.position, Quaternion.identity, _obj.transform.root);
        
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
                StateEvent thisEvent = actionData.characters[_char].characterStates[_state].events[_ev];
                myAnimator.speed *= (float)thisEvent.parameters[9].val.GetVal();
            }
            // Uncomment the below if there's a lag in the VFX animation update
            //myAnimator.Update(Time.deltaTime);
        }   
    }
    

    void OnGUI()
    {
        if (Application.isPlaying)
        {
            int xSpace = 25;
            int ySpace = 15;
            //GUI.Label(new Rect(10, 10, 100, 20), "Hello World!");
            for (int i = 0; i < playerInputBuffer.buttonCommandCheck.Count; i++)
            {
                GUI.Label(new Rect(10f + (i * xSpace), 15f, 100, 20),
                    playerInputBuffer.buttonCommandCheck[i].ToString());
            }

            for (int b = 0; b < playerInputBuffer.buffer.Count; b++)
            {
                //GUI.Label(new Rect(xSpace - 10f, b * ySpace, 100, 20), b.ToString() + ":");
                for (int i = 0; i < playerInputBuffer.buffer[b].rawInputs.Count; i++)
                {
                    if (playerInputBuffer.buffer[b].rawInputs[i].used)
                    {
                        GUI.Label(new Rect(10f + (i * xSpace), 35f + (b * ySpace), 100, 20),
                            playerInputBuffer.buffer[b].rawInputs[i].hold.ToString("0") + ">");
                    }
                    else
                    {
                        GUI.Label(new Rect(10f + (i * xSpace), 35f + (b * ySpace), 100, 20),
                            playerInputBuffer.buffer[b].rawInputs[i].hold.ToString("0"));
                    }
                }
            }

            for (int m = 0; m < playerInputBuffer.motionCommandCheck.Count; m++)
            {
                GUI.Label(new Rect(500f - 25f, m * ySpace, 100, 20),
                    playerInputBuffer.motionCommandCheck[m].ToString());
                GUI.Label(new Rect(500f, m * ySpace, 100, 20), actionData.motionCommands[m].name);

            }

            // CHANGE THE CURRENT MOVE LIST CHARACTER INDEX IT IS CURRENTLY TEMPORARY AND SET TO DEFAULT
            GUI.Label(new Rect(600f, 10f, 100, 20), CurrentMoveList(0).name.ToString());

        }
    }
}
