using System.Collections.Generic;
using UnityEngine;

namespace RayTracing
{

    /// <summary>
    /// A scene-global component that manages and tracks all the 4D gameobjects within a 4D ray tracing scene.
    /// </summary>
    public class Scene4D : MonoBehaviour
    {
        private static Scene4D _instance; // keep the actual instance private
        public static Scene4D Instance
        {
            get
            { // find the script instance in the scene if the private instance is null
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Scene4D>();
                }

                if (_instance != null && _instance.rayTracedShapes == null)
                {
                    _instance.FindShapes();
                }

                return _instance;
            }
        }

        public List<RayTracedShape> rayTracedShapes { get; private set; } // List of shapes in this scene to be rendered by the raycaster

        private void Awake()
        {
            _instance ??= this;

            FindShapes();
        }

        private void FindShapes()
        {
            rayTracedShapes = new List<RayTracedShape>();
            RayTracedShape[] shapes = FindObjectsOfType<RayTracedShape>(); // Fetch all existing objects in scene on awake
            foreach (var shape in shapes)
            {
                Register(shape);
            }
        }

        public void Register(RayTracedShape addedShape) // Add shape to the raycasted scene
        {
            if (!rayTracedShapes.Contains(addedShape))
            {
                rayTracedShapes.Add(addedShape);
            }
        }

        public void Remove(RayTracedShape removedShape) // Remove shape from the raycasted scene
        {
            if (!rayTracedShapes.Remove(removedShape))
            {
                Debug.LogWarning($"Failed to remove shape {removedShape} from the raycast list");
            }
        }
    }
}