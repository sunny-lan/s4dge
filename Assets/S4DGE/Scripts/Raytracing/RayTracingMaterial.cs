using UnityEngine;

/// <summary>
/// The material of a 4D object
/// </summary>
namespace RaytraceRenderer
{
	[System.Serializable]
	public struct RayTracingMaterial
	{
		/// <summary>
		/// Material types supported by the ray tracer
		/// </summary>
		public enum MaterialFlag
		{
			None,
			CheckerPattern,
			InvisibleLight
		}

		/// <summary>
		/// The base colour of the object
		/// </summary>
		public Color colour;

		/// <summary>
		/// The glow colour of the object
		/// </summary>
		public Color emissionColour;

		/// <summary>
		/// The specular reflection colour of the object
		/// </summary>
		public Color specularColour;

		/// <summary>
		/// The strength of the light emission from the object
		/// </summary>
		public float emissionStrength;

		/// <summary>
		/// The smoothness of the object
		/// </summary>
		[Range(0, 1)] public float smoothness;

		/// <summary>
		/// The chance of a light ray reflecting specularly from the object surface
		/// </summary>
		[Range(0, 1)] public float specularProbability;

		/// <summary>
		/// The general type of material of the object
		/// </summary>
		public MaterialFlag flag;

		/// <summary>
		/// Reset the material to the default white material
		/// </summary>
		public void SetDefaultValues()
		{
			colour = Color.white;
			emissionColour = Color.white;
			emissionStrength = 0;
			specularColour = Color.white;
			smoothness = 0;
			specularProbability = 1;
		}
	}
}