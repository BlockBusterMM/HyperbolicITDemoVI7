using UnityEngine;

public class TileHCollider : HCollider
{
    public MobiusGyroposition gyroposition;

    public float sideLength = 0.555892970251421171992048047f;//????//0.48586827175664567818286387f;
    public float halfSideLength = 0.303558658726646730349328588f;    //just use scalar mult
    public float halfSpanningLength = 0.25926358732362497806075346f;
    //the distance from the middle to the sides, being basically half the length of the segment perpendicular to the sides that goes through the middle
    //where a half is calculated using the multiplication formula: |(1/2)*a|=(1-sqrt(1-|a|^2))/|a|
    //in the 4^5 tiling case this should be equal to around: 0.25926358732362497806075346f

    public override bool DoesCollideWith(Complex point)
    {

        //this is the most precise check that can be done
        point = gyroposition.Inverse().Apply(point);

        //U
        if (Hyperbolic.VectorAddition(-(Complex)Vector2.up * halfSpanningLength, point).Im >= 0)
            return false;
        //D
        if (Hyperbolic.VectorAddition(-(Complex)Vector2.down * halfSpanningLength, point).Im <= 0)
            return false;
        //R
        if (Hyperbolic.VectorAddition(-(Complex)Vector2.right * halfSpanningLength, point).Re >= 0)
            return false;
        //L
        if (Hyperbolic.VectorAddition(-(Complex)Vector2.left * halfSpanningLength, point).Re <= 0)
            return false;

        return true;
    }

    public override Complex NormalOfClosestFace(Complex point)
    {
        point = gyroposition.Inverse().Apply(point);

        //divide the plane into 4 sectors
        Complex Out;
        if(point.Re > point.Im)
            Out = new Complex(0.5f, -0.5f);
        else
            Out = new Complex(-0.5f, 0.5f);

        if (point.Re > -point.Im)
            Out = Out + new Complex(-0.5f, -0.5f);
        else
            Out = Out + new Complex(0.5f, 0.5f);

        return Hyperbolic.ApplyAngleGyration(Out, gyroposition.Gyration);
    }

    

    private bool RaycastFace(Complex from, Complex dir, out HRaycastHit hit, Complex normal, float length=1)
    {
        hit = new HRaycastHit();

        from = from.conj();
        from *= normal * new Complex(0, 1);
        dir = dir.conj();
        dir *= normal * new Complex(0,1);

        Complex translation = new Complex(0, -halfSpanningLength);
        dir *= Hyperbolic.MobiusComplexGyration(from, translation); //WHY IS THE GYRATION SWAPPED???
        from = Hyperbolic.VectorAddition(translation, from);

        if(from.Im < 0)
        {
            hit.hit = false;
            hit.length = 1;

            return false;
        }

        //check if the ray will even intersect OX
        //that is if its between -p-1 and -p+1
        
        if (dir.Im >= 0
            || Vector3.Cross((Vector2)Hyperbolic.VectorAddition(-from, -halfSideLength), (Vector2)dir).z * Vector3.Cross((Vector2)dir, (Vector2)Hyperbolic.VectorAddition(-from, halfSideLength)).z <= 0)
        {
            //this if handles the case where the ray is never gonna intersect 0X
            hit.hit = false;
            hit.length = 1;

            return false;
        }
        Complex p=from;
        int i;
        //NOT binary search??
        for(i=1; p.Im>0; i++)
        {
            p = Hyperbolic.VectorAddition(from, Hyperbolic.ScalarMult(i, dir * HCollisionSystem.skinWidth));
            //and here we suddenly do right addition??
            //its correct tho
            if (Hyperbolic.ScalarMult(i, HCollisionSystem.skinWidth).Re > length)
            {
                hit.hit = false;
                hit.length = 1;
                return false;
            }
        }

        hit.hit = true;
        hit.length = Hyperbolic.ScalarMult(i - 1, HCollisionSystem.skinWidth).Re;

        hit.normal = Hyperbolic.MobiusComplexGyration(Hyperbolic.VectorAddition(from, dir * hit.length), -translation).conj() * normal;

        return true;
    }

    public override bool Raycast(Complex from, Complex dir, out HRaycastHit hit, float length = 1)
    {
        hit = new HRaycastHit();
        if(DoesCollideWith(from))
        {
            hit.hit = true;
            hit.stuck = true;
            hit.length = 0;
            return true;
        }

        hit.stuck = false;
        hit.hit = false;
        hit.length = length;

        //some math bull stuff
        dir = gyroposition.Inverse().ApplyAtToDirection(from, dir);

        from = gyroposition.Inverse().Apply(from);

        HRaycastHit Uh, Rh, Dh, Lh;
        RaycastFace(from, dir, out Uh, new Complex(0, 1), length);
        RaycastFace(from, dir, out Rh, new Complex(1, 0), length);
        RaycastFace(from, dir, out Dh, new Complex(0, -1), length);
        RaycastFace(from, dir, out Lh, new Complex(-1, 0), length);

        if (Uh && Uh.length <= hit.length)
        {
            hit.hit = true;
            hit.length = Uh.length;
            hit.normal = Uh.normal;
        }
        if (Rh && Rh.length <= hit.length)
        {
            hit.hit = true;
            hit.length = Rh.length;
            hit.normal = Rh.normal;
        }
        if (Dh && Dh.length <= hit.length)
        {
            hit.hit = true;
            hit.length = Dh.length;
            hit.normal = Dh.normal;
        }
        if (Lh && Lh.length <= hit.length)
        {
            hit.hit = true;
            hit.length = Lh.length;
            hit.normal = Lh.normal;
        }

        if (hit.hit == false)
            return false;

        hit.hCollider = this;
        hit.hitPoint = Hyperbolic.VectorAddition(from, hit.length * dir);

        hit.normal = gyroposition.ApplyAtToDirection(hit.hitPoint, hit.normal);
        hit.hitPoint = gyroposition.Apply(hit.hitPoint);


        return true;
    }
}
