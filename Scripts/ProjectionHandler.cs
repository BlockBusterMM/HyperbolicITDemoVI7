using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ProjectionHandler : MonoBehaviour
{
    [SerializeField]
    Camera cam;

    static event Action<ProjectionType> projectionChangeEvent;

    void Awake()
    {
        ChangeProjection(ProjectionType.mobius);
        projectionChangeEvent += ChangeProjection;
    }

    public enum ProjectionType
    {
        mobius = 0,
        einstein = 1,
        ungar = 2,
    }

    void ChangeProjection(ProjectionType projection)
    {
        switch(projection)
        {
            case ProjectionType.mobius:

                Shader.SetGlobalFloat("_EinsteinPostModel", 0);
                Shader.SetGlobalFloat("_UngarPostModel", 0);
                cam.orthographicSize = 1;

                break;
            case ProjectionType.einstein:

                Shader.SetGlobalFloat("_EinsteinPostModel", 1);
                Shader.SetGlobalFloat("_UngarPostModel", 0);
                cam.orthographicSize = 1;

                break;
            case ProjectionType.ungar:

                Shader.SetGlobalFloat("_EinsteinPostModel", 0);
                Shader.SetGlobalFloat("_UngarPostModel", 1);
                cam.orthographicSize = 5;

                break;
        }
    }

    public static void OnProjectionChange(int a)
    {
        projectionChangeEvent?.Invoke((ProjectionType)a);
    }
}
