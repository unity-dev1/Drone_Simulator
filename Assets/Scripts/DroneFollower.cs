using UnityEngine;

public class DroneFollower : MonoBehaviour
{
    [Tooltip("Reference to the drone GameObject that this object should follow")]
    public GameObject droneToFollow;

    [Tooltip("How smoothly the object follows the drone (higher value = smoother movement)")]
    public float smoothSpeed = 5f;

    [Tooltip("Initial offset from the drone")]
    public Vector3 initialOffset;

    private float lastDroneYaw;

    private void Start()
    {
        if (droneToFollow != null)
        {
            // Calculate the initial offset from the drone
            initialOffset = transform.position - droneToFollow.transform.position;
            lastDroneYaw = droneToFollow.transform.eulerAngles.y;
        }
    }

    private void Update()
    {
        if (droneToFollow == null)
            return;

        // Calculate target position using the initial offset
        Vector3 targetPosition = droneToFollow.transform.position + initialOffset;

        // Smoothly move to the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // Get the drone's current yaw rotation and apply it
        float currentDroneYaw = droneToFollow.transform.eulerAngles.y;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, currentDroneYaw, 0);

        lastDroneYaw = currentDroneYaw;
    }
} 