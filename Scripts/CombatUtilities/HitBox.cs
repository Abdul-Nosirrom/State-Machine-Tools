using UnityEngine;


public class HitBox : MonoBehaviour
{
    private StateManager character;

    // Use this for initialization
    void Start ()
    {
        character = transform.root.GetComponent<StateManager>();
    }

    void OnTriggerStay(Collider other)
    {
        return;
        Debug.Log("Trigger Found");
        if (other.gameObject != transform.root.gameObject)
        {
            Debug.Log("Trigger Start");
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
