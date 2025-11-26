using UnityEngine;

public class DroneYawTrail : MonoBehaviour
{
    [Header("Yaw Trail Settings")]
    public Transform trailAnchor;   // empty child for orbit
    public TrailRenderer trail;     // glowing trail renderer
    public float radius = 2f;       // circle radius
    public float yTolerance = 0.05f; // how much vertical change is allowed

    // Tracks how much yaw rotation is completed
    private float accumulatedYaw = 0f;
    // Stores yaw value from last frame
    private float lastYaw;
    // Stores Y position where yaw started
    private float startY;

    // Flag to check if it's tutorial
    private bool isTutorial = true;
    public bool hasCompletedClockwise = false;
    public bool hasCompletedAnticlockwise = false;

    //DroneController reference
    private DRONECONT droneContoller;
    private float originalTorque;

    void Start()
    {
        if (trail != null)
        {
            trail.emitting = false;
        }
        // Record initial yaw rotation of the drone
        lastYaw = transform.eulerAngles.y;

        // Record starting vertical position
        startY = transform.position.y;
        droneContoller=GetComponent<DRONECONT>();
        originalTorque= droneContoller.yawTorque;
    }

    void Update()
    {

        //if it's tutorial & no anti-clockwise rotation is done
        if(isTutorial && !hasCompletedAnticlockwise)
        {
            DrawYawHighlight();
            droneContoller.yawTorque = 1.0f;

        }

        //if it's tutorial,anti-clockwise rotation is done but not clockwise rotation
        else if(isTutorial && hasCompletedAnticlockwise && !hasCompletedClockwise)
        {
            DrawYawHighlight();
            droneContoller.yawTorque = 1.0f;
        }

        //if tutorial is completed of rotation then remove trail
        else if(!isTutorial)
        {
           droneContoller.yawTorque=originalTorque;
           Destroy(this);
           Destroy(trailAnchor.gameObject);
        }
        
    }

    public void DisableTrail()
    {
        isTutorial = false;
    }
    void DrawYawHighlight()
    {
        //yaw rotation around Y axis
        float currentYaw = transform.eulerAngles.y;

        //Calculate how much yaw changed since last frame
        float deltaYaw = Mathf.DeltaAngle(lastYaw, currentYaw);

        lastYaw = currentYaw;

        // Check vertical drift
        bool withinHeight = Mathf.Abs(transform.position.y - startY) <= yTolerance;

        // Case 1: Drone is yawing, circle not yet complete, and within height tolerance
        if (Mathf.Abs(deltaYaw) > 0.01f && Mathf.Abs(accumulatedYaw) < 360f && withinHeight)
        {
            if (trail != null)
            {
                trail.emitting = true;
            }

            accumulatedYaw += deltaYaw;

            // converting yaw into rad
            float angleRad = Mathf.Deg2Rad * accumulatedYaw;

            // Calculate circular offset (orbit around drone center)
            Vector3 localOffset = new Vector3(Mathf.Sin(angleRad), 0, Mathf.Cos(angleRad)) * radius;

            //Applying offset to anchor, but lock Y so no curly vertical lines form 
            trailAnchor.localPosition = localOffset;
            trailAnchor.position = new Vector3(trailAnchor.position.x, transform.position.y, trailAnchor.position.z);
        }

        // Case 2: A full circle (-360°) has been completed
        else if (Mathf.Abs(accumulatedYaw) >= 360f)
        {

            if (!hasCompletedAnticlockwise)
            {
                // First clockwise rotation completed
                hasCompletedAnticlockwise = true;

                // Reset yaw accumulation (Change 1: Keep trail continuous, avoid breaks)
                accumulatedYaw = 0f;
                // Reset anchor to center
                trailAnchor.localPosition = Vector3.zero;
                // Reset reference Y position
                startY = transform.position.y; // reset base Y


            }

            //
            else if (hasCompletedAnticlockwise && !hasCompletedClockwise)
            {
                // First anti-clockwise rotation completed
                hasCompletedClockwise = true;

                // Reset yaw accumulation 
                accumulatedYaw = 0f;
                // Reset anchor to center
                trailAnchor.localPosition = Vector3.zero;
                // Reset reference Y position
                startY = transform.position.y; // reset base Y

                DisableTrail();
            }

            if (trail != null)
            {
                trail.emitting = false;
                trail.Clear();
            }


        }

        // Case 3: Circle is not complete & then ascend/descend, remove that created arc
        else if (Mathf.Abs(accumulatedYaw) > 0f && !withinHeight)
        {
            if (trail != null)
            {
                trail.emitting = false;
                // remove the partial arc
                trail.Clear();
            }
            accumulatedYaw = 0f;
            trailAnchor.localPosition = Vector3.zero;
            startY = transform.position.y; // reset base Y
        }

        // Case 4: No yawing or drone went outside vertical tolerance
        else
        {
            if (trail != null) trail.emitting = false;
        }
    }
}
