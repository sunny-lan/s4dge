using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace v2
{
    [ExecuteAlways]
    [RequireComponent(typeof(Transform4D))]
    public class MeshRenderer : MonoBehaviour, IShape4DRenderer
    {
        Transform4D t4d;
        Geometry3D slice = new();
        Mesh[] meshes;

        public InterpolationBasedShape Shape { get=>shape; set=>shape=value; }
        [DoNotSerialize] //TODO for now
        InterpolationBasedShape shape;

        public string fileName;
        public Material material;
        //
        // BEGIN editor variables/methods
        //

        // TODO: decide acceptable w range here
        [Range(-10.0f, 10.0f)]
        public float previewW = 0;

        // 
        // END editor variables/methods
        //

        void Start()
        {
            t4d = gameObject.GetComponent<Transform4D>();
            meshes = Array.ConvertAll<object, Mesh>(Resources.LoadAll(fileName, typeof(Mesh)), obj => (Mesh)obj); // find all meshes in children by their mesh filters
            ConvertMeshesTo4D( meshes, t4d );
        }

        public static void ConvertMeshesTo4D( Mesh[] meshArray, Transform4D transform )
        {
            foreach ( Mesh m in meshArray )
            {
                List<Vector2> uvs = new List<Vector2>(); // initialize list for uv1 w coordinates
                for( int i = 0; i < m.vertices.Length; i++ )
                {
                    uvs.Add(new Vector2( transform.position.w ,0)); // set all w coordinates to the transform w
                }
                m.SetUVs(1, uvs); // add the w coordinate uvs to the mesh uv1
            }
        }

        //
        // Here is the rendering code
        // We add a callback for every camera before it begins rendering
        //  - Upon any camera render, we submit and draw all submeshes
        //

        private void OnEnable()
        {
            Camera4D.onBeginCameraRendering += SimpleRender;
        }

        private void OnDisable()
        {
            Camera4D.onBeginCameraRendering -= SimpleRender;
        }

        static int cameraPosShaderID = Shader.PropertyToID("_4D_Camera_Pos");
        private void SimpleRender( ScriptableRenderContext ctx, Camera4D cam )
        {
            if (meshes == null) return;

            MaterialPropertyBlock blk = new();
            blk.SetVector(cameraPosShaderID, cam.t4d.position); //pass in camera position to shader
            foreach (Mesh m in meshes )
            {
                for ( int i = 0; i < m.subMeshCount; ++i )
                {
                    Graphics.DrawMesh(
                        mesh: m,
                        position: t4d.position,  // implicitly discard w dimension (which is now baked into uvs channel 1)
                        // set the 3d rotation to be the simple rotations around x,y,z ( yz plane, xz plane, xy plane)
                        rotation: Quaternion.Euler( t4d.rotation[(int)Rot4D.yz] * Mathf.Rad2Deg, t4d.rotation[(int)Rot4D.xz] * Mathf.Rad2Deg, t4d.rotation[(int)Rot4D.xy] * Mathf.Rad2Deg),
                        material: material, 
                        layer: gameObject.layer, 
                        camera: cam.camera3D,
                        submeshIndex: i,
                        properties: blk
                    );
                }
               
            }
            
        }
    }

}