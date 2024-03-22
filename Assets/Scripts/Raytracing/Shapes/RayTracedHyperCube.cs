using UnityEngine;

/// <summary>
/// A hypercube that can be attached to a 4D gameobject in a raytracing scene
/// </summary>
public class RayTracedHyperCube : RayTracedShape
{
	/// <summary>
	/// The bottom left corner (minimal coordinate) of the hypercube
	/// </summary>
	public Vector4 p1;

	/// <summary>
	/// The top right corner (maximal coordinate) of the hypercube
	/// </summary>
	public Vector4 p2;

	protected new void Awake()
	{
		base.Awake();

		shapeClass = ShapeClass.HyperCube;
	}
}
