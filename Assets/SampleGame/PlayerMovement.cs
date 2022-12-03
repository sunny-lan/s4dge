using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using v2;

public class PlayerMovement : MonoBehaviour
{
    public bool useRotation = false;
    public float acceleration = 1f;
    public Vector4 friction = new Vector4(0.1f, 0.1f, 0.1f, 0.6f);
    public float gravity = 9.81f;
    public float jumpSpeed = 20f;

    public float lookSpeed = 0.05f;

    /// <summary>
    /// In radians
    /// </summary>
    public float minLookDownAngle = -1f, maxLookUpAngle = 1f;

    Camera4D cam4D;
    Transform4D t4d;
    void Start()
    {
        groundLayerMask = LayerMask.GetMask("Ground");

        t4d = GetComponent<Transform4D>();

        cam4D = GetComponentInChildren<Camera4D>();

        sliceRenderer = GetComponent<SliceRenderer>();

        grappleLine = ScriptableObject.CreateInstance<InterpolationBasedShape>();
        sliceRenderer.Shape = null;
    }

    // listen to all collision events in children and self
    private void OnEnable()
    {
        foreach (BoxCollider4D boxCollider in GetComponentsInChildren<BoxCollider4D>())
        {
            //boxCollider.OnCollisionStay += OnCollisionDetected;
        }
    }

    private void OnCollisionDetected(Collider4D obj)
    {
        //velocity = Vector4.zero;

        ////find collision normal
        //var localPos = obj.t4d.WorldToLocal(t4d.position);
        //Vector4 normal = Vector4.zero;
        //var boxCenter = obj.size / 2 + obj.corner;
        //var collisionVector = localPos - boxCenter;
        //for(int axis=0;axis<4;axis++)
        //{
        //    //TODO sus
        //    if (Math.Abs(collisionVector[axis])*1.1f > obj.size[axis] /2)
        //    {
        //        normal[axis] = collisionVector[axis];
        //    }   
        //}


        //normal = obj.t4d.LocalDirectionToWorld(normal); //translate normal to world direction
        //Debug.Log(normal);
        //t4d.position += normal.normalized*Time.deltaTime; // push back out
    }

    // unsubscribe all events when disabled
    private void OnDisable()
    {
        foreach (BoxCollider4D boxCollider in GetComponentsInChildren<BoxCollider4D>())
        {
            boxCollider.OnCollisionStay -= OnCollisionDetected;
        }
    }

    Vector2 lookRotation; //x=side to side rotation, y=up down rotation
    Vector4 velocity;//local direction vector
    float wSlideStart;

    Vector4? hookPoint = null;
    public float grapplingVelocity = 10;
    SliceRenderer sliceRenderer;

    InterpolationBasedShape grappleLine;
    public float grappleMinW = 0, grappleMaxW = 0;
    public float grappleDltW = 0.5f;

    public bool useGravity = true;


    // shoot multiple grapples (from different w) from the player position towards where they are looking
    void Grapple(Vector4 position, Vector4 look)
    {
        look = look.normalized;
        List<Ray4D.Intersection?> collidePoints = new();

        Ray4D.Intersection? collidePoint = null;
        float bestDistance = float.PositiveInfinity;

        for (float scanW = grappleMinW; scanW <= grappleMaxW; scanW += grappleDltW)
        {

            IEnumerable<Ray4D.Intersection> collisions = CollisionSystem.Instance.Raycast(new Ray4D
            {
                src = position.XYZ().withW(scanW + position.w),
                direction = look
            }, Physics.AllLayers);

            // find raycasted point closest to source
            if (collisions.Count() > 0)
            {
                Ray4D.Intersection minForThis = collisions.Min();
                float curDist = (minForThis.point - position).magnitude;
                if (curDist < bestDistance)
                {
                    bestDistance = curDist;
                    collidePoint = minForThis;
                }
            }
        }

        if (collidePoint is Ray4D.Intersection collidePointNotNull)
        {
            hookPoint = collidePointNotNull.point;
        }
    }

