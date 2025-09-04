using JetBrains.Annotations;
using UnityEngine;

public class ClippingInGround : MonoBehaviour
{
    public DRONECONT dc;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8 )  //drone dont collide with each other
        {
            Debug.Log("inground");
            dc.inGround = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 8 || other.gameObject.layer == 9)  //drone dont collide with each other
        {
            Debug.Log("out of ground");
            dc.inGround = false;
        }
    }
}
