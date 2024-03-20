using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using v2;

public class SymposiumControls : MonoBehaviour
{
    public Text lookMode;
    public Text debugCam;
    public float lookSpeed = 0.05f;

    /// <summary>
    /// In radians
    /// </summary>
    public float minLookDownAngle = -1f, maxLookUpAngle = 1f;

    Transform4D t4d;

    void Start()
    {
        t4d = GetComponent<Transform4D>();
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        ResetLookRotation();
    }

    Vector2 lookRotation; //x=side to side rotation, y=up down rotation
    Vector4 velocity; //local direction vector
    public float maxStrafeSpeed = 1f;
    private bool rotateIn4D = false;

    void ResetLookRotation()
    {
        // Update look rotation back to the corresponding rotation
        if (!rotateIn4D) {
            // camera rotates left/right, up/down
            lookRotation.x = t4d.localRotation[(int)Rot4D.xw];
            lookRotation.y = t4d.localRotation[(int)Rot4D.yw];
            lookMode.text = "Rotation Planes = 3D (xy, xw)";
        } else {
            // camera rotates left/right, up/down
            lookRotation.x = t4d.localRotation[(int)Rot4D.xz];
            lookRotation.y = t4d.localRotation[(int)Rot4D.yz];
            lookMode.text = "Rotation Planes = 4D (xz, yz)";
        }
    }

    void Update()
    {
        if (Camera4D.main != null && debugCam != null)
        {
            debugCam.text = "World to Camera\n" + Camera4D.main.WorldToCameraTransform.ToString() + "\n\nCamera to World\n" + Camera4D.main.CameraToWorldTransform.ToString();
            // Debug.Log(Camera4D.main.CameraToWorldTransform.ToString());
        }
        // Camera look
        Vector2 deltaMouse = new(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );

        Vector2 deltaLook = -deltaMouse * lookSpeed;
        lookRotation += deltaLook;
        
        if (Input.GetKeyUp(KeyCode.Space))
        {
            rotateIn4D = !rotateIn4D;

            ResetLookRotation();
        }

        // limit rotation to valid stuff
        lookRotation = new(
            lookRotation.x % (2 * Mathf.PI),
            Mathf.Clamp(lookRotation.y, minLookDownAngle, maxLookUpAngle)
        );

        if (!rotateIn4D) {
            // camera rotates left/right, up/down
            t4d.localRotation[(int)Rot4D.xw] = lookRotation.x;
            t4d.localRotation[(int)Rot4D.yw] = lookRotation.y;
        } else {
            // camera rotates left/right, up/down
            t4d.localRotation[(int)Rot4D.xz] = lookRotation.x;
            t4d.localRotation[(int)Rot4D.yz] = lookRotation.y;
        }

        // wasd movement
        Vector4 wasdDirection = Vector4.zero;
        if (Input.GetKey(KeyCode.W))
        {
            wasdDirection += new Vector4(0, 0, 0, 1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            wasdDirection += new Vector4(0, 0, 0, -1);
        }
        if (Input.GetKey(KeyCode.A))
        {
            wasdDirection += new Vector4(-1, 0, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            wasdDirection += new Vector4(1, 0, 0, 0);
        }

        if (wasdDirection.sqrMagnitude > 0)
        {
            // prevent from exceeding limit (do not apply new velocity if exceeds limits)
            Vector4 accelDirection = t4d.LocalDirectionToWorld(wasdDirection);
            var nextVelocity = velocity + accelDirection * Time.deltaTime;
            velocity = nextVelocity;
        }
        else
        {
            velocity = Vector4.zero;
        }

        t4d.position += t4d.LocalDirectionToWorld(velocity) * Time.deltaTime;

        // Up/down and forwards/backward not dependent on rotation positioned
        if (Input.GetKey(KeyCode.Q))
        {
            t4d.localPosition += new Vector4(0, 1, 0, 0) * maxStrafeSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E))
        {
            t4d.localPosition += new Vector4(0, -1, 0, 0) * maxStrafeSpeed * Time.deltaTime;
        }
        
        if (Input.GetKey(KeyCode.LeftShift))
        {
            t4d.localPosition += new Vector4(0, 0, 1, 0) * maxStrafeSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            t4d.localPosition += new Vector4(0, 0, -1, 0) * maxStrafeSpeed * Time.deltaTime;
        }
    }
}
