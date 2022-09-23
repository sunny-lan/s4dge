using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Render4D : MonoBehaviour
{
    Vector4 cameraPos = new(0, 0, 0, 0);
    float plane = 1;

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

    List<Vector4> points = new List<Vector4>()
    {
    };

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
        for (int i = 0; i < (1 << 4); i++)
        {
            points.Add(new(i&1, (i>>1)&1, (i>>2)&1, (i>>3)&1));
            Debug.Log(points[i]);

        }
    }

    void line(Vector4 p, Vector4 q)
    {
        Debug.DrawLine((project(cameraTransform(p), plane)), project(cameraTransform(q), plane));
    }

    private void Update()
    {
        Camera.main.transform.forward = project(cameraTransform(Vector4.zero), plane) - Camera.main.transform.position;

        foreach (var p in points) foreach (var q in points)
        {
                if((int)((p-q).sqrMagnitude) == 1)
                {
                    line(p, q);
                    
                }
            }

        drawCircle(new(0, 0, 0, 1), new(1, 0, 0, 0));
        drawCircle(new(0, 0, 1, 0), new(1, 0, 0, 0));
        drawCircle(new(0, 1, 0, 0), new(1, 0, 0, 0));
        drawCircle(new(0, 0, 1, 0), new(0, 0, 0, 1));

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
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = rotate(points[i], 2, 3, Time.deltaTime);
                // points[i] = rotate(points[i], 0, 1, Time.deltaTime / 10);
            }
        }
        if (Input.GetKey(KeyCode.W))
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = rotate(points[i], 2, 3, -Time.deltaTime);
                // points[i] = rotate(points[i], 0, 1, -Time.deltaTime / 10);
            }
        }
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
