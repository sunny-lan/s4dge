using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using v2;

public class Demo2 : MonoBehaviour
{
    Transform4D t4d;

    public float v = 0;

    void Start()
    {
        t4d = GetComponent<Transform4D>();
    }

    // Update is called once per frame
    void Update()
    {
        t4d.localRotation[(int)Rot4D.zw] += v * Time.deltaTime;
    }
}
