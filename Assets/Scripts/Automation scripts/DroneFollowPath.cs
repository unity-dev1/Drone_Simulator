using UnityEngine;

public class DronePathFollower : MonoBehaviour
{
    [Header("Drone References")]
    [SerializeField] private Transform droneTransform;
    [SerializeField] private Rigidbody droneRigidbody;
    [SerializeField] private LineRenderer pathRenderer;
    [SerializeField] private LineRenderer hvlineRenderer;
    [SerializeField] private LineRenderer railwaylineRenderer;
    [SerializeField] private LineRenderer commonPathRenderer;
    [SerializeField] private WaterSprayToggle wst;
    [SerializeField] GameObject batteryoverUI;
    public GameObject sprayTankOverUI;
    public bool batteryOver = false;
    public bool sprayOver = false;

    [Header("Path Settings")]
    [SerializeField] private Transform initialCheckpoint;
    [SerializeField] private float checkpointReachThreshold = 1f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    public float MoveSpeed => moveSpeed;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float arrivalThreshold = 0.5f;
    [SerializeField] private float slowdownDistance = 2f;
    [SerializeField] private float yOffset = 0.2f; 

    [Header("Debug")]
    [SerializeField] private bool drawDebugPath = true;
    [SerializeField] private Color debugPathColor = Color.green;

    private Vector3 lastDirection;


    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3[] pathPoints;
    private int currentPointIndex = 0;
    private bool isReturningToStart = false;
    public bool isMoving = false;
    private bool hasReachedPath = false;
    private bool hasReachedCheckpoint = false;
    private bool isBattery;
    public DroneAudio da;
    public PropellersRotate pr;

    private void Awake()
    {
        if (droneTransform == null)
        {
            droneTransform = transform;
        }

        if (droneRigidbody == null)
        {
            droneRigidbody = GetComponent<Rigidbody>();
            if (droneRigidbody == null)
            {
                Debug.LogError("No Rigidbody component found on drone!");
                return;
            }
        }

        initialPosition = droneTransform.position;
        initialRotation = droneTransform.rotation;

       
    }

    private void OnEnable()
    {
        if (SelectionManager.Instance?.selectedScenario.index == 4)
        {
            commonPathRenderer.gameObject.SetActive(false);
            railwaylineRenderer.gameObject.SetActive(false);
            hvlineRenderer.gameObject.SetActive(true);
            pathRenderer = hvlineRenderer;
        }
        else if (SelectionManager.Instance ?.selectedEnvironment.index == 5) 
        {
            commonPathRenderer.gameObject.SetActive(false);
            hvlineRenderer.gameObject.SetActive(false);
            railwaylineRenderer.gameObject.SetActive(true);
            pathRenderer = railwaylineRenderer;
        }
        else
        {
            commonPathRenderer.gameObject.SetActive(true);
            hvlineRenderer.gameObject.SetActive(false);
            railwaylineRenderer.gameObject.SetActive(false);
            pathRenderer = commonPathRenderer;
        }

        if (pathRenderer != null)
        {
            UpdatePathPoints(pathRenderer);
        }
    }

    public void StartFollowingPath()
    {
        if (droneRigidbody == null)
        {
            Debug.LogError("No rigidbody assigned!");
            return;
        }

        if (pathRenderer == null || pathPoints == null || pathPoints.Length == 0)
        {
            Debug.LogError("No valid path assigned!");
            return;
        }

        currentPointIndex = 0;
        isReturningToStart = false;
        isMoving = true;
        hasReachedPath = false;
        
        hasReachedCheckpoint = SelectionManager.Instance?.selectedEnvironment.index != 2;
        
        da.PlayStartThenFlying();
        pr.StartRotation();
        if (wst.sprayEnabled)
            wst.shouldSpray = true;
        lastDirection = Vector3.zero;
    }

    public void ForceReturnToStart()
    {
        if (!isMoving) return;

        if (SelectionManager.Instance?.selectedEnvironment.index == 2)
        {
            hasReachedCheckpoint = false;
            hasReachedPath = false;
        }
        else
        {
            hasReachedPath = true;
        }
        
        isReturningToStart = true;
    }


