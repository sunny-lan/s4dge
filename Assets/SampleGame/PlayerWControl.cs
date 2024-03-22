using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S4DGE {
    // Allows the referenced player to directly affect their t4d w coordinate using the 1 and 2 keyboard keys
    public class PlayerWControl : MonoBehaviour
    {
        public Transform4D playerTransform;
        public float wVelocity = 2.5f; // scales the 'speed' of changes in w

        void Update()
        {
            if (Input.GetKey(KeyCode.Alpha1)) // 1 key above alphabet keys
            {
                playerTransform.position += new Vector4(0,0,0,1) * wVelocity * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.Alpha2)) // 2 key above alphabet keys
            {
                playerTransform.position += new Vector4(0,0,0,-1) * wVelocity * Time.deltaTime;;
            }   
        }
    }
}
