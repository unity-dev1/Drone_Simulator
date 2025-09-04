using UnityEngine;
using System.Collections;

public class IndicatorBlinkers : MonoBehaviour
{
    public MeshRenderer frontIndicator;
    public MeshRenderer backIndicator;
    
    [Header("Blinker Settings")]
    public float blinkRate = 0.5f;
    
    private void Start()
    {
        // Start blinking automatically
        StartCoroutine(BlinkIndicators());
    }
    
    private IEnumerator BlinkIndicators()
    {
        while (true) // Run forever
        {
            // Turn on both indicators
            if (frontIndicator != null)
            {
                frontIndicator.material.EnableKeyword("_EMISSION");
                frontIndicator.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
            if (backIndicator != null)
            {
                backIndicator.material.EnableKeyword("_EMISSION");
                backIndicator.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
            
            yield return new WaitForSeconds(blinkRate);
            
            // Turn off both indicators
            if (frontIndicator != null)
            {
                frontIndicator.material.DisableKeyword("_EMISSION");
            }
            if (backIndicator != null)
            {
                backIndicator.material.DisableKeyword("_EMISSION");
            }
            
            yield return new WaitForSeconds(blinkRate);
        }
    }
}
