
using UnityEngine;
using S4DGE;

namespace S4DGE
{
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
}