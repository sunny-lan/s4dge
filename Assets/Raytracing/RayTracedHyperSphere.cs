using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracedHyperSphere : RayTracedShape
{
	public float radius;

	protected new void Awake()
	{
		base.Awake();

		shapeClass = ShapeClass.HyperSphere;
	}
}
