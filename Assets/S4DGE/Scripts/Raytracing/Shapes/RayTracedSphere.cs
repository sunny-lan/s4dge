using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A 3-sphere that can be attached to a 4D gameobject in a raytracing scene
/// </summary>
namespace RaytraceRenderer
{
	public class RayTracedSphere : RayTracedShape
	{
		/// <summary>
		/// The radius of the 3-sphere
		/// </summary>
		public float radius;

		protected new void Awake()
		{
			base.Awake();

			shapeClass = ShapeClass.Sphere;
		}
	}
}