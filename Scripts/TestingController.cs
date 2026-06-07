using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestingController : MonoBehaviour
{
    public static event Action Restart_e;

    public static void Restart()
    {
        Restart_e?.Invoke();
    }

    public static void Restart(InputAction.CallbackContext a)
    {
        Restart();
    }

    void Awake()
    {
        InputSystem.actions.FindAction("Restart").performed += TestingController.Restart;
    }
}
