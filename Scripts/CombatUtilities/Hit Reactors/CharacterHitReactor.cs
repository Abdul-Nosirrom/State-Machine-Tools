
using System;

public class CharacterHitReactor : HitReactor
{
    private StateManager _stateManager;

    private void Start()
    {
        _stateManager = GetComponent<StateManager>();
    }

    public override void GetHit(StateManager _attacker)
    {
        Attack curAtk = _attacker.currentStateDefinition.attacks[_attacker.currentAttackIndex];
        // Don't hit twice in same attack
        if (curAtk.confirmedHits.ContainsKey(_reactorID)) return;
        
        // Apply Knockback
        // Change State
        // Change Material if needed
        // Any status effects
        _stateManager.hitStun = curAtk.hitStun;
        DataManager.SetHitStop(curAtk.hitStun);
        _attacker.hitConfirm++;
        // We can use a hashmap and add enemies to it, check if theyre there as to not hit them twice
        curAtk.confirmedHits.Add(_reactorID, true);
        // Call global prefab for getting hit effect
        // We can set attack VFX to appear on a different camera thats the child of the player or whatever
        // Set its layer to something different in main, and set it only to attackVFX in camera
        // Setup explained in part 6 at 30 minutes
    }

    public override void GettingHit()
    {
        _stateManager.hitStun--;
    }
}