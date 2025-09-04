using UnityEngine;

public class CollisionDirectionManager : MonoBehaviour
{
    public CollidingDirection collidingDirection;
    public GeofencingWarning geofencingWarning;



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7)
        {
            if (geofencingWarning.collidingDirection == CollidingDirection.None)
                geofencingWarning.onGetDirection.Invoke(collidingDirection, SelectionManager.Instance.selectedDrone.index);
        }
    }
}
