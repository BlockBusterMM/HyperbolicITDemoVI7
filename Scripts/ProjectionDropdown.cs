using System;
using UnityEngine;

public class ProjectionDropdown : MonoBehaviour
{
    public void OnValueChange(Int32 value)
    {
        ProjectionHandler.OnProjectionChange(value);
    }
}
