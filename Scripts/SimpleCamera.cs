using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleCamera : MonoBehaviour
{
    [SerializeField]
    float moveSpeed = 0.6f;

    [SerializeField]
    float drag = 0.95f;

    [SerializeField]
    Complex relative_velocity;

    Complex true_velocity { get { return Hyperbolic.ApplyAngleGyration(relative_velocity, gyroposition.Gyration); } }

    public static MobiusGyroposition gyroposition = new MobiusGyroposition(0, 0);

    InputAction moveAction;
    void Awake()
    {
        Shader.SetGlobalVector("_CameraGyrovector", Vector2.zero);
        Shader.SetGlobalFloat("_CameraGyration", 0);

        moveAction = InputSystem.actions.FindAction("Move");

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
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        relative_velocity *= drag;

        relative_velocity += (Complex)moveInput * moveSpeed;
        MoveBy();


        Shader.SetGlobalVector("_CameraGyrovector", (Vector2)gyroposition.Inverse().Gyrovector);
        Shader.SetGlobalFloat("_CameraGyration", gyroposition.Inverse().Gyration);
    }

    void MoveBy()
    {
        gyroposition.RMoveBy(true_velocity);
    }
}
