using UnityEngine;


/// <summary>
/// For effects and such, let them always face the camera
/// Similarly for enemy healthbars over their heads, UI stuff, etc...
/// </summary>
public class Billboard : MonoBehaviour
{
    public bool reverse;

    public Vector3 rotMin;
    public Vector3 rotMax;

    public bool strikeAngle;
    [UnityEngine.HideInInspector]
    Vector3 rotationOffset;
    
    void Start ()
    {
        rotationOffset = new Vector3(Random.Range(rotMin.x, rotMax.x), Random.Range(rotMin.y, rotMax.y), Random.Range(rotMin.z, rotMax.z));
    }
	
    
    void Update ()
    {
        if (reverse)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * -Vector3.forward,
                Camera.main.transform.rotation * Vector3.up);
            
        }
        else
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up);
        }
        transform.Rotate(rotationOffset);
    }        
}
