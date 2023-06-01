using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace v2 
{

    [ExecuteAlways]
    [RequireComponent(typeof(Transform4D))]
    [RequireComponent(typeof(Canvas))] //DO NOT MOVE THE 3D CAMERA
    public class RaycastCanvas : MonoBehaviour
    {
        public Canvas screen { get; private set; }
        public Transform4D t4d { get; private set; }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
