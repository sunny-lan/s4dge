using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Camera4D : MonoBehaviour
{
    /// <summary>
    /// the current active camera
    /// </summary>
    public static Camera4D main = null;

    /// <summary>
    /// The 4D transform of this camera.
    /// </summary>
    public Transform4D t4d { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        t4d = GetComponent<Transform4D>();

        if (main == null)
            main = this;
    }
}
