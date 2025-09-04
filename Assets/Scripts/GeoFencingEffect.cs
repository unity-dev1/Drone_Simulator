using TMPro;
using UnityEngine;

public class GeoFencingEffect : MonoBehaviour
{
    public SpriteRenderer effectImage1;
    public SpriteRenderer effectImage2;

    public Color32 warningColor;
    public Color32 finalError;

    public void ChangeColor(Color color) 
    {
        if (effectImage1 != null && effectImage1.material != null)
        {
            effectImage1.material.SetColor("_GlowColor", color);
            Debug.Log($"Setting glow color to: {color} for effectImage1");
        }
        
        if (effectImage2 != null && effectImage2.material != null)
        {
            effectImage2.material.SetColor("_GlowColor", color);
            Debug.Log($"Setting glow color to: {color} for effectImage2");
        }
    }

    public void WarningEffect() 
    {
        ChangeColor(warningColor);
    }

    public void FinalWarning() 
    {
        ChangeColor(finalError);
    }
}
