using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class ColorEditor : MonoBehaviour
{
    public Texture2D colorTexture;
    private List<Color> pickedColors;
    public void PrintColors()
    {
        pickedColors = new List<Color>();
        string finalString = "";
        for (int x = 0; x < colorTexture.width; x++)
        {
            for (int y = 0; y < colorTexture.height; y++)
            {
                Color color = colorTexture.GetPixel(x, y);
                if (!pickedColors.Contains(color) && color.a == 1)
                {
                    pickedColors.Add(color);
                    finalString += "  - m_Name:\n    m_Color: { r: " + color.r + ", g: "+ color.g + ", b: "+ color.b + ", a: 1}\n";
                    //finalString += "Turn(float3(" + Mathf.Round(color.r * 255) + ", " + Mathf.Round(color.g * 255) + ", " + Mathf.Round(color.b * 255) + ") / 256);\n";
                }
            }
        }
        Debug.Log(finalString);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(ColorEditor))]
public class ColorEditorUI : Editor
{
    public override void OnInspectorGUI()
    {
        ColorEditor generator = (ColorEditor)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Print Colors"))
        {
            generator.PrintColors();
        }
    }
}
#endif

