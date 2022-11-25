using UnityEngine;
using v2;

public class PlayerMovement : MonoBehaviour
{
    enum State
    {
        Walking
    }

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

        t4d.eulerAngles3D = new(
            lookRotation.y,
            lookRotation.x,
            0
        );



        // Movement: assume z rotation is 0
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

        velocity += deltaVelocity * acceleration * Time.deltaTime;
        velocity -= velocity.normalized * friction * Time.deltaTime;
        velocity = velocity.LimitLength(maxMovementSpd);
        t4d.position += velocity * Time.deltaTime;



        // w-slide
        if (Input.GetKey(KeyCode.C))
        {

        }
    }
}
