using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Mathematics;

[RequireComponent(typeof(Rigidbody))]
public class DRONECONT : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public Transform droneBody;
    public PropellersRotate pr;
    public DroneAudio da;
    public InputActionAsset di;
    public ReturnToLaunch rtl;
    public WaterSprayToggle wst;

    [Header("UI Elements")]
    public TextMeshProUGUI altitudeText;
    public TextMeshProUGUI velocityText;
    public TextMeshProUGUI directionText;
    public TextMeshProUGUI batteryText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI fsSimulatorText;
    public TextMeshProUGUI distanceTraveledText; // New UI element for total distance traveled
    public TextMeshProUGUI sprayQuantityText;
    public GameObject batteryLowUI;
    public GameObject batteryOverUI;
    public GameObject sprayTankOverUI;
    public UnityEngine.UI.Slider speedSlider; // New slider for speed control

    [Header("Input UI Logs")]
    public Transform wsadlog;
    public Transform arrowlog;

    [Header("Movement Settings")]
    public float baseThrust = 10f;
    public float baseMoveForce = 5f;
    public float yawTorque = 50f;
    public float maxTiltAngle = 10f;
    public float tiltSmooth = 5f;
    private float maxSpeedLimit = 5f; // Current maximum speed limit, starting at minimum

    [Header("Battery Settings")]
    public float batteryLife = 4f;
    private bool batterLow = false;

    [Header("Spray Settings")]
    public float sprayLife = 4f;
    private bool isSprayOver = false;



    [Header("Input Manager")]
    public FSJoystickInput joystickInput;
    public PCReceiver pcReceiver; // Reference to PCReceiver

    public bool inGround = true;

    public bool isControl = true;
    public bool isreturntoSpawn = false;

    private Vector3 initialPosition;
    private float initialAltitude;
    private float currentBatteryLife;
    private float currentSprayQuantity;
    private Vector3 lastPosition;
    private float totalDistanceTraveled = 0f;

    // Keyboard input values
    private float verticalKeyboard = 0f;
    private float yawKeyboard = 0f;
    private float arrowKeyboardX = 0f;
    private float arrowKeyboardZ = 0f;

    // Final input values
    private float finalVertical = 0f;
    private float finalYaw = 0f;
    private float finalHorizontalX = 0f;
    private float finalHorizontalZ = 0f;

    private bool pressedArm = false;
    public bool startupDone = false;

    public GameObject mountainObjects;

    //[Header("Input Debugger")]
    //public TextMeshProUGUI throttleup;
    //public TextMeshProUGUI throttledown;
    //public TextMeshProUGUI yawleft;
    //public TextMeshProUGUI yawright;
    //public TextMeshProUGUI pitchforward;
    //public TextMeshProUGUI pitchbackward;
    //public TextMeshProUGUI rollleft;
    //public TextMeshProUGUI rollright;
    //public TextMeshProUGUI Spray;
    //public TextMeshProUGUI Draw;


    void Start()
    {
        di.Enable();

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        initialPosition = transform.position;
        initialAltitude = initialPosition.y;
        currentBatteryLife = batteryLife;
        currentSprayQuantity = sprayLife;
        lastPosition = transform.position;

        if (joystickInput != null)
            joystickInput.Initialize();

        // Initialize speed slider if it exists
        if (speedSlider != null)
        {
            speedSlider.value = speedSlider.minValue;
            maxSpeedLimit = speedSlider.minValue;
            speedSlider.onValueChanged.AddListener(OnSpeedSliderChanged);
        }
    }

    void OnDestroy()
    {
        if (joystickInput != null)
            joystickInput.Cleanup();
    }

    void Update()
    {
        // Calculate distance traveled since last frame
        float frameDistance = Vector3.Distance(transform.position, lastPosition);
        totalDistanceTraveled += frameDistance;
        lastPosition = transform.position; // Update last position

        if (startupDone)
        {
            currentBatteryLife -= Time.deltaTime;
            currentBatteryLife = Mathf.Max(0, currentBatteryLife);
        }
        if (wst.isSpraying)
        {
            currentSprayQuantity -= Time.deltaTime;
            currentSprayQuantity = Mathf.Max(0, currentSprayQuantity);
        }

        if (isreturntoSpawn)
        {
            if (inGround)
            {
                da.PLayCrash();
                pr.StopRotation();
                droneBody.localRotation = Quaternion.identity;
                isreturntoSpawn = false;
            }
        }


        if (joystickInput != null)
            joystickInput.ReadInput();

        UpdateInput();
        UpdateUIElements();
        UpdateUILogs();
    }

    void FixedUpdate()
    {
        if (!startupDone) return;

        // Calculate current velocity magnitude
        float currentSpeed = rb.linearVelocity.magnitude;

        // Calculate how much we need to scale forces to match target speed
        float speedRatio = currentSpeed / maxSpeedLimit;
        float forceMultiplier = speedRatio >= 1f ? 0f : 1f - (speedRatio * 0.8f); // Reduced speed dampening

        // Base force multiplier to help reach target speed (increased significantly)
        float baseForceMultiplier = (maxSpeedLimit / 5f) * 30f;

        // Combined multiplier
        float finalMultiplier = forceMultiplier * baseForceMultiplier;

        // 1) Vertical lift
        rb.AddForce(Vector3.up * finalVertical * baseThrust * finalMultiplier,
                    ForceMode.Acceleration);

        // 2) Yaw control remains the same
        rb.angularVelocity = new Vector3(
            0f,
            finalYaw * yawTorque,
            0f
        );

        // 3) Horizontal movement
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 flatRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
        Vector3 horizontalForce =
            (flatForward * finalHorizontalZ + flatRight * finalHorizontalX)
            * baseMoveForce * finalMultiplier;
        if (!inGround)
            rb.AddForce(horizontalForce, ForceMode.Acceleration);

        // If we're over the speed limit, apply braking force
        if (speedRatio > 1f)
        {
            Vector3 velocityDirection = rb.linearVelocity.normalized;
            float brakingForce = (currentSpeed - maxSpeedLimit) * 20f; // Increased braking force
            rb.AddForce(-velocityDirection * brakingForce, ForceMode.Acceleration);
        }

        // 4) Visual tilt remains the same
        if (droneBody != null)
        {
            Quaternion targetRot;
            if (!inGround)
            {
                float targetTiltX = finalHorizontalZ * maxTiltAngle;
                float targetTiltZ = -finalHorizontalX * maxTiltAngle;
                targetRot = Quaternion.Euler(targetTiltX, 0f, targetTiltZ);
            }
            else
            {
                targetRot = Quaternion.identity;
            }
            droneBody.localRotation = Quaternion.Slerp(
                droneBody.localRotation,
                targetRot,
                Time.fixedDeltaTime * tiltSmooth
            );
        }

    }

    void OnSpeedSliderChanged(float value)
    {
        maxSpeedLimit = value;
    }

    void UpdateInput()
    {
        verticalKeyboard = di.FindAction("throttle").ReadValue<float>();
        yawKeyboard = -di.FindAction("yaw").ReadValue<float>();
        arrowKeyboardX = -di.FindAction("roll").ReadValue<float>();
        arrowKeyboardZ = di.FindAction("pitch").ReadValue<float>();

        if (joystickInput != null && joystickInput.UseJoystick)
        {
            finalVertical = Mathf.Abs(joystickInput.Throttle) >= Mathf.Abs(verticalKeyboard) ? joystickInput.Throttle : verticalKeyboard;
            finalYaw = Mathf.Abs(joystickInput.Pedals) >= Mathf.Abs(yawKeyboard) ? joystickInput.Pedals : yawKeyboard;
            finalHorizontalX = Mathf.Abs(joystickInput.Cyclic.x) >= Mathf.Abs(arrowKeyboardX) ? joystickInput.Cyclic.x : arrowKeyboardX;
            finalHorizontalZ = Mathf.Abs(joystickInput.Cyclic.y) >= Mathf.Abs(arrowKeyboardZ) ? -joystickInput.Cyclic.y : arrowKeyboardZ;
        }
        else if (pcReceiver != null && pcReceiver.UseAndroidInput)
        {
            // Use Android input values directly
            finalVertical = pcReceiver.Throttle;
            finalYaw = pcReceiver.Yaw;
            finalHorizontalX = pcReceiver.Roll;
            finalHorizontalZ = pcReceiver.Pitch;
        }
        else
        {
            finalVertical = verticalKeyboard;
            finalYaw = yawKeyboard;
            finalHorizontalX = arrowKeyboardX;
            finalHorizontalZ = arrowKeyboardZ;
        }
        //Debug.Log(finalHorizontalX +""+ finalHorizontalZ+""+ finalVertical+ ""+finalYaw);

        //   Disarm/Stop    joystick pos --/\
        if (startupDone && !pressedArm && inGround && finalHorizontalX > 0f && finalHorizontalZ < 0f && finalVertical < 0f && finalYaw < 0f)
        {
            Debug.Log("Disarmed");
            pr.StopRotation();
            droneBody.localRotation = Quaternion.identity;
            startupDone = false;
            if (wst.sprayEnabled)
            {
                wst.isSpraying = true;
                wst.ToggleSpray();
                wst.shouldSpray = false;
            }


            da.PLayCrash();
        }

        //   Arm/Start   joystick pos --\/
        if (pressedArm && finalHorizontalX == 0f && finalHorizontalZ == 0f && finalVertical == 0f && finalYaw == 0f)
        {
            startupDone = true;
            pressedArm = false;
            Debug.Log("started");
        }
        if (!startupDone && !pressedArm && finalHorizontalX < 0f && finalHorizontalZ < 0f && finalVertical < 0f && finalYaw > 0f)
        {
            if (wst.sprayEnabled)
                wst.shouldSpray = true;
            Debug.Log("pressed");
            pr.StartRotation();
            pressedArm = true;
            da.PlayStartThenFlying();
        }
    }

    void UpdateUILogs()
    {
        if (wsadlog != null)
        {
            Vector3 pos = new Vector3(finalYaw * 150f, finalVertical * 150f, wsadlog.localPosition.z);
            if (pos.magnitude > 150f) pos = pos.normalized * 150f;
            wsadlog.localPosition = pos;
        }

        if (arrowlog != null)
        {
            Vector3 pos = new Vector3(finalHorizontalX * 150f, finalHorizontalZ * 150f, arrowlog.localPosition.z);
            if (pos.magnitude > 150f) pos = pos.normalized * 150f;
            arrowlog.localPosition = pos;
        }
    }

    void UpdateUIElements()
    {
        float currentAltitude = transform.position.y - initialAltitude;
        float velocityKmh = rb.linearVelocity.magnitude;   //add 3.6 for km/hr
        float batteryPercentage = (currentBatteryLife / batteryLife) * 100f;
        float sprayQuantityPercentage = (currentSprayQuantity / sprayLife) * 100f;
        float horizontalDistance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                                  new Vector3(initialPosition.x, 0, initialPosition.z));

        bool useMetric = PlayerPrefs.GetInt("MetricSystem", 0) == 0;


        if (altitudeText != null)
        {
            float displayAltitude = math.abs(useMetric ? currentAltitude : currentAltitude * 3.281f);
            string unit = useMetric ? "m" : "ft";
            altitudeText.text = $"{displayAltitude:F1} {unit}";
        }

        if (velocityText != null)
        {
            float displayVelocity = useMetric ? velocityKmh : velocityKmh * 2.237f;
            string unit = useMetric ? "m/s" : "mph";
            velocityText.text = $"{displayVelocity:F1} {unit}";
        }

        if (distanceText != null)
        {
            float displayDistance = useMetric ? horizontalDistance : horizontalDistance * 3.281f;
            string unit = useMetric ? "m" : "ft";
            distanceText.text = $"{displayDistance:F1} {unit}";
        }

        if (distanceTraveledText != null)
        {
            float displayTravel = useMetric ? totalDistanceTraveled : totalDistanceTraveled * 3.281f;
            string unit = useMetric ? "m" : "ft";
            distanceTraveledText.text = $"{displayTravel:F1} {unit}";
        }

        if (directionText != null)
            directionText.text = GetCardinalDirection();
        if (batteryText != null)
            batteryText.text = $"{batteryPercentage:F0}%";
        if (sprayQuantityText != null)
            sprayQuantityText.text = $"{sprayQuantityPercentage:F0}%";
        if (fsSimulatorText != null)
            fsSimulatorText.text = joystickInput.DeviceName;


        if (!batterLow && batteryPercentage < 40)
        {

            batteryLowUI.SetActive(true);
            batterLow = true;

        }
        if (batteryPercentage <= 30)
        {
            rtl.ReturnToLaunchPosition();
            startupDone = false;
            Debug.Log(rtl.isReturning);
            if (inGround)
            {
                da.PLayCrash();
                pr.StopRotation();
                droneBody.localRotation = Quaternion.identity;
                gameObject.SetActive(false);
                batteryOverUI.SetActive(true);
            }
        }
        if (wst.sprayEnabled && sprayQuantityPercentage <= 0)
        {
            if (!isSprayOver)
            {
                wst.ToggleSpray();
                isSprayOver = true;
            }
            wst.enabled = false;
            rtl.ReturnToLaunchPosition();
            startupDone = false;
            Debug.Log(rtl.isReturning);
            if (inGround)
            {
                da.PLayCrash();
                pr.StopRotation();
                droneBody.localRotation = Quaternion.identity;
                gameObject.SetActive(false);
                sprayTankOverUI.SetActive(true);
            }
        }
    }

    string GetCardinalDirection()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();
        float angle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg - 90; //-90 for the drone rotation matching the direction
        if (angle < 0) angle += 360;

        if (angle >= 337.5f || angle < 22.5f) return "N";
        if (angle >= 22.5f && angle < 67.5f) return "NE";
        if (angle >= 67.5f && angle < 112.5f) return "E";
        if (angle >= 112.5f && angle < 157.5f) return "SE";
        if (angle >= 157.5f && angle < 202.5f) return "S";
        if (angle >= 202.5f && angle < 247.5f) return "SW";
        if (angle >= 247.5f && angle < 292.5f) return "W";
        return "NW";
    }

    public void ReturntoSpawnPos()
    {
        rtl.ReturnToLaunchPosition();
        isreturntoSpawn = true;
        startupDone = false;
        
    }
}
