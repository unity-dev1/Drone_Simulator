using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class ShapeDrawer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private List<Vector3> points = new List<Vector3>();
    private bool isDrawing = false;
    private GameObject endSphere;

    public DRONECONT dc;

    public float pointDistanceThreshold = 0.1f;
    public float offset = 0.2f;
    public GameObject spherePrefab;

    public FSJoystickInput joystickInput;
    public InputActionAsset di;

    [Header("Camera Settings")]
    public Camera mainCamera; // Your primary gameplay camera
    public Camera screenshotCamera; // Secondary camera for captures
    public RenderTexture renderTexture;
    public string fileName = "ShapeCapture";
    public int imageWidth = 1024;
    public int imageHeight = 1024;
    private bool exit = false;

    [Header("Display Settings")]
    public GameObject screenshotCanvas;
    public RawImage screenshotImage;

    [Header("Drawing UI")]
    public GameObject ui;
    public Sprite startdrawsprite;
    public Sprite enddrawsprite;

    void Start()
    {
        di.Enable();
        ui.SetActive(true);
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
    }

    // Add this variable at the class level alongside the previous one
    private bool previousDrawButtonState = false;

    void Update()
    {
        if (!dc.startupDone) return;  //only when start

        // Handle keyboard input for drawing
        if (di.FindAction("draw shape").triggered)
        {
            ToggleDrawing();
        }

        // Handle joystick input for drawing - toggle on state changes
        if (joystickInput != null)
        {
            // If the drawing button state changed (either direction), toggle drawing
            if (joystickInput.DrawingButtonPressed != previousDrawButtonState)
            {
                ToggleDrawing();
            }

            // Update previous draw button state for next frame
            previousDrawButtonState = joystickInput.DrawingButtonPressed;
        }

        if (isDrawing)
        {
            UpdateDrawing();
        }
    }

    public void ToggleDrawing()
    {
        if (isDrawing)
        {
            ui.GetComponent<Image>().sprite = startdrawsprite;
            CancelDrawing();
        }
        else
        {
            ui.GetComponent<Image>().sprite = enddrawsprite;
            StartDrawing();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDrawing && other.gameObject == endSphere && exit == true)
        {
            StopDrawing();
            ui.GetComponent<Image>().sprite = startdrawsprite;
            exit = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isDrawing && other.gameObject == endSphere)
        {
            exit = true;
        }
    }

    void StartDrawing()
    {
        isDrawing = true;
        points.Clear();
        points.Add(transform.position);
        UpdateLineRenderer();

        Vector3 spherePos = transform.position - transform.forward * offset;
        endSphere = Instantiate(spherePrefab, spherePos, Quaternion.identity);
        endSphere.GetComponent<SphereCollider>().isTrigger = true;
    }

    void UpdateDrawing()
    {
        Vector3 currentPos = transform.position;
        if (Vector3.Distance(points[points.Count - 1], currentPos) > pointDistanceThreshold)
        {
            points.Add(currentPos);
            UpdateLineRenderer();
        }
    }

    void StopDrawing()
    {
        isDrawing = false;

        if (points.Count > 2 && Vector3.Distance(points[0], points[points.Count - 1]) > pointDistanceThreshold)
        {
            points.Add(points[0]);
        }

        RemapToXZPlane();
        MoveToWorldPosition(new Vector3(100, -100, 100));
        UpdateLineRenderer();

        PositionCameraAboveShape();
        CaptureScreenshot();

        if (endSphere != null) Destroy(endSphere);
    }

    void CancelDrawing()
    {
        isDrawing = false;
        points.Clear();
        UpdateLineRenderer();
        exit = false;

        if (endSphere != null)
        {
            Destroy(endSphere);
            endSphere = null;
        }
    }

    void PositionCameraAboveShape()
    {
        if (screenshotCamera == null) return;

        Bounds bounds = new Bounds(points[0], Vector3.zero);
        foreach (Vector3 point in points) bounds.Encapsulate(point);

        float padding = 1.1f;
        float maxDimension = Mathf.Max(bounds.size.x, bounds.size.z) * padding;

        screenshotCamera.orthographic = true;
        screenshotCamera.orthographicSize = maxDimension / 2;
        screenshotCamera.transform.position = bounds.center + Vector3.up * 10;
        screenshotCamera.transform.rotation = Quaternion.LookRotation(Vector3.down);
        screenshotCamera.depth = mainCamera.depth - 1;
    }

    void CaptureScreenshot()
    {
        if (screenshotCamera == null || mainCamera == null) return;

        bool mainCamEnabled = mainCamera.enabled;
        bool screenshotCamEnabled = screenshotCamera.enabled;

        try
        {
            mainCamera.enabled = false;
            screenshotCamera.enabled = true;

            RenderTexture rt = new RenderTexture(imageWidth, imageHeight, 24);
            screenshotCamera.targetTexture = rt;

            screenshotCamera.Render();

            Texture2D tex = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
            tex.Apply();
            screenshotImage.texture = tex;

            byte[] bytes = tex.EncodeToPNG();
            string path = Path.Combine(Application.persistentDataPath, fileName + ".png");
            File.WriteAllBytes(path, bytes);
            Debug.Log($"Saved screenshot to: {path}");

            mainCamera.enabled = mainCamEnabled;
            screenshotCamera.enabled = screenshotCamEnabled;

            screenshotCamera.targetTexture = null;
            RenderTexture.active = null;
            if (rt != null) Destroy(rt);
        }
        finally
        {
            Debug.Log("ok");
            StartCoroutine(DisplayscreenshotCanvas(0.5f));
        }
    }

    private IEnumerator DisplayscreenshotCanvas(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
        //GetComponent<DRONECONT>().enabled = false;
        screenshotCanvas.SetActive(true);
    }

    void RemapToXZPlane()
    {
        if (points.Count < 2) return;

        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        foreach (Vector3 pt in points)
        {
            min.x = Mathf.Min(min.x, pt.x);
            min.y = Mathf.Min(min.y, pt.y);
            min.z = Mathf.Min(min.z, pt.z);
            max.x = Mathf.Max(max.x, pt.x);
            max.y = Mathf.Max(max.y, pt.y);
            max.z = Mathf.Max(max.z, pt.z);
        }

        float[] ranges = {
            max.x - min.x,
            max.y - min.y,
            max.z - min.z
        };

        int leastVariedAxis = 0;
        for (int i = 1; i < 3; i++)
            if (ranges[i] < ranges[leastVariedAxis])
                leastVariedAxis = i;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 pt = points[i];
            points[i] = leastVariedAxis switch
            {
                0 => new Vector3(pt.y, 0, pt.z),
                1 => new Vector3(pt.x, 0, pt.z),
                _ => new Vector3(pt.x, 0, pt.y)
            };
        }
    }

    void MoveToWorldPosition(Vector3 targetPosition)
    {
        Vector3 center = CalculateCenter();
        Vector3 offset = targetPosition - center;

        for (int i = 0; i < points.Count; i++)
        {
            points[i] += offset;
        }
    }

    Vector3 CalculateCenter()
    {
        Vector3 total = Vector3.zero;
        foreach (Vector3 point in points) total += point;
        return total / points.Count;
    }

    void UpdateLineRenderer()
    {
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
        lineRenderer.loop = !isDrawing && points.Count > 2;
    }
}
