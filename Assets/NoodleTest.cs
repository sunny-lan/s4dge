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
                Divisions = 10,
                End = 2*Mathf.PI,
                Start = 0,
                Path = s =>
                {
                    return new(
                        2*Mathf.Sin(s),
                        2*Mathf.Cos(s),
                        0,0
                    );
                }
            };

            var converted = ManifoldConverter.HyperCylinderify(line, s => 1);

            var mesh = MeshGenerator4D.GenerateTetMesh(converted.Position, converted.Normal, converted.Bounds);

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
