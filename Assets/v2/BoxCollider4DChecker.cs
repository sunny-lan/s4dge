using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using v2;


public struct Box
{
    public Vector4 corner;
    public Vector4 size;
    public Transform4D t4d;
}

public class BoxCollider4DChecker
{

    public static Ray4D.Intersection? Min(Ray4D.Intersection? a, Ray4D.Intersection? b)
    {
        if (a == null) return b;
        if (b == null) return a;
        if (a is Ray4D.Intersection aa && b is Ray4D.Intersection bb)
        {
            return aa.CompareTo(bb) < 0 ? aa : bb;
        }
        else
        {
            // dead code
            return null;
        }
    }

    public static Ray4D.Intersection? RayIntersect(Box box, Ray4D ray)
    {
        var corner = box.corner;
        var size = box.size;
        var t4d = box.t4d;

        // transform to local coordinates
        ray = t4d.WorldToLocal(ray);
        ray.src -= corner;

        // faces : x = 0, x = size.x, ..., w = 0, w = size.x
        Ray4D.Intersection? firstIntersect = null;
        for (int face = 0; face < 4; ++face)
        {
            firstIntersect = Min(firstIntersect, ray.intersectPlane(face, 0, size));
            firstIntersect = Min(firstIntersect, ray.intersectPlane(face, size[face], size));
        }

        // transform back to world coordinates
        if (firstIntersect is Ray4D.Intersection intersect)
        {
            intersect.point = t4d.LocalToWorld(intersect.point + corner);
            firstIntersect = intersect;
        }

        return firstIntersect;
    }

    public static bool ContainsPoint(Box box, Vector4 p)
    {
        var corner = box.corner;
        var size = box.size;
        var t4d = box.t4d;

        p = t4d.WorldToLocal(p); //transform to local coordinates

        //check if in box
        p -= corner;
        bool colliding = p.x >= 0 && p.y >= 0 && p.z >= 0 && p.w >= 0
            &&
            p.x <= size.x && p.y <= size.y && p.z <= size.z && p.w <= size.w;

        if (colliding)
        {
            Log.Print("Collision detected at: " + p.x + " " + p.y + " " + p.z + " " + p.w, Log.collisions);
        }
        return colliding;

    }

    //TODO performance
    public static IEnumerable<Vector4> GetCorners(Box box)
    {
        var corner = box.corner;
        var size = box.size;
        var t4d = box.t4d;

        for (int i = 0; i < (1 << 4); i++)
        {
            Vector4 offset = size;
            offset.Scale(new Vector4(
                (i >> 0) & 1,
                (i >> 1) & 1,
                (i >> 2) & 1,
                (i >> 3) & 1
            ));
            yield return t4d.LocalToWorld(corner + offset);
        }
    }

    public static bool DoesCollide(Box box, Box other)
    {

        foreach (var corner in GetCorners(box))
        {
            if (ContainsPoint(box, other.corner))
            {
                return true;
            }
        }
        return false;
    }
}