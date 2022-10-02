using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CustomScaler : MonoBehaviour
{
    private float screenWidth;
    private float screenHeight;
    public CanvasScaler canvasScaler;
    public int refWidth;
    public int refHeight;
    public static float canvasScale;
    private void Update()
    {
        if (screenWidth != Screen.width || screenHeight != Screen.height)
        {
            screenWidth = Screen.width;
            screenHeight = Screen.height;

            float widthScale = screenWidth / refWidth;
            float heightScale = screenHeight / refHeight;
            float finalScale = (widthScale + heightScale) * .5f;
            canvasScaler.scaleFactor = Mathf.Clamp(Mathf.Round(finalScale * 2) / 2, 1, 10);
            canvasScale = canvasScaler.scaleFactor;
        }
    }
}
