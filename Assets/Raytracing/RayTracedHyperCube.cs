using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracedHyperCube : RayTracedShape
{
	public Vector4 p1;
	public Vector4 p2;

	protected new void Awake()
	{
		base.Awake();

		shapeClass = ShapeClass.HyperCube;
	}
}
