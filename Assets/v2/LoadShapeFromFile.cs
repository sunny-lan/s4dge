using OpenCover.Framework.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using v2;

public class LoadShapeFromFile : MonoBehaviour
{
    public string filePath= "Assets/Models/inclinedPlaneModel.s4dge";
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SliceRenderer>().shape = new InterpolationBasedShape(filePath);
    }
}
