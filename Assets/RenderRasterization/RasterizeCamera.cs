using RasterizationRenderer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RasterizeCamera : MonoBehaviour
{
    public bool enableShadows;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // generate shadow maps
        foreach (var lightSource in FindObjectsByType<LightSource4D>(FindObjectsSortMode.InstanceID))
        {
            lightSource.UpdateShadowMap(enableShadows);
        }


        // draw each object
        foreach (var rasterizeObject in FindObjectsByType<RasterizeObject>(FindObjectsSortMode.InstanceID))
        {
            rasterizeObject.DrawFrame();
        }
    }

    //private void OnGUI()
    //{
    //    foreach (var lightSource in FindObjectsByType<LightSource4D>(FindObjectsSortMode.InstanceID))
    //    {
    //        lightSource.UpdateShadowMap();
    //        //RenderUtils.PrintTexture(lightSource.ShadowMap, 6);
    //        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), lightSource.ShadowMap);
    //    }
    //}
}
