using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    Camera4D camera4d;
    // Start is called before the first frame update
    void Start()
    {
        camera4d = GetComponent<Camera4D>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey(KeyCode.LeftBracket))
        {
            camera4d.t4d.localPosition.w += Time.deltaTime * 2;
        }
        if (Input.GetKey(KeyCode.RightBracket))
        {
            camera4d.t4d.localPosition.w -= Time.deltaTime * 2;
        }
    }
}
