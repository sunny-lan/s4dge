using UnityEngine;
using v2;

public class LightSource4D : MonoBehaviour
{
    Transform4D transform4D;

    public Data data { get => new(transform4D.position); }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Data
    {
        public Vector4 position;

        public Data(Vector4 position) { this.position = position; }

        public static int SizeBytes { get => sizeof(float) * 4; }
    }

    private void Start()
    {
        transform4D = GetComponent<Transform4D>();
    }
}