    InterpolationPoint4D V4At(string name, Vector4 vec)
    {
        return new(name, new List<PointInfo> {
                    new PointInfo { position4D = vec.XYZ().withW(int.MinValue) },
                    new PointInfo { position4D = vec.XYZ().withW(int.MaxValue) }
                });
    }

    void DrawGrapple(Vector4 localGrappleStart, Vector4 hp)
    {
        //grappleLine = S4DGELoader.LoadS4DGE("Assets/Models/hypercube.s4dge");
        //sliceRenderer.Shape = grappleLine;
        //return;

        // draw grapple line as face
        Vector4 localHp = t4d.WorldToLocal(hp);
        float offset = 0.1f;
        InterpolationPoint4D start1 = V4At("start1", localGrappleStart);
        InterpolationPoint4D start2 = V4At("start2", localGrappleStart + new Vector4(offset, 0, 0, 0));
        InterpolationPoint4D start3 = V4At("start3", localGrappleStart + new Vector4(0, offset, 0, 0));
        InterpolationPoint4D start4 = V4At("start4", localGrappleStart + new Vector4(offset, offset, 0, 0));
        InterpolationPoint4D end1 = V4At("end1", localHp);
        InterpolationPoint4D end2 = V4At("end2", localHp + new Vector4(offset, 0, 0, 0));
        InterpolationPoint4D end3 = V4At("end3", localHp + new Vector4(0, offset, 0, 0));
        InterpolationPoint4D end4 = V4At("end4", localHp + new Vector4(offset, offset, 0, 0));

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

    int groundLayerMask;

    public float maxStrafeSpeed = 1f;
    public float maxSprintSpeed = 1f;

    public float airAccelModifier = 0.5f; // less movement ability in air

    // different fov depending on speed
    public float normalFov = 60;
    public float sprintFov = 80;
    public float fovSmoothing = 0.1f;

    public float maxCameraWRot = 0.5f; // max amount camera will turn in w
    public float wSlideCameraFactor = 0.3f; // amount player rotates in w per radian of normal rotation during w-slide
    public float wSlideFactor = 1f; // amount player moves in w per radian of normal rotation

    public float standingHeight = 1f, crouchingHeight = 0.5f;
    public float cameraSmooth = 5f; // speed to move camera by between standing and crouching positions

    public float grappleAccelFactor = 0.2f;
    public float slideFrictionModifier = 0.5f; // less friction when sliding
    public float slideSpeedBoost = 1.5f; // initial speed boost when sliding

    public Vector4 airFriction = new Vector4(0.1f, 0.1f, 0.1f, 0.6f);

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
            lookRotation.x % (2 * Mathf.PI),
            Mathf.Clamp(lookRotation.y, minLookDownAngle, maxLookUpAngle)
        );

        if (useRotation) //bool flag to disable look rotation for debugging purposes
        {
            // player rotates left/right
            t4d.localEulerAngles3D = new(
                0,
                lookRotation.x,
                0
            );

            // camera rotates up/down
            cam4D.t4d.localEulerAngles3D = new(
                lookRotation.y,
                0,
                0
            );
        }

        // w-slide
        if (Input.GetKeyDown(KeyCode.C))
        {
            wSlideStart = lookRotation.x;
        }

        bool isCrouchPressed = Input.GetKey(KeyCode.C);
        if (isCrouchPressed)
        {
            //TODO
        }
        else
        {
            // reset xw rotation to 0 again
            t4d.localRotation[(int)Rot4D.xw] = 0;
        }

        //check on ground
        var groundPosition = cam4D.t4d.position;
        groundPosition.y -=0.5f;

        //* Add collisions for other directions
        var fowardDirection = cam4D.t4d.forward;
        fowardDirection.y = 0;
        Ray4D forwardCast = new(){
            direction = fowardDirection,
            src = groundPosition.XYZ().withW(t4d.position.w),
        };
        var forwardCollisions = CollisionSystem.Instance.Raycast(forwardCast, Physics.AllLayers)
            .Where(x => x.delta < 0.7f);
        
