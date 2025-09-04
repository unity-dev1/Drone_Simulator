using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class HatchPattern : MonoBehaviour
{
    [Header("Ray Settings")]
    [SerializeField] private int rayCount = 5;
    [SerializeField] private float spacing = 50f;
    [SerializeField] private float maxDistance = 1000f;
    [Range(0f, 360f)][SerializeField] private float angle = 0f; // Renamed from orbitOffset
    public float altitude = 5f; // Added altitude parameter
    public float altitudemaxValue = 50f;

    [Header("Line Renderer Settings")]
    [SerializeField] private Color lineColor = new Color(1f, 0.6f, 0f, 0.4f);
    [Range(0f, 1f)]
    [SerializeField] private float lineOpacity = 0.4f;

    [Header("References")]
    [SerializeField] private PerimeterLine perimeterLine;
    [SerializeField] private GameObject hashPrefab;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform canvasCenter;
    [SerializeField] private GameObject arrowCollection;

    [Header("UI References")]
    [SerializeField] private Slider spacingSlider;
    [SerializeField] private Slider angleSlider;
    [SerializeField] private Slider altitudeSlider;
    [SerializeField] private TextMeshProUGUI spacingText;
    [SerializeField] private TextMeshProUGUI angleText;
    [SerializeField] private TextMeshProUGUI altitudeText;

    [Header("Drone References")]
    [SerializeField] private LineRenderer droneLineRenderer;
    [SerializeField] private Transform droneParent; // Parent transform for the drone line
    //UnityEditor.TransformWorldPlacementJSON:{ "position":{ "x":-65.4000015258789,"y":-179.60000610351563,"z":173.5},"rotation":{ "x":-0.5,"y":-0.5,"z":-0.5,"w":0.5},"scale":{ "x":2.0,"y":2.0,"z":2.0} }
    public Vector3 dronePos = new Vector3(-110, 189, 42.3f);
    public Vector3 droneRot = new Vector3(90, 0, -90);
    public Vector3 droneScale = new Vector3(2.1f, 2.1f, 2.1f);

    [Header("Scene Objects")]
    public GameObject drone;
    [SerializeField] private GameObject backgroundCanvas;
    [SerializeField] private GameObject automationCanvas;
    [SerializeField] private GameObject ingameCanvas;
    [SerializeField] private GameObject currentCamera; // Cinemachine camera
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject frontCamera;

    public GameObject CamerasCollection;
    public GameObject ground;
    public GameObject hud;

    public LineRenderer mainLineRenderer;
    private List<GameObject> activeHashes = new List<GameObject>();
    private List<GameObject> activeArrows = new List<GameObject>();

    // Previous values to detect changes
    private float previousSpacing;
    private float previousAngle;
    private float previousAltitude;

    // Cache the material to avoid recreation
    private Material lineMaterial;

    private void Awake()
    {
        Debug.Log("[HatchPattern] Awake called");
        
        // Ensure LineRenderer exists
        mainLineRenderer = GetComponent<LineRenderer>();
        if (mainLineRenderer == null)
        {
            Debug.LogWarning("[HatchPattern] No LineRenderer found, adding one");
            mainLineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Assign material in Inspector for reliability
        if (lineMaterial == null)
        {
            Debug.LogWarning("[HatchPattern] lineMaterial not assigned in Inspector. Please assign a Material with Unlit/Transparent or Sprites/Default shader for orange transparency.");
        }

        // Validate UI references (must be assigned in Inspector)
        if (spacingSlider == null) Debug.LogError("[HatchPattern] spacingSlider is not assigned in Inspector!");
        if (angleSlider == null) Debug.LogError("[HatchPattern] angleSlider is not assigned in Inspector!");
        if (altitudeSlider == null) Debug.LogError("[HatchPattern] altitudeSlider is not assigned in Inspector!");
        if (spacingText == null) Debug.LogError("[HatchPattern] spacingText is not assigned in Inspector!");
        if (angleText == null) Debug.LogError("[HatchPattern] angleText is not assigned in Inspector!");
        if (altitudeText == null) Debug.LogError("[HatchPattern] altitudeText is not assigned in Inspector!");
    }

    private void Start()
    {
        Debug.Log("[HatchPattern] Start called");
        // Set up UI listeners only if components exist
        SetupUIListeners();

        // Store initial values
        previousSpacing = spacing;
        previousAngle = angle;
        previousAltitude = altitude;

        // Initialize line renderer
        if (droneLineRenderer != null)
        {
            droneLineRenderer.gameObject.SetActive(false);
        }

        // Generate initial pattern only if required components exist
        if (perimeterLine != null && canvasCenter != null)
        {
            UpdateOrbitPosition();
            GeneratePattern();
        }
        else
        {
            Debug.LogError("[HatchPattern] Missing required components for pattern generation");
        }
    }

    private void SetupUIListeners()
    {
        if (spacingSlider != null)
        {
            spacingSlider.minValue = 25f;
            spacingSlider.maxValue = 400f;
            spacingSlider.value = spacing;
            spacingSlider.onValueChanged.AddListener(OnSpacingChanged);
            UpdateSpacingText();

           
        }
        if (angleSlider != null)
        {
            angleSlider.minValue = 0f;
            angleSlider.maxValue = 360f;
            angleSlider.value = angle;
            angleSlider.onValueChanged.AddListener(OnAngleChanged);
            UpdateAngleText();
        }
        if (altitudeSlider != null)
        {
            altitudeSlider.minValue = altitude;
            altitudeSlider.maxValue = altitudemaxValue;
            altitudeSlider.value = altitude;
            altitudeSlider.onValueChanged.AddListener(OnAltitudeChanged);
            UpdateAltitudeText();
        }

        EnvironmentOptionSO selectedEnvironment = SelectionManager.Instance?.selectedEnvironment;

        if (selectedEnvironment.index == 3 || selectedEnvironment.index == 6) 
        {
            spacingSlider.value = 35;
            spacingSlider.interactable = false;
            angleSlider.interactable = false;
            altitudeSlider.interactable = false;
        }
    }

    private void OnSpacingChanged(float newValue)
    {
        spacing = newValue;
        UpdateSpacingText();
        GeneratePattern();
    }

    private void OnAngleChanged(float newValue)
    {
        angle = newValue;
        UpdateAngleText();
        UpdateOrbitPosition();
        GeneratePattern();
    }

    private void OnAltitudeChanged(float newValue)
    {
        altitude = newValue;
        UpdateAltitudeText();
        // For now, altitude doesn't affect the pattern but you can implement this later
    }

    private void UpdateSpacingText()
    {
        if (spacingText != null)
        {
            // Map slider value from 25-400 to display value 5-120
            float displayValue = Mathf.Lerp(5f, 120f, (spacing - 25f) / 375f);
            spacingText.text = displayValue.ToString("F1");
        }
    }

    private void UpdateAngleText()
    {
        if (angleText != null)
        {
            angleText.text = angle.ToString("F1") + "°";
        }
    }

    private void UpdateAltitudeText()
    {
        if (altitudeText != null)
        {
            float remappedValue = Mathf.Round(RemapValue(altitude, altitudeSlider.minValue, altitudeSlider.maxValue, 1, 
                altitudeSlider.maxValue));
            altitudeText.text = remappedValue.ToString();
        }
    }

    public float RemapValue(float value, float oldMin, float oldMax, float newMin = 1f, float newMax= 10f)
    {
        return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
    }


    private void UpdateOrbitPosition()
    {
        if (canvasCenter == null) return;

        float radians = angle * Mathf.Deg2Rad;
        float x = canvasCenter.position.x + Mathf.Cos(radians) * 100f;
        float y = canvasCenter.position.y + Mathf.Sin(radians) * 100f;

        transform.position = new Vector3(x, y, canvasCenter.position.z);
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    public void GeneratePattern()
    {
        ClearOldObjects();

        float totalWidth = (rayCount - 1) * spacing;
        float startX = -totalWidth / 2f;

        List<EdgeCollider2D> edgeColliders = perimeterLine.GetEdgeColliders();
        List<Vector3> linePoints = new List<Vector3>();
        List<Vector3> allFirstIntersections = new List<Vector3>();
        List<Vector3> allLastIntersections = new List<Vector3>();

        // First collect all intersection points
        for (int i = 0; i < rayCount; i++)
        {
            float xPos = startX + i * spacing;
            Vector3 rayStartWorld = transform.TransformPoint(new Vector3(xPos, 0f, 0f));
            Vector3 rayDirWorld = -transform.up;

            List<Vector3> intersections = GetSortedIntersections(rayStartWorld, rayDirWorld, edgeColliders);

            if (intersections.Count >= 2)
            {
                allFirstIntersections.Add(intersections[0]);
                allLastIntersections.Add(intersections[intersections.Count - 1]);
            }
        }

        // Create zig-zag pattern
        if (allFirstIntersections.Count > 0)
        {
            // Alternate directions for rays
            bool goToFirst = true; // Next point should be first intersection
            for (int i = 0; i < allFirstIntersections.Count; i++)
            {
                if (goToFirst)
                {
                    linePoints.Add(allFirstIntersections[i]);
                    linePoints.Add(allLastIntersections[i]);
                }
                else
                {
                    linePoints.Add(allLastIntersections[i]);
                    linePoints.Add(allFirstIntersections[i]);
                }

                AddArrow(linePoints);
                goToFirst = !goToFirst;
            }

            // Set up main line renderer
            mainLineRenderer.positionCount = linePoints.Count;
            mainLineRenderer.SetPositions(linePoints.ToArray());

            // Place hash marks
            GameObject firstHash = Instantiate(hashPrefab, allFirstIntersections[0], Quaternion.identity, transform);
            GameObject lastHash = null;

            if (!goToFirst)
            {
                lastHash = Instantiate(hashPrefab, allLastIntersections[allLastIntersections.Count - 1], Quaternion.identity, transform);
            }
            else
            {
                lastHash = Instantiate(hashPrefab, allFirstIntersections[allFirstIntersections.Count - 1], Quaternion.identity, transform);
            }

            activeHashes.Add(firstHash);
            activeHashes.Add(lastHash);
        }
    }

    private void AddArrow(List<Vector3> points)
    {
        if (points.Count >= 2)
        {
            Vector3 midpoint = (points[points.Count - 2] + points[points.Count - 1]) / 2f;
            GameObject arrow = Instantiate(arrowPrefab, midpoint, Quaternion.identity, arrowCollection.transform);

            Vector3 direction = (points[points.Count - 1] - points[points.Count - 2]).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.Euler(0, 0, angle);
            activeArrows.Add(arrow);
        }
    }

    private List<Vector3> GetSortedIntersections(Vector3 origin, Vector3 direction, List<EdgeCollider2D> colliders)
    {
        List<Vector3> intersections = new List<Vector3>();

        foreach (EdgeCollider2D collider in colliders)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, maxDistance);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == collider)
                {
                    intersections.Add(new Vector3(hit.point.x, hit.point.y, 89.9f));
                }
            }
        }

        intersections.Sort((a, b) => Vector3.Distance(a, origin).CompareTo(Vector3.Distance(b, origin)));
        return intersections;
    }

    private void ClearOldObjects()
    {
        mainLineRenderer.positionCount = 0;

        foreach (GameObject hash in activeHashes) Destroy(hash);
        foreach (GameObject arrow in activeArrows) Destroy(arrow);

        activeHashes.Clear();
        activeArrows.Clear();
    }

    private void OnValidate()
    {
        // Update UI to match inspector values if running in play mode
        if (Application.isPlaying)
        {
            if (spacingSlider != null) spacingSlider.value = spacing;
            if (angleSlider != null) angleSlider.value = angle;
            if (altitudeSlider != null) altitudeSlider.value = altitude;
            // Update material color if it exists
            if (lineMaterial != null)
            {
                Color newColor = lineColor;
                newColor.a = lineOpacity;
                lineMaterial.color = newColor;
            }
        }
    }

    public void DoneAutomation()
    {
        Debug.Log("[HatchPattern] DoneAutomation called");
        if (droneLineRenderer == null)
        {
            Debug.LogError("[HatchPattern] Drone Line Renderer is not assigned!");
            return;
        }

        // Get points from main line renderer
        Vector3[] worldPoints = new Vector3[mainLineRenderer.positionCount];
        mainLineRenderer.GetPositions(worldPoints);

        if (worldPoints.Length == 0)
        {
            Debug.LogError("[HatchPattern] No points to copy to drone line renderer");
            return;
        }

        Debug.Log($"[HatchPattern] Copying {worldPoints.Length} points to drone line renderer");

        // Set up drone line renderer
        droneLineRenderer.positionCount = worldPoints.Length;
        
        // Create a new material instance if needed
        if (lineMaterial != null)
        {
            Material newMaterial = new Material(lineMaterial);
            newMaterial.color = new Color(1f, 0.5f, 0f, 0.6f); // Orange, 60% transparent
            droneLineRenderer.material = newMaterial;
        }
        else
        {
            // If no material is assigned, create a default one
            Material defaultMaterial = new Material(Shader.Find("Sprites/Default"));
            defaultMaterial.color = new Color(1f, 0.5f, 0f, 0.6f);
            droneLineRenderer.material = defaultMaterial;
        }

        droneLineRenderer.startWidth = 0.5f;
        droneLineRenderer.endWidth = 0.5f;
        droneLineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        droneLineRenderer.receiveShadows = false;
        droneLineRenderer.allowOcclusionWhenDynamic = false;

        // Convert world points to local space of drone parent - optimize by pre-allocating array
        Vector3[] localPoints = new Vector3[worldPoints.Length];
        if (droneParent != null)
        {
            for (int i = 0; i < worldPoints.Length; i++)
            {
                localPoints[i] = droneParent.InverseTransformPoint(worldPoints[i]);
            }
            droneLineRenderer.SetPositions(localPoints);
        }
        else
        {
            Debug.LogWarning("[HatchPattern] droneParent is null, using world points");
            droneLineRenderer.SetPositions(worldPoints);
        }

        //Set Drone linerenderer position
        droneLineRenderer.transform.position = dronePos;

        if (SelectionManager.Instance?.selectedEnvironment.index == 3)
        {
            droneRot = new Vector3(90, 0, -270);
            droneLineRenderer.transform.rotation = Quaternion.Euler(droneRot);
        }

        if (SelectionManager.Instance?.selectedEnvironment.index == 6)
        {
            droneRot = new Vector3(0, 67.8150024f, -180);
            droneLineRenderer.transform.rotation = Quaternion.Euler(droneRot);
        }

        droneLineRenderer.transform.rotation = Quaternion.Euler(droneRot);
        droneLineRenderer.transform.localScale = droneScale;

        // Adjust altitude by moving the entire GameObject
        if (droneParent != null)
        {
            Vector3 newPos = droneParent.position;
            newPos.y += altitude;
            droneParent.position = newPos;
        }
        else
        {
            Vector3 newPos = droneLineRenderer.transform.position;
            newPos.y += altitude;
            droneLineRenderer.transform.position = newPos;
        }

        // Make sure the LineRenderer is enabled and visible
        droneLineRenderer.enabled = true;
        droneLineRenderer.gameObject.SetActive(true);

        // Handle scene transition
        StartCoroutine(CompleteAutomationSequence());
    }

    private IEnumerator CompleteAutomationSequence()
    {
        Debug.Log("[HatchPattern] Starting CompleteAutomationSequence");
        // Wait one frame to ensure everything is processed
        yield return new WaitForSeconds(0.2f);

        if (CamerasCollection != null)
        {
            Debug.Log("[HatchPattern] Activating CamerasCollection");
            CamerasCollection.SetActive(true);
        }
        else
        {
            Debug.LogError("[HatchPattern] CamerasCollection is null!");
        }

        if (frontCamera != null)
        {
            Debug.Log("[HatchPattern] Activating frontCamera");
            frontCamera.SetActive(true);
        }
        else
        {
            Debug.LogError("[HatchPattern] frontCamera is null!");
        }

        if (hud != null)
        {
            Debug.Log("[HatchPattern] Activating hud");
            hud.SetActive(true);
        }
        else
        {
            Debug.LogError("[HatchPattern] hud is null!");
        }

        // Disable UI elements
        if (backgroundCanvas != null)
        {
            Debug.Log("[HatchPattern] Disabling backgroundCanvas");
            backgroundCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("[HatchPattern] backgroundCanvas is null!");
        }

        if (automationCanvas != null)
        {
            Debug.Log("[HatchPattern] Disabling automationCanvas");
            automationCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("[HatchPattern] automationCanvas is null!");
        }

        if (ingameCanvas != null)
        {
            Debug.Log("[HatchPattern] Activating ingameCanvas");
            ingameCanvas.SetActive(true);
        }
        else
        {
            Debug.LogError("[HatchPattern] ingameCanvas is null!");
        }

        // Enable ground and player camera
        if (ground != null)
        {
            Debug.Log("[HatchPattern] Activating ground");
            ground.SetActive(true);
        }
        else
        {
            Debug.LogError("[HatchPattern] ground is null!");
        }

        if (playerCamera != null)
        {
            Debug.Log("[HatchPattern] Activating playerCamera");
            playerCamera.SetActive(true);
        }
        else
        {
            Debug.LogError("[HatchPattern] playerCamera is null!");
        }

        if (drone != null)
        {
            Debug.Log("[HatchPattern] Activating drone");
            drone.SetActive(true);
        }
        else
        {
            Debug.LogError("[HatchPattern] drone is null!");
        }

        // Disable automation camera
        if (currentCamera != null)
        {
            Debug.Log("[HatchPattern] Disabling currentCamera");
            currentCamera.SetActive(false);
        }
        else
        {
            Debug.LogError("[HatchPattern] currentCamera is null!");
        }

        Debug.Log("[HatchPattern] Automation sequence completed - switched to player view");
    }

}