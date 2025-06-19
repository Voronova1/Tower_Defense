using UnityEngine;
using UnityStandardAssets.Water;

[ExecuteInEditMode]
[RequireComponent(typeof(Water))]
public class WaterFix : MonoBehaviour
{
    private Water water;
    private Camera currentCamera;

    void OnEnable()
    {
        water = GetComponent<Water>();
        Camera.onPreCull += OnCameraPreCull;
    }

    void OnDisable()
    {
        Camera.onPreCull -= OnCameraPreCull;
    }

    void OnCameraPreCull(Camera cam)
    {
        if (!water.enabled || !water.GetComponent<Renderer>().enabled)
            return;

        currentCamera = cam;
        water.RenderWaterEffects(cam);
    }
}