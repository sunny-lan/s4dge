using RasterizationRenderer;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NoodleTest : MonoBehaviour
{
}

[CustomEditor(typeof(NoodleTest))]
public class NoodleTestEditor : Editor
{

    public override void OnInspectorGUI()
    {
        var renderer = (target as Component).GetComponent<TetMeshRenderer>();
        if (GUILayout.Button("Gen noodle"))
        {
            var line = new ParametricShape1D()
            {
                Divisions = 8,
                End = 1,
                Start = 0,
                Path = s =>
                {
                    Vector4 st = new(0, 0, 0, 0), ed = new(4, 0, 0, 0);
                    return s * st + (1 - s) * ed;
                }
            };

            var converted = ManifoldConverter.HyperCylinderify(line, s => 1, s => new Frame4D()
            {
                T = new(1, 0, 0, 0),
                B = new(0, 1, 0, 0),
                N = new(0, 0, 1, 0),
                D = new(0, 0, 0, 1),
            });

            var mesh = MeshGenerator4D.GenerateTetMesh(converted.Equation, _ => new(), converted.Bounds);

            renderer.mesh = ScriptableObject.CreateInstance<TetMesh_UnityObj>();
            renderer.mesh.mesh_Raw = new(mesh);
        }
        if (GUILayout.Button("Gen torus"))
        {
            renderer.mesh = ScriptableObject.CreateInstance<TetMesh_UnityObj>();
            var mesh = MeshGenerator4D.Generate3TorusMesh(1f, 1);
            renderer.mesh.mesh_Raw = new(mesh);
        }
    }
}
