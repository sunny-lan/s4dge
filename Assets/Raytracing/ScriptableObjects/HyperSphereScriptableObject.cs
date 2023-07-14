using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using v2;

[CreateAssetMenu(fileName = "New HyperSphere", menuName = "Raycasting Objects/HyperSphere")]
public class HyperSphereScriptableObject : ScriptableObject
{
    public RayTracingMaterial material;
    public Transform4DScriptableObject transform4D;
    public float radius;
}
