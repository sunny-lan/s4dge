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

    Vector4 cameraTransform(Vector4 worldPos) {
        return worldPos - cameraPos;
    }

    Vector3 project(Vector4 v, float plane)
    {
        return new Vector3(v.x, v.y, v.z)*plane/v.w;
    }

    Vector4 rotate(Vector4 v, int axis1, int axis2, float theta)
    {
        Matrix4x4 matrix = Matrix4x4.identity;
        matrix[axis1, axis1] = MathF.Cos(theta);
        matrix[axis1, axis2] = -MathF.Sin(theta);
        matrix[axis2, axis1] = MathF.Sin(theta);
        matrix[axis2, axis2] = MathF.Cos(theta);

        return matrix * v;
    }


    void drawCircle(Vector4 axis1, Vector4 axis2)
    {
        Vector4 lst = (axis2);
        for (float theta = 0; theta < 2*Mathf.PI; theta += Mathf.PI / 100)
        {
            Vector4 cur = axis1 * Mathf.Sin(theta) + axis2 * Mathf.Cos(theta);
            line(lst, cur);
            lst = cur;
        }
    }

    private void Awake()
    {
        cube = new(this);
        triangles = new();
        mesh = new() { vertices = new Vector3[] { }, triangles = new int[0] };
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void line(Vector4 p, Vector4 q)
    {
        Debug.DrawLine((project(cameraTransform(p), screenPlane)), project(cameraTransform(q), screenPlane));
    }

    class Cube
    {
        Render4D render;
        List<Vector4> points = new List<Vector4>()
        {
        };
        HashSet<(Vector4 a, Vector4 b)> lines= new();
        Dictionary<Vector4, HashSet<Vector4>> adj = new();
        List<Vector4[]> faces = new();

        public float rotation;


        public Cube(Render4D r4)
        {
            this.render = r4;
            // add point (0, 0, 0, 0), (0, 0, 0, 1), ..., (1, 1, 1, 1)
            for (int i = 0; i < (1 << 4); i++)
            {
                points.Add(new(i & 1, (i >> 1) & 1, (i >> 2) & 1, (i >> 3) & 1));
                Debug.Log(points[i]);
                adj[points[i]] = new();
            }

            // add lines
            foreach (var p in points)
            {
                foreach (var q in points) 
                {
                    // draw a line between points if they differ in exactly one coordinate
                    if (Mathf.Round((p - q).sqrMagnitude) == 1)
                    {
                        lines.Add((p, q));
                        adj[p].Add(q);
                    }
                }
            }
            getFaces();
        }

        Vector4 rotate(Vector4 vertex)
        {
            return render.rotate(render.rotate(vertex, 1, 3, rotation),2,0,rotation*2);
        }

        public void drawLines()
        {
            foreach (var l in lines) 
                render.line(rotate(l.a), rotate(l.b));
        }

        public void fillFaces()
        {
            foreach(var face in faces)
                render.drawSquare(face.Select(rotate).ToArray());
        }

        private void getFaces()
        {
            for (int i=0;i<points.Count;i++)
            {
                Vector4 p_i = points[i];
                for (int j = i + 1; j < points.Count; j++)
                {
                    Vector4 p_j = points[j];
                    if (!adj[p_i].Contains(p_j)) continue; // check if the first two lines connect

                    for (int k = i + 1; k < points.Count; k++)
                    {
                        Vector4 p_k = points[k];
                        if (!adj[p_j].Contains(p_k)) continue; // 2nd pair of lines, ...
                        for (int l = i + 1; l < points.Count; l++)
                        {
                            Vector4 p_l = points[l];
                            if (!adj[p_k].Contains(p_l) || !adj[p_l].Contains(p_i)) continue;

                            faces.Add(new Vector4[] { p_i, p_j, p_k, p_l });
                        }
                    }
                }
            }
        }

    }

    Cube cube;
    Mesh mesh;
    List<Vector3> vertices = new();
    List<int> triangles = new();

    void drawTriangle(params Vector4[] triangleVertices)
    {
        this.vertices.AddRange(triangleVertices.Select(v => project(cameraTransform(v), screenPlane)));
        for (int i = 0; i < 3; i++) triangles.Add(triangles.Count);
    }

    void drawSquare(Vector4[] vertices)
    {
        drawTriangle(vertices[0], vertices[1], vertices[2]);
        drawTriangle(vertices[2], vertices[3], vertices[0]);
    }
    

    private void Update()
    {
        triangles.Clear();
        vertices.Clear();

        Camera.main.transform.forward = project(cameraTransform(Vector4.zero), screenPlane) - Camera.main.transform.position;

        cube.drawLines();
        cube.fillFaces();


        //drawCircle(new(0, 0, 0, 1), new(1, 0, 0, 0));
        //drawCircle(new(0, 0, 1, 0), new(1, 0, 0, 0));
        //drawCircle(new(0, 1, 0, 0), new(1, 0, 0, 0));
        //drawCircle(new(0, 0, 1, 0), new(0, 0, 0, 1));

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
        if (Input.GetKey(KeyCode.Q))
        {
            cube.rotation += Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.W))
        {
            cube.rotation -= Time.deltaTime;
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
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
