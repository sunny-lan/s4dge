using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using v2;

public class HyperCubeDemo : MonoBehaviour
{
    Transform4D t4d;
    void Start()
    {
        t4d=GetComponent<Transform4D>();
    }

    void Update()
    {
        t4d.localRotation[(int)Rot4D.yw] = 1f*Mathf.Sin(Time.time);
    }
}
