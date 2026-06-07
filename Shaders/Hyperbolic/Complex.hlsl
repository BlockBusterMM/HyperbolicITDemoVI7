#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

//this is all complex number arhithmetic

void Mult_float(float2 A, float2 B, out float2 Out)
{
    Out = float2(A[0] * B[0] - A[1] * B[1], A[0] * B[1] + A[1] * B[0]);
}

void Conj_float(float2 A, out float2 Out)
{
    Out = float2(A[0], -A[1]);
}

void AddMobius_float(float2 A, float2 B, out float2 Out)
{
    //we wanna get (a+b)/(1+conjugate(a)*b))
    
    //which we calculate by expanding using the factor Expander = (1+u*conjugate(v))
    float2 Div = float2(1 + A[0] * B[0] + A[1] * B[1], A[0] * B[1] - A[1] * B[0]);
    
    //this is equal to 
    float2 Expander = float2(Div[0], - A[0] * B[1] + A[1] * B[0]);
    float2 Nom = A + B;
    Nom = float2(Nom[0] * Expander[0] - Nom[1] * Expander[1], Nom[0] * Expander[1] + Nom[1] * Expander[0]);
    Out = Nom / (Div[0]*Div[0] + Div[1]*Div[1]);
}

void MobiusToEinstein_float(float2 z, out float2 Out)
{
    //out = z* 2/(1+|z|^2)
    float times = z.x * z.x + z.y * z.y;
    times = 2 / (1 + times);
    
    Out = z * times;
}

void MobiusToUngar_float(float2 z, out float2 Out)
{
    //out = z* 2/(1-|z|^2)
    float times = z.x * z.x + z.y * z.y;
    times = 2 / (1 - times);
    
    Out = z * times;
}

#endif