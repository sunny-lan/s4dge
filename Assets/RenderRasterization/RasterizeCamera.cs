using RasterizationRenderer;
using UnityEngine;

public class RasterizeCamera : MonoBehaviour
{
    public bool enableShadows;
    public bool showShadowMap;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!showShadowMap)
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
    }

    private void OnGUI()
    {
        if (showShadowMap)
        {
            foreach (var lightSource in FindObjectsByType<LightSource4D>(FindObjectsSortMode.InstanceID))
            {
                lightSource.UpdateShadowMap(true);
                //RenderUtils.PrintTexture(lightSource.ShadowMap, 6);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), lightSource.ShadowMap);
            }
        }
    }
}
