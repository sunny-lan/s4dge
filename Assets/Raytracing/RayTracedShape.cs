using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using v2;

/// <summary>
/// The abstract base class for any shape that can be attached to a 4D gameobject in a raytracing scene
/// </summary>
[RequireComponent(typeof(Transform4D))]
[ExecuteInEditMode]
public abstract class RayTracedShape : MonoBehaviour
{
    /// <summary>
    /// The material of the shape
    /// </summary>
    public RayTracingMaterial material;

    /// <summary>
    /// The position and orientation of the shape
    /// </summary>
    public Transform4D transform4D { get; private set; }

    [HideInInspector] public ShapeClass shapeClass = ShapeClass.Unknown;
    
    [SerializeField, HideInInspector] int materialObjectID;
	[SerializeField, HideInInspector] bool materialInitFlag;

    protected void Awake()
    {
        transform4D = GetComponent<Transform4D>();
    }

    protected void OnDestroy()
    {
        if (Scene4D.Instance != null) // Check that scene was not also destroyed
        {
            Scene4D.Instance.Remove(this);
        }
    }

    void Start()
    {
        Scene4D.Instance.Register(this);
    }

	void OnValidate()
	{
		if (!materialInitFlag)
		{
			materialInitFlag = true;
			material.SetDefaultValues();
		}
	}
}
