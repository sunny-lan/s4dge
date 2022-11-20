using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class Camera4D : MonoBehaviour
{
    /// <summary>
    /// the current active camera
    /// </summary>
    public static Camera4D main
    {
        get
        {
            if(_main == null)
            {
                _main= FindObjectOfType<Camera4D>();
            }
            return _main;
        }

        set
        {
            _main = value;
        }
    }
    static Camera4D _main = null;

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

    static int cameraPosShaderID = Shader.PropertyToID("_4D_Camera_Pos");
    private void Update()
    {
        if (this != Camera4D.main) return;

        // pass camera position to shader
        Shader.SetGlobalVector(cameraPosShaderID, t4d.position);

        // sync 3D camera to 4d camera position
        // TODO somehow sync rotation?
        Camera.main.transform.position = t4d.position;
    }
}
