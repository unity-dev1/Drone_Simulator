using UnityEngine;

public class LandingPad : MonoBehaviour
{
    public bool isLanding = false;

    private void Awake()
    {
        isLanding = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            isLanding = true;
        }
    }
}
