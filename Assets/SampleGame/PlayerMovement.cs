using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using v2;

public class PlayerMovement : MonoBehaviour
{
    enum State
    {
        Walking
    }

    public bool useRotation = false;
    public bool useAcceleration = false;
    public float acceleration = 1f, friction = 0.1f;

    public float lookSpeed = 0.05f;
    public float maxMovementSpd = 1f;

    /// <summary>
    /// In radians
    /// </summary>
    public float minLookDownAngle = -1f, maxLookUpAngle = 1f;

    Camera4D camera;
    Transform4D t4d;
    void Start()
    {
        t4d = GetComponent<Transform4D>();

        camera = GetComponentInChildren<Camera4D>();

        sliceRenderer = GetComponent<SliceRenderer>();

        grappleLine = ScriptableObject.CreateInstance<InterpolationBasedShape>();
        sliceRenderer.Shape = null;
    }

    // listen to all collision events in children and self
    private void OnEnable()
    {
        foreach(BoxCollider4D boxCollider in GetComponentsInChildren<BoxCollider4D>())
        {
            boxCollider.OnCollisionStay += OnCollisionDetected;
        }
    }

    private void OnCollisionDetected(BoxCollider4D obj)
    {
        velocity = Vector4.zero;

        //find collision normal
        var localPos = obj.t4d.WorldToLocal(t4d.position);
        Vector4 normal = Vector4.zero;
        float maxDist = -1;
        var boxCenter = obj.size / 2 + obj.corner;
        var collisionVector = localPos - boxCenter;
        for(int axis=0;axis<4;axis++)
        {
            if (collisionVector[axis] > maxDist)
            {
                maxDist = collisionVector[axis];
                normal = Vector4.zero;
                normal[axis] = maxDist;
            }   
        }

        normal = obj.t4d.LocalDirectionToWorld(normal); //translate normal to world direction
        t4d.position += normal.normalized*Time.deltaTime; // push back out
    }

    // unsubscribe all events when disabled
    private void OnDisable()
    {
        foreach (BoxCollider4D boxCollider in GetComponentsInChildren<BoxCollider4D>())
        {
            boxCollider.OnCollisionStay -= OnCollisionDetected;
        }
    }

    State state = State.Walking;

    Vector2 lookRotation; //x=side to side rotation, y=up down rotation
    Vector4 velocity;
    float wSlideStart;

    Vector4? hookPoint = null;
    public float grappleVelocity = 5;
    SliceRenderer sliceRenderer;

    public InterpolationBasedShape grappleLine;

    // shoot a grapple from the player position towards where they are looking
    void Grapple(Vector4 position, Vector4 look)
    {
        var collidePoints = CollisionSystem.Instance.Raycast(new Ray4D { src = position, direction = look }, Physics.AllLayers);
        var collidePoint = collidePoints.Min();
        
        if (collidePoint is Ray4D.Intersection cp)
        {
            hookPoint = cp.point;
            Debug.Log(hookPoint);
        }   
    }

    // returns a unit vector that is orthogonal to vec
    Vector4 GetOrthogonalVector(Vector4 vec)
    {
        if (vec.y == 0)
        {
            return new Vector4(0, 1, 0, 0);
        }
        return new Vector4(1, vec.x / vec.y, 0, 0).normalized;
    }

    InterpolationPoint4D V4At(Vector4 vec)
    {
        return new(new List<PointInfo> {
                    new PointInfo { position4D = vec.XYZ().withW(int.MinValue) },
                    new PointInfo { position4D = vec.XYZ().withW(int.MaxValue) }
                });
    }

    void DrawGrapple(Vector4 playerPos, Vector4 hp)
    {
        //grappleLine = S4DGELoader.LoadS4DGE("Assets/Models/hypercube.s4dge");
        //sliceRenderer.Shape = grappleLine;
        //return;

        // draw grapple line as face
        Vector4 localPlayerPos = new Vector4(0.5f, 1f, 0.5f, 0f);
        Vector4 localHp = camera.t4d.WorldToLocal(hp);
        float offset = 0.1f;
        InterpolationPoint4D start1 = V4At(localPlayerPos);
        InterpolationPoint4D start2 = V4At(localPlayerPos + new Vector4(offset, 0, 0, 0));
        InterpolationPoint4D start3 = V4At(localPlayerPos + new Vector4(0, offset, 0, 0));
        InterpolationPoint4D start4 = V4At(localPlayerPos + new Vector4(offset, offset, 0, 0));
        InterpolationPoint4D end1 = V4At(localHp);
        InterpolationPoint4D end2 = V4At(localHp + new Vector4(offset, 0, 0, 0));
        InterpolationPoint4D end3 = V4At(localHp + new Vector4(0, offset, 0, 0));
        InterpolationPoint4D end4 = V4At(localHp + new Vector4(offset, offset, 0, 0));

        grappleLine.points = new Dictionary<string, InterpolationPoint4D>
            {
                {"start1", start1 },
                {"start2", start2 },
                {"start3", start3 },
                {"start4", start4 },
                {"end1", end1},
                {"end2", end2},
                {"end3", end3},
                {"end4", end4},
            };

        grappleLine.lines4D.Clear();
        grappleLine.faces4D = new List<Face<InterpolationPoint4D>> {
            new Face<InterpolationPoint4D>(new List<InterpolationPoint4D> { start1, start2, start4, start3 }),
            new Face<InterpolationPoint4D>(new List<InterpolationPoint4D> { end1, end2, end4, end3 }),
            new Face<InterpolationPoint4D>(new List<InterpolationPoint4D> { start1, start2, end2, end1 }),
            new Face<InterpolationPoint4D>(new List<InterpolationPoint4D> { start1, start3, end3, end1 }),
            new Face<InterpolationPoint4D>(new List<InterpolationPoint4D> { start2, start4, end4, end2 }),
            new Face<InterpolationPoint4D>(new List<InterpolationPoint4D> { start3, start4, end4, end3 }),
        };

        sliceRenderer.Shape = grappleLine;
    }

    void Update()
    {
        // Camera look
        Vector2 deltaMouse = new(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );

        Vector2 deltaLook = -deltaMouse * lookSpeed;
        lookRotation += deltaLook;

        // limit rotation to valid stuff
        lookRotation = new(
            lookRotation.x%(2*Mathf.PI),
            Mathf.Clamp( lookRotation.y, minLookDownAngle, maxLookUpAngle)
        );

        if (!useRotation)
        {
            t4d.localEulerAngles3D = new(
                lookRotation.y,
                lookRotation.x,
                0
            );
        }

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

        // grapple
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Grapple(camera.t4d.position, camera.t4d.forward);
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

        // grapple velocity
        if (hookPoint is Vector4 hp)
        {
            Vector4 playerPos = camera.t4d.position;
            Vector4 delta = (hp - playerPos);
            Vector4 dir = delta.normalized;

            // if angle between look, hook is >90 degrees or we reach the dest point, stop grappling
            if (Util.Angle(camera.t4d.forward, dir) > Mathf.PI / 2 || Vector4.Magnitude(delta) <= 1e-1)
            {
                hookPoint = null;
                sliceRenderer.Shape = null;
            }
            else
            {
                DrawGrapple(playerPos, hp);
                // grapple affects player velocity
                deltaVelocity += (dir * grappleVelocity) * Time.deltaTime;
            }
        }

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
