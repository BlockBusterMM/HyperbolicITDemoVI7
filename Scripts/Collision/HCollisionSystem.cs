using System.Collections.Generic;
using UnityEngine;

public struct HCollision
{
    public HCollider collider;
    public Complex normal;
}

public static class HCollisionSystem
{
    public const float skinWidth = 0.0005f;
    public static List<HCollider> world = new List<HCollider>();

    public static bool DoesPointCollideWithWorld(Complex point)
    {
        //super inefficient at the moment
        foreach (var collider in world)
        {
            if (collider.DoesCollideWith(point))
            {
                //Debug.Log(collider.gameObject.name);
                return true;
            }
        }
        return false;
    }
    public static bool DoesPointCollideWithWorld(Complex point, out HCollision collision)
    {
        //super inefficient at the moment
        foreach(var collider in world)
        {
            if (collider.DoesCollideWith(point))
            {
                Debug.Log(collider.gameObject.name);
                collision.collider = collider;
                collision.normal = collider.NormalOfClosestFace(point);
                return true;
            }
        }
        collision = new HCollision();
        return false;
    }


    public static bool Raycast(Complex from, Complex dir, out HRaycastHit hit, float length = 1)
    {
        {
            HCollision collision;
            if (DoesPointCollideWithWorld(from, out collision))
            {
                hit = new HRaycastHit();
                hit.hit = true;
                hit.hitPoint = from;
                hit.stuck = true;
                hit.length = 0;
                hit.hCollider = collision.collider;
                hit.normal = (Complex)Vector2.zero;

                //Debug.LogWarning("raycast from inside world");
                return true;
            }
        }

        dir = dir.normalized();
        HRaycastHit Nh;

        hit = new HRaycastHit();
        hit.length = length;
        hit.hit = false;

        foreach (var collider in world)
        {
            if (!collider.Raycast(from, dir, out Nh, hit.length))
                continue;
            hit = Nh;
        }

        //this doesnt seem to be the problem
        if (DoesPointCollideWithWorld(Hyperbolic.VectorAddition(from, dir * (hit.length - skinWidth))))
        {

            //int i;
            //for(i=1;i<20;i++)
            //{
            //    if (!DoesPointCollideWithWorld(Hyperbolic.VectorAddition(from, dir * (hit.length - skinWidth * i))))
            //        break;
            //}
            //Debug.LogError("The point is inside world after raycast. " + i.ToString() + " skinWidths deep.");
            Debug.LogWarning("The point is inside world after raycast.");
        }

        return false;
    }
}

public struct HRaycastHit
{
    public bool hit;
    //length of the ray before hitting or ending
    public float length;
    //if ray starts inside
    public bool stuck;

    //theese values will not be assigned if the ray did not hit
    public HCollider hCollider;
    public Complex normal;
    public Complex hitPoint;

    public static implicit operator bool(HRaycastHit raycastHit)
    {
        return raycastHit.hit;
    }
}

public abstract class HCollider : MonoBehaviour
{
    void Awake()
    {
        HCollisionSystem.world.Add(this);
    }

    private void OnDestroy()
    {
        HCollisionSystem.world.Remove(this);
    }

    public abstract bool DoesCollideWith(Complex point);
    public abstract Complex NormalOfClosestFace(Complex point);
    public abstract bool Raycast(Complex from, Complex dir, out HRaycastHit hit, float length = 1);
}