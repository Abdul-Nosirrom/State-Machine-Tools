using UnityEngine;


public class HitBox : MonoBehaviour
{
    private CharacterStateManager character;

    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int stateIndex;
    
    
    // Use this for initialization
    void Start ()
    {
        character = transform.root.GetComponent<CharacterStateManager>();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject != transform.root.gameObject)
        {
            if (character.hitActive > 0)
            {
                CharacterStateManager victim = other.transform.root.GetComponent<CharacterStateManager>();
                victim.GetHit(character);
            }
            //Debug.Log("HIT!");
        }
    }
}
