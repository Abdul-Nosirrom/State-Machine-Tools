using UnityEngine;

public class VFXControl : StateMachineBehaviour
{
    // DeparentTime is time/tick based not frame based
    public float deparentTime = 1;
    public Animator myAnimator;
    public Transform vfxRoot;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        myAnimator = animator;
    }

    public void Deparent()
    {
        if (vfxRoot != null)
        {
            if (vfxRoot.parent != null && vfxRoot.parent != vfxRoot)
            {
                vfxRoot.SetParent(null);
            }
        }
    }

    public void DestroySelf()
    {
        // Check to destroy object in case where it has/hasn't been deparented
        Destroy(vfxRoot != null ? vfxRoot.gameObject : myAnimator.gameObject);
    }


    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime >= deparentTime) { Deparent(); }
        if (stateInfo.normalizedTime >= 1) { DestroySelf(); }
    }
}
    
