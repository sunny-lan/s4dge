using UnityEngine;
using S4DGE;

namespace S4DGE
{
    public class Movement : MonoBehaviour
    {
        Transform4D t4d;
        float rotation = 0;
        public float rotSpeed = Mathf.PI;
        public float amplitude = 3;

        void Start()
        {
            t4d = GetComponent<Transform4D>();
        }

        void Update()
        {
            t4d.localPosition = Mathf.Sin(rotation) * new Vector4(1,0,0,1) * amplitude;
            rotation = (rotation + rotSpeed*Time.deltaTime)%(Mathf.PI*2);
        }
    }
}