    private void FixedUpdate()
    {
        if (!isMoving || droneRigidbody == null) return;

        Vector3 targetPosition;

        if (SelectionManager.Instance?.selectedEnvironment.index == 2)
        {
            if (!hasReachedCheckpoint && initialCheckpoint != null)
            {
                targetPosition = initialCheckpoint.position;
                float distanceToCheckpoint = Vector3.Distance(droneTransform.position, targetPosition);
                
                if (distanceToCheckpoint < 1f)
                {
                    hasReachedCheckpoint = true;
                    droneRigidbody.linearVelocity = Vector3.zero;
                    return;
                }
                MoveTowardsTarget(targetPosition);
                return;
            }
        }

        if (isReturningToStart)
        {
            if (SelectionManager.Instance?.selectedEnvironment.index == 2 && hasReachedCheckpoint)
            {
                targetPosition = initialPosition + new Vector3(0, yOffset, 0);
                float distanceToInitial = Vector3.Distance(droneTransform.position, targetPosition);
                
                if (distanceToInitial < arrivalThreshold)
                {
                    wst.hideSpray();
                    isMoving = false;
                    droneRigidbody.linearVelocity = Vector3.zero;
                    droneRigidbody.angularVelocity = Vector3.zero;
                    droneTransform.rotation = initialRotation;
                    wst.shouldSpray = false;
                    da.PLayCrash();
                    pr.StopRotation();
                    if (batteryOver)
                    {
                        batteryoverUI.SetActive(true);
                        gameObject.SetActive(false);
                    }
                    else if (sprayOver)
                    {
                        sprayTankOverUI.SetActive(true);
                        gameObject.SetActive(false);
                    }
                    return;
                }
                MoveTowardsTarget(targetPosition);
                return;
            }

            targetPosition = initialPosition + new Vector3(0, yOffset, 0);
            if (wst.shouldSpray)
            {
                wst.isSpraying = true;
                wst.ToggleSpray();
            }

            if (Vector3.Distance(droneTransform.position, targetPosition) < arrivalThreshold)
            {
                wst.hideSpray();
                isMoving = false;
                droneRigidbody.linearVelocity = Vector3.zero;
                droneRigidbody.angularVelocity = Vector3.zero;
                droneTransform.rotation = initialRotation;
                Debug.Log("Drone returned to initial position");
                wst.shouldSpray = false;
                da.PLayCrash();
                pr.StopRotation();
                if (batteryOver)
                {
                    batteryoverUI.SetActive(true);
                    gameObject.SetActive(false);
                }
                else if (sprayOver)
                {
                    sprayTankOverUI.SetActive(true);
                    gameObject.SetActive(false);
                }
            }
            MoveTowardsTarget(targetPosition);
            return;
        }

        if (!hasReachedPath)
        {
            targetPosition = pathRenderer.transform.TransformPoint(pathPoints[0]);

            if (Vector3.Distance(droneTransform.position, targetPosition) < arrivalThreshold)
            {
                hasReachedPath = true;
                if (SelectionManager.Instance?.selectedEnvironment.index == 6)
                {
                    droneRigidbody.rotation = Quaternion.Euler(0, 64.7f, 0);
                }

                if (SelectionManager.Instance?.selectedScenario.index == 4) 
                {
                    droneRigidbody.rotation = Quaternion.Euler(0, 180f, 0);
                }

                Debug.Log("Reached start of path, now following path");
                if (wst.sprayEnabled)
                {
                    wst.isSpraying = false;
                    wst.ToggleSpray();
                }
            }
            MoveTowardsTarget(targetPosition);
        }
        else
        {
            targetPosition = pathRenderer.transform.TransformPoint(pathPoints[currentPointIndex]);
            if (Vector3.Distance(droneTransform.position, targetPosition) < arrivalThreshold)
            {
                currentPointIndex++;

                if (currentPointIndex < pathPoints.Length)
                {
                    Vector3 newDirection = (pathRenderer.transform.TransformPoint(pathPoints[currentPointIndex]) - droneTransform.position).normalized;

                    if (lastDirection != Vector3.zero)
                    {
                        float angle = Vector3.Angle(lastDirection, newDirection);
                        if (angle > 10f)
                        {
                            if (wst.sprayEnabled)
                                wst.ToggleSpray();
                        }
                    }

                    lastDirection = newDirection;
                }

                if (currentPointIndex >= pathPoints.Length)
                {
                    isReturningToStart = true;
                    Debug.Log("Drone reached end of path - returning to start");
                }
            }
            MoveTowardsTarget(targetPosition);
        }
    }

    private void MoveTowardsTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - droneTransform.position).normalized;
        float distanceToTarget = Vector3.Distance(droneTransform.position, targetPosition);

        float currentSpeed = moveSpeed;
        if (distanceToTarget < slowdownDistance)
        {
            currentSpeed = Mathf.Lerp(0.5f, moveSpeed, distanceToTarget / slowdownDistance);
        }

        droneRigidbody.linearVelocity = direction * currentSpeed;

        if (direction != Vector3.zero)
        {
            Vector3 flatDirection = new Vector3(direction.x, 0, direction.z);
            if (flatDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
                if (SelectionManager.Instance?.selectedEnvironment.index == 6 || SelectionManager.Instance?.selectedScenario.index == 4) return;
                droneRigidbody.rotation = Quaternion.Slerp(droneRigidbody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }
    }

    public void SetPath(LineRenderer newPath)
    {
       pathRenderer = newPath;
       UpdatePathPoints(pathRenderer);
    }

    private void UpdatePathPoints(LineRenderer pathRenderer)
    {
        if (pathRenderer != null)
        {
            pathPoints = new Vector3[pathRenderer.positionCount];
            pathRenderer.GetPositions(pathPoints);
        }
    }

    public void SetDrone(Transform newDrone, Rigidbody newRigidbody)
    {
        droneTransform = newDrone;
        droneRigidbody = newRigidbody;

        if (droneTransform != null)
        {
            initialPosition = droneTransform.position;
            initialRotation = droneTransform.rotation;
        }
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
}
