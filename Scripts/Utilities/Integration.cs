
using System;
using UnityEngine;

public static class Integration
{
    #region Vector3
    public static Vector3 Euler(Vector3 vel, Func<Vector3, Vector3> EoM, float dT)
    {
        return vel + EoM(vel) * dT;
    }

    public static Vector3 RK4(Vector3 vel, Func<Vector3, Vector3> EoM, float dT)
    {

        return Vector3.zero;
    }

    public static Vector3 Verlet(Vector3 vel, Vector3 pos, Func<Vector3, Vector3> EoM, float dT)
    {
        vel += 0.5f * EoM(vel) * dT;
        pos += vel * dT;

        return Vector3.zero;
    }
    
    #endregion
    
}

public class IntegrationTest
{
    public Vector3 BasicMovementEOM(Vector3 vel)
    {
        return Vector3.zero;
    }

    public void Test()
    {
        Integration.Euler(Vector3.forward, BasicMovementEOM, Time.deltaTime);
    }
}