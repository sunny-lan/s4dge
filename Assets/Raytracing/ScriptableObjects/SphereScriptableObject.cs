using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Sphere", menuName = "Raycasting Objects/Sphere")]
public class SphereScriptableObject : ScriptableObject
{
    public RayTracingMaterial material;
    public Transform4DScriptableObject transform4D;

    public float radius;


}
