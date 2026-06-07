using UnityEngine;
public class CameraBrain : MonoBehaviour
{
    [SerializeField]
    float linearEase = 0.2f;
    [SerializeField]
    float angularEase = 0.2f;

    [SerializeField]
    bool follow = true;

    public static MobiusGyroposition gyroposition = new MobiusGyroposition(0,0);

    private void Awake()
    {
        Shader.SetGlobalVector("_CameraGyrovector", Vector2.zero);
        Shader.SetGlobalFloat("_CameraGyration", 0);

        TestingController.Restart_e += Restart;
    }

    private void FixedUpdate()
    {
        if (follow)
        {
            FollowPlayer();
        }
    }

    void FollowPlayer()
    {
        //we do the lerp on the inverse or our gyroposition

        //eased position = a+(-a+b)*t

        MobiusGyroposition gInv = gyroposition.Inverse();

        gInv.Gyrovector = Hyperbolic.VectorAddition(gInv.Gyrovector, Hyperbolic.ScalarMult(linearEase, Hyperbolic.VectorAddition(-gInv.Gyrovector, PlayerController.gyroposition.Gyrovector)));
        gInv.Gyration = Mathf.Deg2Rad * Mathf.LerpAngle(Mathf.Rad2Deg * gInv.Gyration, Mathf.Rad2Deg * PlayerController.gyroposition.Gyration, angularEase);

        gyroposition = gInv.Inverse();

        Shader.SetGlobalVector("_CameraGyrovector", (Vector2)gyroposition.Gyrovector);
        Shader.SetGlobalFloat("_CameraGyration", gyroposition.Gyration);
    }

    private void Restart()
    {
        gyroposition = new();
    }
}
