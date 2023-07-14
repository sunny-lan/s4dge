using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracedHyperSphere : RayTracedShape
{
	public float radius;

	public HyperSphereScriptableObject HyperSphereScriptableObject;

	protected new void Awake()
	{
		base.Awake();

		shapeClass = ShapeClass.HyperSphere;

		if (HyperSphereScriptableObject != null) {
			radius = HyperSphereScriptableObject.radius;
			material = HyperSphereScriptableObject.material;

			transform4D.localPosition = HyperSphereScriptableObject.transform4D.localPosition;
			transform4D.localScale = HyperSphereScriptableObject.transform4D.localScale;
			
		}
	}
}
