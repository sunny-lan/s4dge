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
        Material[] materials;

        public InterpolationBasedShape Shape { get=>shape; set=>shape=value; }
        [DoNotSerialize] //TODO for now
        InterpolationBasedShape shape;
        //
        // BEGIN editor variables/methods
        //
        public string fileName;
        public Material fallBackMaterial;
        // TODO: decide acceptable w range here
        [Range(-10.0f, 10.0f)]
        public float previewW = 0;
        public bool showWhenOutOfRange = true;

        // 
        // END editor variables/methods
        //

        void Start()
        {
            t4d = gameObject.GetComponent<Transform4D>();
            meshes = Resources.LoadAll(fileName, typeof(Mesh)).Cast<Mesh>().ToArray(); // find all meshes in children by their mesh filters
            Log.Print($"Loaded {meshes.Length} meshes", Log.meshRendering);
            materials = Resources.LoadAll(fileName, typeof(Material)).Cast<Material>().ToArray();; // find all the materials in children by their renderers
            foreach (Material mat in materials)
            {
                Log.Print( $"Loaded material: {mat.name}", Log.meshRendering );
            }
            AddUV1ToMeshes( meshes, t4d.position.w );
        }

        // adds a list of uvs to each mesh's uv1 for the given w value
        public static void AddUV1ToMeshes( Mesh[] meshArray, float w )
        {
            foreach ( Mesh m in meshArray )
            {
                List<Vector2> uvs = new List<Vector2>(); // initialize list for uv1 w coordinates
                for( int i = 0; i < m.vertices.Length; i++ )
                {
                    uvs.Add(new Vector2( w ,0)); // set all w coordinates to the transform w
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
            // do not display the mesh if it is more than previewW away from the camera w and showWhenOutOfRange is deselected
            if ( !showWhenOutOfRange && Mathf.Abs( t4d.position.w - cam.t4d.position.w ) > previewW ) return;

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
                        material: i < materials.Length ? materials[i] : fallBackMaterial, 
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