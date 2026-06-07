using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PresentationController : MonoBehaviour
{
    [SerializeField]
    int initialScene = 0;
    [SerializeField]
    List<string> Scenes;

    int currentSceneIndex = -1;
    void Awake()
    {
        InputSystem.actions.FindAction("Previous").performed += Previous;
        InputSystem.actions.FindAction("Next").performed += Next;
        SwitchToScene(initialScene);
    }

    void Previous(InputAction.CallbackContext a)
    {
        SwitchToScene((Scenes.Count + currentSceneIndex - 1) % Scenes.Count);
    }
    void Next(InputAction.CallbackContext a)
    {
        SwitchToScene((Scenes.Count + currentSceneIndex + 1) % Scenes.Count);
    }

    void SwitchToScene(int sceneIndex)
    {
        if(currentSceneIndex>=0)
            SceneManager.UnloadSceneAsync(Scenes[currentSceneIndex]);
        SceneManager.LoadSceneAsync(Scenes[sceneIndex], LoadSceneMode.Additive);
        currentSceneIndex = sceneIndex;
        TestingController.Restart();
    }
}
