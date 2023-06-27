using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using v2;

[RequireComponent(typeof(Transform4D))]
[ExecuteInEditMode]
public abstract class RayTracedShape : MonoBehaviour
{
    public RayTracingMaterial material;
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