        var backDirection = cam4D.t4d.back;
        backDirection.y = 0; 
        Ray4D backwardCast = new(){
            direction = backDirection,
            src = groundPosition.XYZ().withW(t4d.position.w),
        };
        var backwardCollisions = CollisionSystem.Instance.Raycast(backwardCast, Physics.AllLayers)
            .Where(x => x.delta < 0.7f);
        var rightDirection = cam4D.t4d.right;
        rightDirection.y = 0;
        Ray4D rightCast = new(){
            direction = rightDirection,
            src = groundPosition.XYZ().withW(t4d.position.w),
        };
        var rightCollisions = CollisionSystem.Instance.Raycast(rightCast, Physics.AllLayers)
            .Where(x => x.delta < 0.7f);
        var leftDirection = cam4D.t4d.left;
        leftDirection.y = 0;
        Ray4D leftCast = new(){
            direction = leftDirection,
            src = groundPosition.XYZ().withW(t4d.position.w),
        };
        var leftCollisions = CollisionSystem.Instance.Raycast(leftCast, Physics.AllLayers)
            .Where(x => x.delta < 0.7f);
        Ray4D upCast = new(){
            direction = Vector3.up,
            src = cam4D.t4d.position,
        };
        var upCollisions = CollisionSystem.Instance.Raycast(upCast, Physics.AllLayers)
            .Where(x => x.delta < 0.3f);

        Ray4D down = new()
        {
            direction = Vector3.down,
            src = t4d.position,
        };

        var downCollisions = CollisionSystem.Instance.Raycast(down, Physics.AllLayers)
            .Where(x => x.delta < 0.5f);
        
        bool grounded = useGravity switch
        {
            true => downCollisions.Any(),
            false => true // if no gravity, always grounded
        };

        float cameraHeight = (isCrouchPressed && grounded) switch
        {
            true => crouchingHeight,
            false => standingHeight,
        };
        cam4D.t4d.localPosition.y = Mathf.Lerp(cam4D.t4d.localPosition.y, cameraHeight, cameraSmooth * Time.deltaTime);

