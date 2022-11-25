using UnityEngine;
using v2;

public class PlayerMovement : MonoBehaviour
{
    public float lookSpeed = 1f;
    public float moveSpeed = 1f;

    /// <summary>
    /// In radians
    /// </summary>
    public float minLookDownAngle = -1f, maxLookUpAngle = 1f; 

    Transform4D t4d;
    void Start()
    {
        t4d = GetComponent<Transform4D>();
    }

    Vector2 lookRotation; //x=side to side rotation, y=up down rotation

    void Update()
    {
        // Implement movement: assume z rotation is 0
        Vector4 deltaPosition = Vector4.zero;
        if (Input.GetKey(KeyCode.W))
        {
            deltaPosition = t4d.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            deltaPosition = t4d.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            deltaPosition = t4d.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            deltaPosition = t4d.right;
        }
        t4d.position += deltaPosition * moveSpeed * Time.deltaTime;

        // Camera look
        Vector2 deltaMouse = new(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );

        Vector2 deltaLook = -deltaMouse * lookSpeed * Time.deltaTime;
        lookRotation += deltaLook;

        // limit rotation to valid stuff
        lookRotation = new(
            lookRotation.x%(2*Mathf.PI),
            Mathf.Clamp( lookRotation.y, minLookDownAngle, maxLookUpAngle)
        );

        t4d.eulerAngles3D = new(
            lookRotation.y,
            lookRotation.x,
            0
        );
    }
}
