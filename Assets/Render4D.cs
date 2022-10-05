using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Render4D : MonoBehaviour
{
    Vector4 cameraPos = new(0, 0, 0, -3);
    float screenPlane = 1;

    Shape4D shape;
    List<Shape4D> shapesList;

    public Vector4 cameraTransform(Vector4 worldPos) {
        return worldPos - cameraPos;
    }

    public Vector3 project(Vector4 v, float plane)
    {
        return new Vector3(v.x, v.y, v.z)*plane/v.w;
    }

    public Vector4 rotate(Vector4 v, int axis1, int axis2, float theta)
    {
        Matrix4x4 matrix = Matrix4x4.identity;
        matrix[axis1, axis1] = MathF.Cos(theta);
        matrix[axis1, axis2] = -MathF.Sin(theta);
        matrix[axis2, axis1] = MathF.Sin(theta);
        matrix[axis2, axis2] = MathF.Cos(theta);

        return matrix * v;
    }


   public void drawCircle(Vector4 axis1, Vector4 axis2)
    {
        Vector4 lst = (axis2);
        for (float theta = 0; theta < 2*Mathf.PI; theta += Mathf.PI / 100)
        {
            Vector4 cur = axis1 * Mathf.Sin(theta) + axis2 * Mathf.Cos(theta);
            line(lst, cur);
            lst = cur;
        }
    }

    private void Awake() // init shape
    {
        shapesList = new List<Shape4D>();
        shapesList.Add(new Cube(this));
        shapesList.Add(new FiveCell(this));
        shapesList.Add(new InclinedPlane(this));
        shape = shapesList[2]; // Set which shape to render
        
        ResetMesh();
    }

    private void ResetMesh()
    {
        triangles = new();
        mesh = new() { vertices = new Vector3[] { }, triangles = new int[0] };
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void line(Vector4 p, Vector4 q)
    {
        Debug.DrawLine((project(cameraTransform(p), screenPlane)), project(cameraTransform(q), screenPlane));
    }

    Mesh mesh;
    List<Vector3> vertices = new();
    List<int> triangles = new();

    public void drawTriangle(params Vector4[] triangleVertices)
    {
        this.vertices.AddRange(triangleVertices.Select(v => project(cameraTransform(v), screenPlane)));
        for (int i = 0; i < 3; i++) triangles.Add(triangles.Count);

        this.vertices.AddRange(triangleVertices.Select(v => project(cameraTransform(v), screenPlane)).Reverse());
        for (int i = 2; i >= 0; i--) triangles.Add(triangles.Count);
    }

    public void drawSquare(Vector4[] vertices)
    {
        drawTriangle(vertices[0], vertices[1], vertices[2]);
        drawTriangle(vertices[2], vertices[3], vertices[0]);
    }


    public void line(Vector3 p, Vector3 q)
    {
        Debug.DrawLine(p,q);
    }

    public void drawTriangle(params Vector3[] triangleVertices)
    {
        this.vertices.AddRange(triangleVertices);
        for (int i = 0; i < 3; i++) triangles.Add(triangles.Count);

        this.vertices.AddRange(triangleVertices.Reverse());
        for (int i = 2; i >= 0; i--) triangles.Add(triangles.Count);
    }

    public void drawSquare(params Vector3[] vertices)
    {
        drawTriangle(vertices[0], vertices[1], vertices[2]);
        drawTriangle(vertices[2], vertices[3], vertices[0]);
    }

    private void Update()
    {
        triangles.Clear();
        vertices.Clear();

        Camera.main.transform.forward = project(cameraTransform(Vector4.zero), screenPlane) - Camera.main.transform.position;

        shape.drawLines();
        shape.fillFaces();


        //drawCircle(new(0, 0, 0, 1), new(1, 0, 0, 0));
        //drawCircle(new(0, 0, 1, 0), new(1, 0, 0, 0));
        //drawCircle(new(0, 1, 0, 0), new(1, 0, 0, 0));
        //drawCircle(new(0, 0, 1, 0), new(0, 0, 0, 1));

        // Slicing Visualization Key inputs

        if (Input.GetKey(KeyCode.LeftBracket))
        {
            shape.w += Time.deltaTime * 2;
        }
        if (Input.GetKey(KeyCode.RightBracket))
        {
            shape.w -= Time.deltaTime * 2;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            cameraPos.x += Time.deltaTime * 2;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            cameraPos.x -= Time.deltaTime * 2;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            cameraPos.y += Time.deltaTime * 2;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            cameraPos.y -= Time.deltaTime * 2;
        }
        if (Input.GetKey(KeyCode.A))
        {
            cameraPos.z += Time.deltaTime * 2;
        }
        if (Input.GetKey(KeyCode.S))
        {
            cameraPos.z -= Time.deltaTime * 2;
        }
        if (Input.GetKey(KeyCode.Z))
        {
            cameraPos.w += Time.deltaTime * 2;
        }
        if (Input.GetKey(KeyCode.X))
        {
            cameraPos.w -= Time.deltaTime * 2;
        }
        // Use R key to swap rotation modes
        if (Input.GetKeyUp(KeyCode.R))
        {
            shape.useAxialRotations = !shape.useAxialRotations;
        }
        // use tab to switch shapes
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            ResetMesh();
            shape = shapesList[(shapesList.FindIndex((s) => s == shape) + 1) % shapesList.Count];
        }
        if (Input.GetKey(KeyCode.Q) && !shape.useAxialRotations )
        {
            shape.rotation += Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.W) && !shape.useAxialRotations )
        {
            shape.rotation -= Time.deltaTime;
        }

        if ( shape.useAxialRotations )
        {
            CheckAxialRotationInputs();
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
    }

    /*
        Y and U : +/- rotation around within wx plane
        H and J : +/- rotation around within wy plane
        N and M : +/- rotation around within wz plane
        I and O : +/- rotation around within xy plane
        K and L : +/- rotation around within xz plane
        , and . : +/- rotation around within yz plane
    */
    private void CheckAxialRotationInputs() {
        // get input for all axial rotations, which takes 4 keys horizontally (Y to O) and 3 keys vertically (Y to N)
        if( Input.GetKey(KeyCode.Y) ) {
            shape.allRotations[0] += Time.deltaTime;
        }
        if( Input.GetKey(KeyCode.U) ) {
            shape.allRotations[0] -= Time.deltaTime;
        }
        if( Input.GetKey(KeyCode.H) ) {
            shape.allRotations[1] += Time.deltaTime;
        }
        if( Input.GetKey(KeyCode.J) ) {
            shape.allRotations[1] -= Time.deltaTime;
        }
        if( Input.GetKey(KeyCode.N) ) {
            shape.allRotations[2] += Time.deltaTime;
        }
        if( Input.GetKey(KeyCode.M) ) {
            shape.allRotations[2] -= Time.deltaTime;
        }
        if( Input.GetKey(KeyCode.I) ) {
            shape.allRotations[3] += Time.deltaTime;
        }
        if( Input.GetKey(KeyCode.O) ) {
            shape.allRotations[3] -= Time.deltaTime;
        }
        if( Input.GetKey(KeyCode.K) ) {
            shape.allRotations[4] += Time.deltaTime;
        }
        if( Input.GetKey(KeyCode.L) ) {
            shape.allRotations[4] -= Time.deltaTime;
        }
        if( Input.GetKey(KeyCode.Comma ) ) {
            shape.allRotations[5] += Time.deltaTime;
        }
        if( Input.GetKey(KeyCode.Period ) ) {
            shape.allRotations[5] -= Time.deltaTime;
        }
    }

    public Vector4 Vector4DeepCopy( Vector4 a ) {
        Vector4 b = new Vector4();
        b.w = a.w;
        b.x = a.x;
        b.y = a.y;
        b.z = a.z;
        return b;
    }

    //private void OnDrawGizmos()
    //{

    //    foreach (var p in points)
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawSphere(project(p, 1), 0.05f);
    //    }
    //}
}
