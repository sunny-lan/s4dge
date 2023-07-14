using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using v2;

[CreateAssetMenu(fileName = "New Sphere", menuName = "Raycasting Objects/Sphere")]
public class RayTraced : ScriptableObject
{
    public RayTracingMaterial material;
    public Transform4DScriptableObject transform4D;
    public float radius;
}
