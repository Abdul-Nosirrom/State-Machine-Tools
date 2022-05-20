using UnityEngine;


public class HitBox : MonoBehaviour
{
    private StateManager character;

    [IndexedItem(IndexedItemAttribute.IndexedItemType.STATES)]
    public int stateIndex;
    
    
    // Use this for initialization
    void Start ()
    {
        character = transform.root.GetComponent<StateManager>();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject != transform.root.gameObject)
        {
            if (character.hitActive > 0)
            {
                HitReactor victim = other.transform.root.GetComponent<HitReactor>();
                if (victim != null)
                    victim.GetHit(character);
            }
            //Debug.Log("HIT!");
        }
    }
}
