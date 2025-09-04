using TMPro;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIHUD : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public DronePathFollower dpf;
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
    public Slider speedSlider;

    public GameObject batteryLowUI;

    private Vector3 initialPosition;
    private float initialAltitude;
    private float currentBatteryLife;
    private float currentSprayQuantity;
    private Vector3 lastPosition;
    private float totalDistanceTraveled = 0f;

    [Header("Battery Settings")]
    public float batteryLife = 4f;
    private bool batterLow = false;

    [Header("Spray Settings")]
    public float sprayLife = 4f;
    private bool isSprayOver = false;

    [Header("Input Manager")]
    public FSJoystickInput joystickInput;


    void Start()
    {
        initialPosition = transform.position;
        initialAltitude = initialPosition.y;
        currentBatteryLife = batteryLife;
        currentSprayQuantity = sprayLife;
        lastPosition = transform.position; // Initialize last position

        if (speedSlider != null && dpf != null)
        {
            speedSlider.value = speedSlider.minValue;
            speedSlider.onValueChanged.AddListener(OnSpeedSliderChanged);
        }
    }

    void OnDestroy()
    {
        if (speedSlider != null)
        {
            speedSlider.onValueChanged.RemoveListener(OnSpeedSliderChanged);
        }
    }

    void OnSpeedSliderChanged(float value)
    {
        if (dpf != null)
        {
            dpf.SetMoveSpeed(value);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (dpf.isMoving)
        {
            currentBatteryLife -= Time.deltaTime;
            currentBatteryLife = Mathf.Max(0, currentBatteryLife);
        }
        if (wst.isSpraying)
        {
            currentSprayQuantity -= Time.deltaTime;
            currentSprayQuantity = Mathf.Max(0, currentSprayQuantity);
        }
        UpdateUIElements();
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
            fsSimulatorText.text = joystickInput != null ? joystickInput.DeviceName : "No Device";

        if (!batterLow && batteryPercentage < 40)
        {

            batteryLowUI.SetActive(true);
            batterLow = true;

        }
        if (batteryPercentage <= 30)
        {
            Debug.Log("batteey over");
            dpf.batteryOver = true;
            dpf.ForceReturnToStart();

        }

        if (sprayQuantityPercentage <= 0)
        {
            dpf.sprayOver = true;
            dpf.ForceReturnToStart();
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

    public void RestartScene()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
