using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using v2;

public class RotationControlDemo : MonoBehaviour
{
    public Transform4D t4d;
    public float rotSpeed = 2.5f; // scales the 'speed' of changes in w

    void Start()
    {
        t4d = GetComponent<Transform4D>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Alpha1)) // 1 key above alphabet keys
        {
            t4d.localRotation[0] += rotSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            t4d.localRotation[0] -= rotSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Alpha2)) // 2 key above alphabet keys
        {
            t4d.localRotation[1] += rotSpeed * Time.deltaTime;
        } 
        else if (Input.GetKey(KeyCode.W))
        {
            t4d.localRotation[1] -= rotSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Alpha3)) // 3 key above alphabet keys
        {
            t4d.localRotation[2] += rotSpeed * Time.deltaTime;
        } 
        else if (Input.GetKey(KeyCode.E))
        {
            t4d.localRotation[2] -= rotSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Alpha4)) // 4 key above alphabet keys
        {
            t4d.localRotation[3] += rotSpeed * Time.deltaTime;
        } 
        else if (Input.GetKey(KeyCode.R))
        {
            t4d.localRotation[3] -= rotSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Alpha5)) // 5 key above alphabet keys
        {
            t4d.localRotation[4] += rotSpeed * Time.deltaTime;
        } 
        else if (Input.GetKey(KeyCode.T))
        {
            t4d.localRotation[4] -= rotSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Alpha6)) // 6 key above alphabet keys
        {
            t4d.localRotation[5] += rotSpeed * Time.deltaTime;
        } else if (Input.GetKey(KeyCode.Y))
        {
            t4d.localRotation[5] -= rotSpeed * Time.deltaTime;
        }
    }
}
