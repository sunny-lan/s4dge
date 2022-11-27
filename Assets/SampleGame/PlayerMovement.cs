using UnityEngine;
using v2;

public class PlayerMovement : MonoBehaviour
{
    enum State
    {
        Walking
    }

    public bool useAcceleration = false;
    public float acceleration = 1f, friction = 0.1f;

    public float lookSpeed = 1f;
    public float maxMovementSpd = 1f;

    /// <summary>
    /// In radians
    /// </summary>
    public float minLookDownAngle = -1f, maxLookUpAngle = 1f; 

    Transform4D t4d;
    void Start()
    {
        t4d = GetComponent<Transform4D>();
    }

    State state = State.Walking;

    Vector2 lookRotation; //x=side to side rotation, y=up down rotation
    Vector4 velocity;
    float wSlideStart;

    void Update()
    {
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

        t4d.localEulerAngles3D = new(
            lookRotation.y,
            lookRotation.x,
            0
        );

        // w-slide
        if (Input.GetKeyDown(KeyCode.C))
        {
            wSlideStart = lookRotation.x;
        }

        if (Input.GetKey(KeyCode.C))
        {
            // when the user presses c, they rotate in w as well
            var currentWAngle = lookRotation.x - wSlideStart;
            t4d.localRotation[(int)Rot4D.xw] = currentWAngle;
        }
        else
        {
            // reset xw rotation to 0 again
            t4d.localRotation[(int)Rot4D.xw] = 0;
        }

        // wasd movement
        Vector4 deltaVelocity = Vector4.zero;
        if (Input.GetKey(KeyCode.W))
        {
            deltaVelocity = t4d.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            deltaVelocity = t4d.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            deltaVelocity = t4d.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            deltaVelocity = t4d.right;
        }
        deltaVelocity.y = 0; // no y movement

        if (useAcceleration)
        {
            velocity += deltaVelocity * acceleration * Time.deltaTime;
            velocity -= velocity.normalized * friction * Time.deltaTime;
            velocity = velocity.LimitLength(maxMovementSpd);
        }
        else
        {
            velocity = deltaVelocity * maxMovementSpd;
        }
        t4d.localPosition += velocity * Time.deltaTime;
    }
}
