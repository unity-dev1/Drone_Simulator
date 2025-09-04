using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections;
using Unity.Cinemachine; 

public class ExplodeDrone : MonoBehaviour
{
    [SerializeField] GameObject droneParts;
    [SerializeField] GameObject maindrone;
    [SerializeField] GameObject crashCamera;
    [SerializeField] GameObject crashUI;
    [SerializeField] GameObject geofenceUI;
    [SerializeField] DroneAudio da;

    private CinemachineCamera cinemachineCamera;
    private Rigidbody droneRigidbody;
    private const float VELOCITY_THRESHOLD = 0.1f;
    private const float CAMERA_HEIGHT = 5f;
    private const float CAMERA_DISTANCE = 6f;
    private const float CAMERA_SMOOTHING = 0.1f;
    private Vector3 cameraVelocity = Vector3.zero;
    private LayerMask obstacleLayer;
    private Vector3 initialCrashPosition;
    private bool isOutside;

    private void Start()
    {
        cinemachineCamera = crashCamera.GetComponent<CinemachineCamera>();
        droneRigidbody = maindrone.GetComponent<Rigidbody>();
        obstacleLayer = ~LayerMask.GetMask("Drone", "Ignore Raycast");
    }

    private Vector3 FindCameraPosition(Vector3 dronePos)
    {
        
        Vector3[] cameraOffsets = new Vector3[]
        {
            new Vector3(0, CAMERA_HEIGHT, -CAMERA_DISTANCE),  
            new Vector3(CAMERA_DISTANCE, CAMERA_HEIGHT, 0),   
            new Vector3(-CAMERA_DISTANCE, CAMERA_HEIGHT, 0),  
            new Vector3(0, CAMERA_HEIGHT, CAMERA_DISTANCE),   
            new Vector3(0, CAMERA_HEIGHT * 1.5f, 0)         
        };

        foreach (Vector3 offset in cameraOffsets)
        {
            Vector3 targetPos = dronePos + offset;
            
            bool positionIsOutside = !Physics.Linecast(targetPos, initialCrashPosition, obstacleLayer);
            if (positionIsOutside == isOutside)
            {
                if (!Physics.Linecast(targetPos, dronePos, obstacleLayer))
                {
                    return targetPos;
                }
            }
        }
        
        return dronePos + Vector3.up * (CAMERA_HEIGHT * 2f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 10 || other.gameObject.layer == 9)  
        {
            Debug.Log("crashed-" + other);
            initialCrashPosition = maindrone.transform.position;
            
            RaycastHit hit;
            isOutside = !Physics.Raycast(initialCrashPosition, Vector3.up, out hit, 100f, obstacleLayer);
            
            droneParts.transform.localRotation = Quaternion.identity;   
            da.PLayCrash();
            droneRigidbody.useGravity = true;
            droneRigidbody.mass = 100;
            droneRigidbody.linearDamping = 0;
            maindrone.GetComponent<PropellersRotate>().enabled = false;
            maindrone.GetComponent<DRONECONT>().enabled = false;

            if (cinemachineCamera != null)
            {
                cinemachineCamera.Priority = 20;
                StartCoroutine(FollowCrashingDrone());
            }
        }
    }

    IEnumerator FollowCrashingDrone()
    {
        var brain = Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>();
        yield return new WaitUntil(() => brain.IsBlending);
        yield return new WaitUntil(() => !brain.IsBlending);
        
        Vector3 lastGoodPosition = crashCamera.transform.position;
        bool hasGoodPosition = false;

        while (droneRigidbody.linearVelocity.magnitude > VELOCITY_THRESHOLD)
        {
            Vector3 dronePosition = maindrone.transform.position;
            Vector3 newCameraPosition = FindCameraPosition(dronePosition);
            
            if (newCameraPosition != dronePosition + Vector3.up * (CAMERA_HEIGHT * 2f))
            {
                lastGoodPosition = newCameraPosition;
                hasGoodPosition = true;
            }
            else if (hasGoodPosition)
            {
                newCameraPosition = lastGoodPosition;
            }
            
            crashCamera.transform.position = Vector3.SmoothDamp(
                crashCamera.transform.position,
                newCameraPosition,
                ref cameraVelocity,
                CAMERA_SMOOTHING
            );
            
            crashCamera.transform.LookAt(dronePosition);
            yield return null;
        }

        Vector3 finalDronePos = maindrone.transform.position;
        Vector3 finalCameraPos = FindCameraPosition(finalDronePos);
        
        if (hasGoodPosition && finalCameraPos == finalDronePos + Vector3.up * (CAMERA_HEIGHT * 2f))
        {
            finalCameraPos = lastGoodPosition;
        }
        
        crashCamera.transform.position = finalCameraPos;
        crashCamera.transform.LookAt(finalDronePos);
        crashUI.SetActive(true);
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}