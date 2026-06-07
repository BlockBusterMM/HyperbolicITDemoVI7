using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public struct Complex
{
    public float Re, Im;

    public Complex(float re, float im)
    {
        Re = re; Im = im;
    }
    static public implicit operator Complex(float a)
    {
        return new Complex(a, 0);
    }
    static public explicit operator Complex(Vector2 a)
    {
        return new Complex(a.x, a.y);
    }
    static public explicit operator Vector2(Complex a)
    {
        return new Vector2(a.Re, a.Im);
    }
    static public Complex operator +(Complex a, Complex b)
    {
        return new Complex(a.Re + b.Re, a.Im + b.Im);
    }
    static public Complex operator -(Complex a)
    {
        return new Complex(-a.Re, -a.Im);
    }
    static public Complex operator -(Complex a, Complex b)
    {
        return a + (-b);
    }
    static public Complex operator *(Complex a, float b)
    {
        return new Complex(a.Re * b, a.Im * b);
    }
    static public Complex operator *(Complex a, Complex b)
    {
        return new Complex(a.Re*b.Re - a.Im*b.Im, a.Re*b.Im + a.Im*b.Re);
    }
    static public Complex operator /(Complex a, float b)
    {
        return a * (1 / b);
    }
    static public Complex operator /(Complex a, Complex b)
    {
        return a*(b.conj() / b.sqrLength());
    }
    public Complex conj()
    {
        return new Complex(Re, -Im);
    }
    public float sqrLength()
    {
        return Re*Re + Im*Im;
    }

    public Complex normalized()
    {
        if (sqrLength()==0)
            return 0;
        return this / Mathf.Sqrt(sqrLength());
    }

    public Complex arg()
    {
        return Vector2.SignedAngle(Vector2.right, (Vector2)this);
    }

    public Complex CastOnto(Complex onto)
    {
        //this is incorrect in the hyperbolic plane
        return (Complex)(Vector2)Vector3.Project((Vector2)this, (Vector2)onto);
    }
}

public static class Hyperbolic
{
    //the Mobius disk will always be of s=1 and is also the Poincare Disk Model
    public static Complex VectorAddition(Complex a, Complex b)
    {
        return (a + b) / (1 + (a.conj() * b));
    }

    public static Complex MobiusComplexGyration(Complex a, Complex b)
    {
        Complex Out = 1+(a.conj() * b);
        return (Out * Out) / Out.sqrLength();
    }
    public static float MobiusAngleGyration(Complex a, Complex b)
    {
        Complex Out = 1 + (a.conj() * b);
        return 2*Mathf.Atan(Out.Im / Out.Re);
    }

    public static Complex ApplyAngleGyration(Complex z, float angle)
    {
        //just rotate the vector z counter-clockwise by the given angle
        var rot = Quaternion.Euler(new Vector3(0, 0, -360*angle/(2*Mathf.PI))) * new Vector3(z.Re, z.Im, 0);
        return new Complex(rot.x, rot.y);
    }

    public static Complex ScalarMult(float r, Complex z)
    {
        //Debug.Log((float)Math.Atanh(Mathf.Sqrt(z.sqrLength())));
        return (float)Math.Tanh(r * (float)Math.Atanh(Mathf.Sqrt(z.sqrLength()))) * z.normalized();
    }
    /// <summary>
    /// Method <c>TriangleSideLength</c>/c> returns the squared length of the side opposite to angle alpha.
    /// </summary>
    public static float SqrTriangleSideLength(float alpha, float beta, float gamma)
    {
        return (Mathf.Cos(alpha) + Mathf.Cos(beta + gamma)) / (Mathf.Cos(alpha) + Mathf.Cos(beta - gamma));
    }

    public static void DebugDrawHLine(Complex from, Complex dir, Color color, int detail = 10, bool obeyCam = false)
    {
        dir = CameraBrain.gyroposition.ApplyAtToDirection(from, dir);
        from = CameraBrain.gyroposition.Apply(from);

        Complex a,b=from;
        for (float i = 1; i <= detail; i++)
        {
            a = b;
            b = VectorAddition(from, dir * (i / detail));
            Debug.DrawLine((Vector2)a,(Vector2)b, color);
        }
    }

    public static void DebugDrawHLine(Complex from, Complex dir, int detail = 10, bool obeyCam = false)
    {
        DebugDrawHLine(from, dir, Color.white, detail, obeyCam);
    }
    /*
    public static float LineDist(Complex v, Complex onto)
    {
        //LineDist(v,onto)*i*onto + v is parallel to onto
        return 0;
    }
    */
}

[Serializable]
public struct MobiusGyroposition
{
    //which is the transformation function:
    //f(z) = Gyrovector +m Gyration*z

    public Complex Gyrovector;
    public float Gyration;

    public MobiusGyroposition(Vector2 gyrovector, float gyration)
    {
        Gyrovector = (Complex)gyrovector;
        Gyration = gyration;
    }
    public MobiusGyroposition(Complex gyrovector, float gyration)
    {
        Gyrovector = gyrovector;
        Gyration = gyration;
    }
    public MobiusGyroposition LMovedBy(Complex delta)
    {
        var Out = new MobiusGyroposition();
        Out.Gyration = Mathf.Repeat(Hyperbolic.MobiusAngleGyration(delta, Gyrovector) + Gyration, 2 * Mathf.PI);
        Out.Gyrovector = Hyperbolic.VectorAddition(delta, Gyrovector);
        return Out;
    }
    public void LMoveBy(Complex delta)
    {
        this = LMovedBy(delta);
    }

    // THIS IS NOT A PROPER TRANSLATION OF THE TRANSFORMATION!!!
    // it makes the 0 point move along a gyroline
    public MobiusGyroposition RMovedBy(Complex delta)
    {
        var Out = new MobiusGyroposition();
        Out.Gyration = Mathf.Repeat(Hyperbolic.MobiusAngleGyration(Gyrovector, delta) + Gyration, 2 * Mathf.PI);
        Out.Gyrovector = Hyperbolic.VectorAddition(Gyrovector, delta);
        return Out;
    }
    public void RMoveBy(Complex delta)
    {
        this = RMovedBy(delta);
    }

    public MobiusGyroposition Inverse()
    {
        return new MobiusGyroposition(Hyperbolic.ApplyAngleGyration(-Gyrovector, -Gyration), -Gyration);
    }

    public void RotateBy(float angle)
    {
        Gyration = Mathf.Repeat(angle + Gyration, 2 * Mathf.PI);
        Gyrovector = Hyperbolic.ApplyAngleGyration(Gyrovector, angle);
    }

    public Complex Apply(Complex z)
    {
        return Hyperbolic.VectorAddition(Gyrovector, Hyperbolic.ApplyAngleGyration(z, Gyration));
    }

    public MobiusGyroposition Apply(MobiusGyroposition z)
    {
        return new MobiusGyroposition(Hyperbolic.ApplyAngleGyration(z.Gyrovector, Gyration), z.Gyration + Gyration).LMovedBy(Gyrovector);
    }

    public Complex ApplyAtToDirection(Complex from, Complex dir)
    {
        dir = Hyperbolic.ApplyAngleGyration(dir, Gyration);
        dir *= Hyperbolic.MobiusComplexGyration(Hyperbolic.ApplyAngleGyration(from, Gyration), Gyrovector);
        return dir;
    }
}
