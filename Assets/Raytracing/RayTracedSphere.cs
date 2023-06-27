using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracedSphere : RayTracedShape
{
	public float radius;

	protected new void Awake()
	{
		base.Awake();

		shapeClass = ShapeClass.Sphere;
	}
}
