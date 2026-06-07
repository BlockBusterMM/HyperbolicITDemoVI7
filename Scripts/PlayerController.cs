using System.Net.Http;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    float moveSpeed = 0.6f;
    [SerializeField]
    float jumpPower = 0.01f;
    [SerializeField]
    float gravity = 0.1f;

    [SerializeField]
    float drag = 0.95f;
    [SerializeField]
    float groundDrag = 0.7f;

    //tg of the max slope angle
    [SerializeField]
    float maxSlope = 0.5f;

    [SerializeField]
    Complex relative_velocity;

    bool Grounded = false;

    Complex true_velocity { get { return Hyperbolic.ApplyAngleGyration(relative_velocity, gyroposition.Gyration); } }

    public static MobiusGyroposition gyroposition = new MobiusGyroposition(0, 0);

    MeshRenderer meshRenderer;

    InputAction moveAction;
    void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        meshRenderer = GetComponent<MeshRenderer>();

        TestingController.Restart_e += Restart;
    }

    void Restart()
    {
        relative_velocity = (Complex)Vector2.zero;
        gyroposition = new MobiusGyroposition();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        CheckGrounded();
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        relative_velocity *= drag;
        if (Grounded)
            relative_velocity *= groundDrag;

        relative_velocity += (Complex)Vector2.right * moveInput.x * moveSpeed;
        relative_velocity += (Complex)Vector2.down * gravity;

        if(Grounded && moveInput.y > 0)
        {
            Grounded = false;
            relative_velocity += (Complex)Vector2.up * jumpPower;
        }
        MoveBy();
        if (HCollisionSystem.DoesPointCollideWithWorld(gyroposition.Gyrovector))
            Debug.LogError("Stuck after move!");


        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetVector("_PositionGyrovector", (Vector2)gyroposition.Gyrovector);
        propertyBlock.SetFloat("_PositionGyration", gyroposition.Gyration);
        propertyBlock.SetVector("unity_SpriteProps", new Vector4(1, 1, -1, 0));
        propertyBlock.SetColor("unity_SpriteColor", Color.white);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    void MoveBy()
    {
        int depth = 10;
        HRaycastHit hit;

        float leftover_velocity = Mathf.Sqrt(relative_velocity.sqrLength());
        while (depth-- >= 0)
        {
            MobiusGyroposition newPos;
            Hyperbolic.DebugDrawHLine(gyroposition.Gyrovector, true_velocity.normalized()*0.2f, Color.cyan);

            HCollisionSystem.Raycast(gyroposition.Gyrovector, true_velocity, out hit, leftover_velocity);
            if (!hit.hit)
            {
                newPos = gyroposition.RMovedBy(true_velocity);
                if (!HCollisionSystem.DoesPointCollideWithWorld(newPos.Gyrovector))
                    gyroposition = newPos;
                else
                    Debug.LogWarning("Almost stuck when exiting move due to lack of collision!");

                return;
            }

            Hyperbolic.DebugDrawHLine(gyroposition.Gyrovector, hit.normal, Color.red);
            newPos = gyroposition.RMovedBy(true_velocity.normalized() * hit.length * 1f);
            newPos.RMoveBy(hit.normal * HCollisionSystem.skinWidth);
            leftover_velocity -= hit.length * 1f;

            if (!HCollisionSystem.DoesPointCollideWithWorld(newPos.Gyrovector))
                gyroposition = newPos;
            //else
            //    Debug.LogWarning("Soft Stuck");


            if (HCollisionSystem.DoesPointCollideWithWorld(gyroposition.Gyrovector))
                Debug.LogError("Stuck while move!");

            relative_velocity = Hyperbolic.ApplyAngleGyration(true_velocity.CastOnto(hit.normal * new Complex(0, 1)), -newPos.Gyration);


            //not sure if this works
            //int unstuck = 5;
            //while (HCollisionSystem.DoesPointCollideWithWorld(newPos.Gyrovector) && unstuck-- >= 0)
            //{
            //    if (unstuck == 0)
            //    {
            //        Debug.LogWarning("stuck");
            //        return;
            //    }
            //    newPos.RMoveBy(hit.normal * HCollisionSystem.skinWidth);
            //}

            
        }

        Debug.LogWarning("reached collision depth limit");
        return;
    }

    void CheckGrounded()
    {
        HRaycastHit hit;
        HCollisionSystem.Raycast(gyroposition.Gyrovector, Hyperbolic.ApplyAngleGyration((Complex)Vector2.down, gyroposition.Gyration), out hit, HCollisionSystem.skinWidth*2);

        if (!hit)
        {
            Grounded = false;
            return;
        }

        Complex normal = hit.normal;
        normal = Hyperbolic.ApplyAngleGyration(normal, -gyroposition.Gyration);

        if (normal.Im == 0)
        {
            Grounded = false;
            return;
        }

        Grounded = Mathf.Abs(normal.Re)/normal.Im <= maxSlope;
        return;
    }
}
