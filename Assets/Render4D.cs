using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Render4D : MonoBehaviour
{

    
    Vector3 project(Vector4 v, float plane)
    {
        return new Vector3(v.x, v.y, v.z)*plane/v.w;
    }

    List<Vector4> points = new List<Vector4>()
    {
    };

    

    private void Awake()
    {
        for (int i = 0; i < (1 << 4); i++)
        {
            points.Add(new(i&1, (i>>1)&1, (i>>2)&1, (i>>3)&1));
            Debug.Log(points[i]);

        }
    }

    private void Update()
    {
        float plane = 1;
        foreach(var p in points) foreach (var q in points)
        {
                if((int)((p-q).sqrMagnitude) == 1)
                {
                    Debug.DrawLine(project(p, plane), project(q, plane));
                    
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