        // wasd movement
        Vector4 wasdDirection = Vector4.zero;
        if (Input.GetKey(KeyCode.W))
        {
            wasdDirection += Vector3.forward.withW(0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            wasdDirection += Vector3.back.withW(0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            wasdDirection += Vector3.left.withW(0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            wasdDirection += Vector3.right.withW(0);
        }

        bool isSprintRequested = grounded && Input.GetKey(KeyCode.LeftShift) && wasdDirection.z > 0;
        bool isActuallySprinting = grounded && velocity.magnitude > maxStrafeSpeed; //sprint continues after key released sometimes

        bool isSliding = isCrouchPressed && isActuallySprinting;

        if (isSliding)
        {
            // when the user presses c, they rotate in w as well
            var currentWAngle = lookRotation.x - wSlideStart;
            t4d.localRotation[(int)Rot4D.xw] = Mathf.Clamp(
                currentWAngle * wSlideCameraFactor, -maxCameraWRot, maxCameraWRot);
            t4d.position += t4d.LocalDirectionToWorld(
                Vector3.zero.withW(velocity.magnitude * deltaLook.x * wSlideFactor));
        }

        //accel modifier based on whether in air, since strafe slower in air
        float accel_modifier = grounded ? 1.0f : airAccelModifier;

        bool isAccelerating = false; // true if walking or running

        if (wasdDirection.sqrMagnitude > 0)
        {
            // limit z (forward/back) velocity based on whether or not sprinting
            float maxSpeed = (isSprintRequested) switch
            {
                true => maxSprintSpeed,
                false => maxStrafeSpeed
            };

            // prevent from exceeding limit (do not apply new velocity if exceeds limits)
            Vector4 accelDirection = t4d.LocalDirectionToWorld(wasdDirection);
            var nextVelocity = velocity + accel_modifier * acceleration * accelDirection * Time.deltaTime;

            if (
                nextVelocity.magnitude < velocity.magnitude || //allow deacceleration
                (
                    nextVelocity.magnitude < accel_modifier * maxSpeed &&
                    Mathf.Abs(t4d.WorldDirectionToLocal(nextVelocity).x) < accel_modifier * maxStrafeSpeed
                )
            )
            {
                velocity = nextVelocity;
                isAccelerating = true;
            }
        }

        Vector4 actualFriction = Vector4.zero;
        if (grounded)
        {
            if (!isAccelerating) //don't add friction when running
            {
                actualFriction = friction;
                if (isSliding)
                    actualFriction *= slideFrictionModifier;
            }
        }
        else //air
        {
            actualFriction = airFriction;
        }

        velocity = Vector4.Scale(
            velocity , 
            Vector4.Max(
                Vector4.zero, 
                Vector4.one - actualFriction * Time.deltaTime
            )
        );

        // grapple
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Grapple(cam4D.t4d.position, cam4D.t4d.forward);
        }

        // grapple velocity
        if (hookPoint is Vector4 hp)
        {
            Vector4 playerPos = t4d.position;
            Vector4 delta = (hp - playerPos);
            Vector4 dir = delta.normalized;

            // if angle between look, hook is >90 degrees or we reach the dest point, stop grappling
            if (Util.Angle(cam4D.t4d.forward, dir) > Mathf.PI / 2 || Vector4.Magnitude(delta) <= 1e-1)
            {
                hookPoint = null;
                sliceRenderer.Shape = null;
            }
            else
            {
                DrawGrapple(localGrappleStart: new Vector4(0.5f, 1f, 0.5f, 0f), hp);
                // grapple affects player velocity
                var curVel = Vector4.Dot(velocity, dir);
                velocity += dir * Math.Max(0, (grapplingVelocity - curVel) * grappleAccelFactor);
            }
        }

        if(upCollisions.Any() && velocity.y > 0){ // Collide with something above. Can this cause a condition where phase through platform if platform above and below??
            velocity.y = 0;
        }


        // gravity calcs
        if (grounded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                velocity.y = jumpSpeed;
            }
            else
            {
                if (velocity.y < 0)
                    velocity.y = 0;

                // force position to ground
                t4d.position = t4d.position.withY(Mathf.Max(downCollisions.Max().point.y, t4d.position.y)); 
            }
        }
        else //else gravity
        {
            if (useGravity)
                velocity.y -= gravity * Time.deltaTime;
        }

        //* Stopping movement if colliding
        if(forwardCollisions.Any()){
            var cx = fowardDirection.x;
            var cz = fowardDirection.z;
            if(velocity.x * cx > 0){ // If heading in that direction stop
                velocity.x = 0;
            }
            if(velocity.z * cz > 0){ // If heading in that direction stop
                velocity.z = 0;
            }
        }
        if(backwardCollisions.Any()){
            var cx = backDirection.x;
            var cz = backDirection.z;
            if(velocity.x * cx > 0){ // If heading in that direction stop
                velocity.x = 0;
            }
            if(velocity.z * cz > 0){ // If heading in that direction stop
                velocity.z = 0;
            }
        }
        if(forwardCollisions.Any()){
            var cx = fowardDirection.x;
            var cz = fowardDirection.z;
            if(velocity.x * cx > 0){ // If heading in that direction stop
                velocity.x = 0;
            }
            if(velocity.z * cz > 0){ // If heading in that direction stop
                velocity.z = 0;
            }
        }
        if(rightCollisions.Any()){
            var cx = rightDirection.x;
            var cz = rightDirection.z;
            if(velocity.x * cx > 0){ // If heading in that direction stop
                velocity.x = 0;
            }
            if(velocity.z * cz > 0){ // If heading in that direction stop
                velocity.z = 0;
            }
        }
        if(leftCollisions.Any()){
            var cx = leftDirection.x;
            var cz = leftDirection.z;
            if(velocity.x * cx > 0){ // If heading in that direction stop
                velocity.x = 0;
            }
            if(velocity.z * cz > 0){ // If heading in that direction stop
                velocity.z = 0;
            }
        }


        cam4D.camera3D.fieldOfView = Mathf.Lerp(cam4D.camera3D.fieldOfView, Mathf.Lerp(
            normalFov,
            sprintFov,
            Mathf.InverseLerp(maxStrafeSpeed, maxSprintSpeed, velocity.magnitude)
        ), fovSmoothing * Time.deltaTime);

        t4d.position += t4d.LocalDirectionToWorld(velocity) * Time.deltaTime;
    }
}
