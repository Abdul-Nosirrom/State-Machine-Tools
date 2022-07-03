
using System;
using UnityEngine;

public static class Integration
{
    #region Vector3
    public static Vector3 Euler(Vector3 vel, Vector3 pos, Func<Vector3, Vector3, Vector3> EoM, float dT)
    {
        return vel + EoM(pos, vel) * dT;
    }

    /// <summary>
    /// Assumption for 2nd order Diff Eqs is that when decomposed, the EoM for position is just
    /// x_{i+1} = x_i + v_{i} dt
    /// Also assuming none of the EoM have any explicit time-dependence, but this should be easy to add
    /// to the parameter of EoM and just evolving it according to RK4 (pass in t + dT/2 for example)
    /// Time dependent forces could be useful in the case of oscillating potentials or something/wind zones
    /// idk
    /// </summary>
    /// <param name="vel"></param>
    /// <param name="pos"></param>
    /// <param name="EoM"></param>
    /// <param name="dT"></param>
    /// <returns></returns>
    public static Vector3 RK4(StateManager stateManager, Vector3 vel, Vector3 pos, Func<StateManager, Vector3, Vector3, Vector3> EoM, float dT)
    {
        Vector3 k1x, k2x, k3x, k4x;
        Vector3 k1v, k2v, k3v, k4v;
        Vector3 dTvec = Vector3.one * dT;

        k1v = EoM(stateManager, pos, vel);
        k1x = vel;

        k2v = EoM(stateManager, pos + dT * k1x / 2, vel + dT * k1v / 2);
        k2x = vel + dT * k1v / 2;

        k3v = EoM(stateManager, pos + dT * k2x / 2, vel + dT * k2v / 2);
        k3x = vel + dT * k3v;

        k4v = EoM(stateManager, pos + dT * k3x, vel + dT * k3v);
        k4x = vel + dT * k3v;
        
        return vel + dT * (k1v + k2v + k3v + k4v) / 6;
    }
    
    #endregion
    
}
