using UnityEngine;

[System.Serializable]
public class Attack
{
    public float start;
    public float end;
    public float length;
    public float hitStun; // Frames to stun and inhibit control
    
    /* HitAnim is the hit direction to influence animation
       Could be swapped with a string for victim state name? Have universal controller FSM */
    public Vector2 hitAnim;
    /* Could probably process the knockback vector to be relative to this.transforms */
    public Vector3 knockback;

    public Vector3 hitBoxPos;
    public Vector3 hitBoxScale;

    public float cancelWindow;
    
    // Store what's been hit in current attack
    public GenericDictionary<string, bool> confirmedHits;
}
