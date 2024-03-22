
using UnityEngine;
using v2;

public class Rotator : MonoBehaviour
{
    Transform4D t4d;


    public float[] rotSpeed = new float[6];

    void Start()
    {
        t4d = GetComponent<Transform4D>();
    }

    // Update is called once per frame
    void Update()
    {
        for(int i=0;i<6;i++)
            t4d.localRotation[i] += Time.deltaTime*rotSpeed[i];
    }
}
