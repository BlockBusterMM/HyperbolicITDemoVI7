using System;
using UnityEngine;

public class RaycastTest : MonoBehaviour
{
    //[SerializeField]
    Complex from;

    [SerializeField]
    Complex dir;
    [SerializeField]
    float rot;

    [SerializeField]
    int detail = 10;

    private void FixedUpdate()
    {
        from = (Complex)(Vector2)transform.position;
        HRaycastHit hit;

        for(int i = 0; i<detail;i++)    HCollisionSystem.Raycast(from, Hyperbolic.ApplyAngleGyration(dir, (i * 2 * Mathf.PI) /detail), out hit);


        //HCollisionSystem.Raycast(from, dir, out hit);
        //Hyperbolic.DebugDrawHLine(from, dir, Color.red, 50);

        dir = Hyperbolic.ApplyAngleGyration(dir, rot);
    }
}
