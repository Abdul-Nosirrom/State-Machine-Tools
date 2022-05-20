using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Swinging Event", menuName = "State Events/Player/Swinging", order=1)]
public class SwingingEvent : StateEventObject
{
    private StateManager _stateManager;
    private LineRenderer _lineRenderer;
    private Vector3 _grapplePoint, _currentSwingPos;
    private SpringJoint _joint;

    private bool alreadyGrappling = false;
    
    private float _maxDistance;
    private float _springConstant, _damping, _massScale;

    public void Execute(PlayerStateManager stateManager, float MaxGrappleDistance, float SpringConstant, float Damping, float MassScale, bool stop)
    {
        Debug.Log("Called Swing!");
        if (stop)
        {
            Debug.Log("On State Exit Sucessfuly Called!");
            _lineRenderer.positionCount = 0;
            Destroy(_joint);
            alreadyGrappling = false;
            return;
        }
        
        _maxDistance = MaxGrappleDistance;
        _springConstant = SpringConstant;
        _damping = Damping;
        _massScale = MassScale;
        
        _stateManager = stateManager;
        _lineRenderer = _stateManager.GetComponent<LineRenderer>();

        Swing();
    }

    public void Swing()
    {
        RaycastHit hit;
        var position = _stateManager.transform.position;
        var up = _stateManager.transform.up;
        if (!alreadyGrappling && Physics.Raycast(position, up, out hit, _maxDistance))
        {
            alreadyGrappling = true;
            _grapplePoint = hit.point;
            
            _joint = _stateManager.gameObject.AddComponent<SpringJoint>();
            _lineRenderer = _stateManager.gameObject.GetComponent<LineRenderer>();
            
            _joint.autoConfigureConnectedAnchor = false;
            _joint.connectedAnchor = _grapplePoint;
            
            float distanceFromPoint = Vector3.Distance(position, _grapplePoint);

            _joint.maxDistance = distanceFromPoint * 0.5f;
            _joint.minDistance = distanceFromPoint * 0.5f;

            _joint.spring = 4.5f;

            _lineRenderer.positionCount = 2;
            _currentSwingPos = position;
            alreadyGrappling = true;
        }

        // Draw Line
        _currentSwingPos = Vector3.Lerp(_currentSwingPos, _grapplePoint, Time.fixedDeltaTime * 8f);
        
        _lineRenderer.SetPosition(0, _stateManager.transform.position);
        _lineRenderer.SetPosition(1, _grapplePoint);
        
    }

    public Vector3 EoM(Vector3 vel)
    {

        return Vector3.zero;
    }
    

}