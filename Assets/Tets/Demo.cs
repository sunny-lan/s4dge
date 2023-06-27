
using UnityEngine;
using v2;

public class Demo : MonoBehaviour
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
        t4d.localRotation[(int)Rot4D.xz] += Time.deltaTime;
        t4d.localRotation[(int)Rot4D.xw] += v*Time.deltaTime;
    }
}
