using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

public class PCReceiver : MonoBehaviour
{
    private Process adbProcess;
    private Thread logcatThread;
    private volatile bool running = true;
    public Vector2 leftJoystick;
    public Vector2 rightJoystick;
    public Vector2 staticCamPosition;

    [Header("Deadzone Settings")]
    [Range(0f, 45f)]
    public float deadzoneAngle = 30f; 
    private const float DEADZONE_MAGNITUDE = 0.1f; 
    private Vector2 processedLeftJoystick; 
    private Vector2 processedRightJoystick;

    private float throttleValue;
    private float yawValue;
    private float rollValue;
    private float pitchValue;

    public bool isSpray { get; private set; }
    public bool isDraw { get; private set; }
    public bool isRTL { get; private set; }
    public bool isAutomation { get; private set; }
    public bool isCaptureState { get; private set; }
    public bool isIndicatorState { get; private set; }
    public bool isThermalState { get; private set; }
    public int selectedCamValue { get; private set; }
    

    private Queue<bool> rtlStateQueue = new Queue<bool>();
    private Queue<bool> drawStateQueue = new Queue<bool>();
    private Queue<bool> sprayStateQueue = new Queue<bool>();
    private Queue<bool> automationStateQueue = new Queue<bool>();
    private Queue<bool> captureStateQueue = new Queue<bool>();
    private Queue<bool> indicatorStateQueue = new Queue<bool>();
    private Queue<bool> thermalStateQueue = new Queue<bool>();
    private Queue<int> selectedCamQueue = new Queue<int>();
    private Queue<Vector2> staticCamQueue = new Queue<Vector2>();
    private readonly object rtlQueueLock = new object();
    private readonly object drawQueueLock = new object();
    private readonly object sprayQueueLock = new object();
    private readonly object automationQueueLock = new object();
    private readonly object captureStateQueueLock = new object();
    private readonly object indicatorStateQueueLock = new object();
    private readonly object thermalStateQueueLock = new object();
    private readonly object selectedCamQueueLock = new object();
    private readonly object staticCamQueueLock = new object();

    public GameObject drawButton;
    public GameObject sprayButton;
    public GameObject automation;

    public float Throttle => throttleValue; 
    public float Yaw => yawValue;     
    public float Roll => rollValue;    
    public float Pitch => pitchValue;

    public UnityEngine.UI.Button rtlButton;

    public UnityEngine.UI.Button indicatorButton;
    public UnityEngine.UI.Button thermalButton;
    public UnityEngine.UI.Button captureButton;

    private DRONECONT droneController;
    public bool UseAndroidInput = false;

    private float deviceCheckInterval = 1f;
    private float lastDeviceCheckTime;

    private bool isTransitioning = false;
    private float lastTransitionTime;
    private const float TRANSITION_TIMEOUT = 2f;
    private bool needsControlTransition = false;
    private const int ADB_RESTART_DELAY = 1000;

    private static readonly Regex leftJoystickRegex = new Regex(@"LEFTJOYSTICK: ([^,]+),([^\s]+)", RegexOptions.Compiled);
    private static readonly Regex rightJoystickRegex = new Regex(@"RIGHTJOYSTICK: ([^,]+),([^\s]+)", RegexOptions.Compiled);
    private static readonly Regex staticCamRegex = new Regex(@"STATICCAM: ([^,]+),([^\s]+)", RegexOptions.Compiled);
    private static readonly Regex sprayStateRegex = new Regex(@"SPRAYSTATE: (True|False|true|false)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex drawStateRegex = new Regex(@"DRAWSTATE: (True|False|true|false)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex rtlStateRegex = new Regex(@"RTLSTATE: (True|False|true|false)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex automationStateRegex = new Regex(@"AUTOMATIONSTATE: (True|False)", RegexOptions.Compiled);

    private static readonly Regex captureStateRegex = new Regex(@"CAPTURESTATE: (True|False)", RegexOptions.Compiled);
    private static readonly Regex indicatorStateRegex = new Regex(@"INDICATORSTATE: (True|False)", RegexOptions.Compiled);
    private static readonly Regex thermalStateRegex = new Regex(@"THERMALSTATE: (True|False)", RegexOptions.Compiled);
    private static readonly Regex selectedCamRegex = new Regex(@"SELECTEDCAM: (\d+)");

    private bool isInitialized = false;
    private bool isInitializing = false;
    private Task initializationTask;

    private DronePathFollower cachedPathFollower;
    private ShapeDrawer cachedShapeDrawer;
    private WaterSprayToggle cachedSprayToggle;

    public StaticCameraMovement staticCameraMovement;
    public GameObject staticCameraJoystick;

    private bool isConnecting = false;
    private bool isDisconnecting = false;
    private CancellationTokenSource connectionCts;
    private const int CONNECTION_TIMEOUT = 1500; 
    private const int RECONNECT_DELAY = 200; 

    private bool isLogcatRunning = false;
    private const int LOGCAT_RESTART_DELAY = 2000;
    private const int MAX_RECONNECT_ATTEMPTS = 2;
    private int reconnectAttempts = 0;
    private const int CONNECTION_RETRY_DELAY = 2000; 
    private bool isForceReconnecting = false;
    private Task connectionTask;
    private bool isReconnecting = false;
    private Process currentADBProcess;
    private readonly object joystickLock = new object();
    private readonly object stateLock = new object();
    private volatile bool isProcessingValues = false;

    private MainThreadDispatcher mainThreadDispatcher;

    private float componentRetryTimer = 0f;
    private const float COMPONENT_RETRY_INTERVAL = 3f;
    private bool hasRetriedComponents = false;

    void Start()
    {
        // Add a small delay to ensure SelectionManager and other components are initialized first
        Invoke("DelayedStart", 0.2f);
    }

    private void DelayedStart()
    {
        UnityEngine.Debug.Log("PCReceiver DelayedStart called");
        
        // Find or create MainThreadDispatcher first
        mainThreadDispatcher = FindFirstObjectByType<MainThreadDispatcher>();
        if (mainThreadDispatcher == null)
        {
            UnityEngine.Debug.Log("Creating new MainThreadDispatcher");
            var go = new GameObject("MainThreadDispatcher");
            mainThreadDispatcher = go.AddComponent<MainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }

        // Find drone controller
        droneController = FindObjectOfType<DRONECONT>();
        if (droneController == null)
        {
            UnityEngine.Debug.LogError("DRONECONT not found in the scene!");
            // We'll continue even without the drone controller
        }
        else
        {
            UnityEngine.Debug.Log("Found DRONECONT successfully");
        }

        // Try to find the RTL button if not already assigned
        if (rtlButton == null)
        {
            // Look for RTL button by name in the scene
            var rtlObj = GameObject.Find("RTLButton");
            if (rtlObj != null)
            {
                rtlButton = rtlObj.GetComponent<UnityEngine.UI.Button>();
                UnityEngine.Debug.Log($"Found RTL button by name: {(rtlButton != null ? "success" : "failed")}");
            }
            else
            {
                // Try to find it by looking for all buttons and checking names
                var allButtons = FindObjectsOfType<UnityEngine.UI.Button>();
                foreach (var btn in allButtons)
                {
                    if (btn.name.Contains("RTL") || btn.name.Contains("Return"))
                    {
                        rtlButton = btn;
                        UnityEngine.Debug.Log($"Found RTL button by search: {btn.name}");
                        break;
                    }
                }
            }
        }
        else
        {
            UnityEngine.Debug.Log("RTL button already assigned in inspector");
        }

        // Find path follower component with multiple fallback methods
        cachedPathFollower = FindFirstObjectByType<DronePathFollower>();
        if (cachedPathFollower == null)
        {
            UnityEngine.Debug.Log("DronePathFollower not found directly, trying alternative methods...");
            
            // Try to find it under specific objects
            if (automation != null)
            {
                cachedPathFollower = automation.GetComponentInChildren<DronePathFollower>(true); // Include inactive objects
                UnityEngine.Debug.Log($"Tried to find DronePathFollower in automation's children: {(cachedPathFollower != null ? "found" : "not found")}");
            }

            // If still not found, try to find it in the drone controller's hierarchy
            if (cachedPathFollower == null && droneController != null)
            {
                cachedPathFollower = droneController.GetComponentInChildren<DronePathFollower>(true);
                UnityEngine.Debug.Log($"Tried to find DronePathFollower in drone controller's children: {(cachedPathFollower != null ? "found" : "not found")}");
            }

            // If still not found, try to find it by name
            if (cachedPathFollower == null)
            {
                var pathFollowerObj = GameObject.Find("DronePathFollower");
                if (pathFollowerObj != null)
                {
                    cachedPathFollower = pathFollowerObj.GetComponent<DronePathFollower>();
                    UnityEngine.Debug.Log($"Tried to find DronePathFollower by name: {(cachedPathFollower != null ? "found" : "not found")}");
                }
            }

            
        }
        else
        {
            UnityEngine.Debug.Log("Found DronePathFollower successfully");
        }

        // Find other components
        cachedShapeDrawer = FindObjectOfType<ShapeDrawer>();
        UnityEngine.Debug.Log($"ShapeDrawer found: {(cachedShapeDrawer != null ? "yes" : "no")}");
        
        cachedSprayToggle = FindObjectOfType<WaterSprayToggle>();
        UnityEngine.Debug.Log($"WaterSprayToggle found: {(cachedSprayToggle != null ? "yes" : "no")}");

        // Find StaticCameraMovement if not already assigned
        if (staticCameraMovement == null)
        {
            staticCameraMovement = FindObjectOfType<StaticCameraMovement>();
            UnityEngine.Debug.Log($"StaticCameraMovement found: {(staticCameraMovement != null ? "yes" : "no")}");
        }
        
        UnityEngine.Debug.Log("PCReceiver initialization complete");
        
        // Start ADB initialization after a short delay
        Invoke("StartADBInitialization", 0.5f);
    }
    
    private void StartADBInitialization()
    {
        if (!isInitialized && !isInitializing)
        {
            isInitializing = true;
            UnityEngine.Debug.Log("Starting ADB initialization");
            _ = InitializeADBAsync();
        }
    }

    void Update()
    {
        // If any critical components are null, try to find them periodically
        if ((rtlButton == null || cachedPathFollower == null) && !hasRetriedComponents)
        {
            componentRetryTimer += Time.deltaTime;
            if (componentRetryTimer >= COMPONENT_RETRY_INTERVAL)
            {
                RetryFindComponents();
                componentRetryTimer = 0f;
                hasRetriedComponents = true; // Only retry once to avoid constant searching
            }
        }

        if (!isInitialized && !isInitializing)
        {
            isInitializing = true;
            UnityEngine.Debug.Log("Starting ADB initialization from Update");
            _ = InitializeADBAsync();
            return;
        }

        if (isInitializing && initializationTask != null && initializationTask.IsCompleted)
        {
            isInitializing = false;
            isInitialized = true;
        }

        if (!isInitialized)
        {
            return;
        }

        if (UseAndroidInput && !isTransitioning)
        {
            ProcessJoystickInput();
        }
        else
        {
            ResetJoystickValues();
        }

        if (isTransitioning && Time.time - lastTransitionTime > TRANSITION_TIMEOUT)
        {
            isTransitioning = false;
            needsControlTransition = false;
            UnityEngine.Debug.Log("Control transition completed");
        }

        if (Time.time - lastDeviceCheckTime >= deviceCheckInterval)
        {
            lastDeviceCheckTime = Time.time;
            if (!isReconnecting)
            {
                _ = CheckDeviceConnectionAsync();
            }
        }

        if (!isTransitioning)
        {
            ProcessStateQueues();
        }
    }

    private void ProcessJoystickInput()
    {
        Vector2 currentLeftJoystick;
        Vector2 currentRightJoystick;

        lock (joystickLock)
        {
            currentLeftJoystick = leftJoystick;
            currentRightJoystick = rightJoystick;
        }

        // Process left joystick
        float leftMagnitude = currentLeftJoystick.magnitude;
        float leftAngle = Mathf.Atan2(currentLeftJoystick.y, currentLeftJoystick.x) * Mathf.Rad2Deg;
        if (leftAngle < 0) leftAngle += 360f;

        if (leftMagnitude > DEADZONE_MAGNITUDE)
        {
            bool isPrimaryY = (leftAngle >= 90f - deadzoneAngle && leftAngle <= 90f + deadzoneAngle) || 
                            (leftAngle >= 270f - deadzoneAngle && leftAngle <= 270f + deadzoneAngle);
            bool isPrimaryX = (leftAngle >= 0f - deadzoneAngle && leftAngle <= 0f + deadzoneAngle) || 
                            (leftAngle >= 180f - deadzoneAngle && leftAngle <= 180f + deadzoneAngle);

            if (isPrimaryY)
            {
                processedLeftJoystick = new Vector2(currentLeftJoystick.x * 0.2f, currentLeftJoystick.y);
            }
            else if (isPrimaryX)
            {
                processedLeftJoystick = new Vector2(currentLeftJoystick.x, currentLeftJoystick.y * 0.2f);
            }
            else
            {
                processedLeftJoystick = currentLeftJoystick;
            }
        }
        else
        {
            processedLeftJoystick = Vector2.zero;
        }

        // Process right joystick with same deadzone behavior
        float rightMagnitude = currentRightJoystick.magnitude;
        float rightAngle = Mathf.Atan2(currentRightJoystick.y, currentRightJoystick.x) * Mathf.Rad2Deg;
        if (rightAngle < 0) rightAngle += 360f;

        if (rightMagnitude > DEADZONE_MAGNITUDE)
        {
            bool isPrimaryY = (rightAngle >= 90f - deadzoneAngle && rightAngle <= 90f + deadzoneAngle) || 
                            (rightAngle >= 270f - deadzoneAngle && rightAngle <= 270f + deadzoneAngle);
            bool isPrimaryX = (rightAngle >= 0f - deadzoneAngle && rightAngle <= 0f + deadzoneAngle) || 
                            (rightAngle >= 180f - deadzoneAngle && rightAngle <= 180f + deadzoneAngle);

            if (isPrimaryY)
            {
                processedRightJoystick = new Vector2(currentRightJoystick.x * 0.2f, currentRightJoystick.y);
            }
            else if (isPrimaryX)
            {
                processedRightJoystick = new Vector2(currentRightJoystick.x, currentRightJoystick.y * 0.2f);
            }
            else
            {
                processedRightJoystick = currentRightJoystick;
            }
        }
        else
        {
            processedRightJoystick = Vector2.zero;
        }

        throttleValue = processedLeftJoystick.y;
        yawValue = processedLeftJoystick.x;
        rollValue = processedRightJoystick.x;
        pitchValue = processedRightJoystick.y;
    }

    private void ResetJoystickValues()
    {
        processedLeftJoystick = Vector2.zero;
        processedRightJoystick = Vector2.zero;
        throttleValue = 0f;
        yawValue = 0f;
        rollValue = 0f;
        pitchValue = 0f;
    }

    private void ProcessStateQueues()
    {
        lock (automationQueueLock)
        {
            while (automationStateQueue.Count > 0)
            {
                bool newAutomationState = automationStateQueue.Dequeue();
                UnityEngine.Debug.Log($"Processing Automation state: {newAutomationState}, automation object: {(automation != null ? "exists" : "null")}, cachedPathFollower: {(cachedPathFollower != null ? "exists" : "null")}");
                if (automation != null && cachedPathFollower != null)
                {
                    try
                    {
                        UnityEngine.Debug.Log("Attempting to start following path");
                        cachedPathFollower.StartFollowingPath();
                        UnityEngine.Debug.Log("Successfully started following path");
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogError($"Error calling StartFollowingPath: {e.Message}\n{e.StackTrace}");
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError($"Cannot execute automation: automation={automation != null}, cachedPathFollower={cachedPathFollower != null}");
                }
                isAutomation = newAutomationState;
            }
        }

        if (droneController == null) 
        {
            return;
        }

        lock (rtlQueueLock)
        {
            while (rtlStateQueue.Count > 0)
            {
                bool newRTLState = rtlStateQueue.Dequeue();
                UnityEngine.Debug.Log($"Processing RTL state: {newRTLState}, rtlButton: {(rtlButton != null ? "exists" : "null")}");
                if (rtlButton != null)
                {
                    UnityEngine.Debug.Log("Invoking RTL button click");
                    rtlButton.onClick.Invoke();
                    UnityEngine.Debug.Log("RTL button click invoked successfully");
                }
                else
                {
                    UnityEngine.Debug.LogError("RTL button is null - cannot invoke return to launch");
                }
                isRTL = newRTLState;
            }
        }

        lock (drawQueueLock)
        {
            while (drawStateQueue.Count > 0)
            {
                bool newDrawState = drawStateQueue.Dequeue();
                if (drawButton != null && drawButton.activeInHierarchy && cachedShapeDrawer != null)
                {
                    cachedShapeDrawer.ToggleDrawing();
                }
                isDraw = newDrawState;
            }
        }

        lock (sprayQueueLock)
        {
            while (sprayStateQueue.Count > 0)
            {
                bool newSprayState = sprayStateQueue.Dequeue();
                if (sprayButton != null && sprayButton.activeInHierarchy && cachedSprayToggle != null)
                {
                    cachedSprayToggle.ToggleSpray();
                }
                isSpray = newSprayState;
            }
        }
    }

    private async Task InitializeADBAsync()
    {
        if (automation != null)
        {
            UnityEngine.Debug.Log("Starting ADB process initialization");
            
            try
            {
                // Run these tasks asynchronously to avoid blocking the main thread
                var initTask = Task.Run(async () => {
                    await Task.Delay(100); // Small delay to ensure UI responsiveness
                    return await StartADBProcessAsync();
                });
                
                // Short timeout to prevent long freezes
                bool result = await Task.WhenAny(initTask, Task.Delay(3000)) == initTask && await initTask;
                
                if (result)
                {
                    UnityEngine.Debug.Log("ADB initialization completed successfully");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("ADB initialization timed out or failed - will retry later");
                    // Set a flag to retry later instead of blocking now
                    Invoke("RetryADBInitialization", 5f);
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Error in ADB initialization: {ex.Message}");
            }
        }
        isInitialized = true;
    }
    
    private void RetryADBInitialization()
    {
        if (!UseAndroidInput)
        {
            UnityEngine.Debug.Log("Retrying ADB initialization");
            _ = StartADBProcessAsync();
        }
    }

    private bool StartLogcatProcess()
    {
        try
        {
            if (currentADBProcess != null)
            {
                try
                {
                    if (!currentADBProcess.HasExited)
                    {
                        currentADBProcess.Kill();
                    }
                    currentADBProcess.Dispose();
                }
                catch { }
                currentADBProcess = null;
            }

            using (Process clearProcess = new Process())
            {
                clearProcess.StartInfo.FileName = @"C:\Android\platform-tools\adb.exe";
                clearProcess.StartInfo.Arguments = "logcat -c";
                clearProcess.StartInfo.UseShellExecute = false;
                clearProcess.StartInfo.CreateNoWindow = true;
                clearProcess.Start();
                clearProcess.WaitForExit();
            }

            currentADBProcess = new Process();
            currentADBProcess.StartInfo.FileName = @"C:\Android\platform-tools\adb.exe";
            currentADBProcess.StartInfo.Arguments = "logcat -v time Unity:V *:S";
            currentADBProcess.StartInfo.UseShellExecute = false;
            currentADBProcess.StartInfo.RedirectStandardOutput = true;
            currentADBProcess.StartInfo.RedirectStandardError = true;
            currentADBProcess.StartInfo.CreateNoWindow = true;
            currentADBProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            
            currentADBProcess.Start();
            
            if (logcatThread != null && logcatThread.IsAlive)
            {
                try
                {
                    logcatThread.Abort();
                }
                catch { }
                logcatThread = null;
            }

            isLogcatRunning = true;
            logcatThread = new Thread(ReadLogcat);
            logcatThread.IsBackground = true;
            logcatThread.Priority = System.Threading.ThreadPriority.Highest;
            logcatThread.Start();

            return true;
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Error starting logcat process: {e.Message}");
            return false;
        }
    }

    void ReadLogcat()
    {
        if (currentADBProcess == null || currentADBProcess.StandardOutput == null)
        {
            UnityEngine.Debug.LogError("ADB process or StandardOutput is null");
            isLogcatRunning = false;
            return;
        }

        StreamReader reader = currentADBProcess.StandardOutput;
        UnityEngine.Debug.Log("Starting to read logcat output");

        while (running && !reader.EndOfStream)
        {
            try
            {
                string line = reader.ReadLine();

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                if (line.Contains("LEFTJOYSTICK:") || line.Contains("RIGHTJOYSTICK:") ||
                    line.Contains("RTLSTATE:") || line.Contains("SPRAYSTATE:") ||
                    line.Contains("DRAWSTATE:") || line.Contains("AUTOMATIONSTATE:") ||
                    line.Contains("CAPTURESTATE:") || line.Contains("INDICATORSTATE:") ||
                    line.Contains("THERMALSTATE:") || line.Contains("SELECTEDCAM:") ||
                    line.Contains("STATICCAM:"))
                {
                    ProcessControlLine(line);
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Error reading logcat: {e.Message}");
                Thread.Sleep(5); 
            }
        }

        isLogcatRunning = false;
        UnityEngine.Debug.Log("Logcat reading stopped");
    }

    private void ProcessControlLine(string line)
    {
        if (isProcessingValues) return;
        isProcessingValues = true;

        try
        {
            // Added debug logging for incoming control lines
            if (line.Contains("RTLSTATE:") || line.Contains("AUTOMATIONSTATE:"))
            {
                UnityEngine.Debug.Log($"Received control line: {line}");
            }

            if (line.Contains("LEFTJOYSTICK:"))
            {
                var match = leftJoystickRegex.Match(line);
                if (match.Success)
                {
                    if (float.TryParse(match.Groups[1].Value, out float x) &&
                        float.TryParse(match.Groups[2].Value, out float y))
                    {
                        lock (joystickLock)
                        {
                            leftJoystick = new Vector2(-x, y);
                        }
                    }
                }
            }
            else if (line.Contains("RIGHTJOYSTICK:"))
            {
                var match = rightJoystickRegex.Match(line);
                if (match.Success)
                {
                    if (float.TryParse(match.Groups[1].Value, out float x) &&
                        float.TryParse(match.Groups[2].Value, out float y))
                    {
                        lock (joystickLock)
                        {
                            rightJoystick = new Vector2(-x, y);
                        }
                    }
                }
            }
            else if (line.Contains("STATICCAM:"))
            {
                var match = staticCamRegex.Match(line);
                if (match.Success)
                {
                    if (float.TryParse(match.Groups[1].Value, out float x) &&
                        float.TryParse(match.Groups[2].Value, out float y))
                    {
                        lock (staticCamQueueLock)
                        {
                            staticCamPosition = new Vector2(x, y);
                            if (staticCameraMovement != null)
                            {
                                if (mainThreadDispatcher != null)
                                {
                                    mainThreadDispatcher.Enqueue(() => {
                                        if (!staticCameraJoystick.gameObject.activeInHierarchy) return;
                                        staticCameraMovement.movementVector = staticCamPosition;
                                    });
                                }
                                else
                                {
                                    UnityEngine.Debug.LogWarning("MainThreadDispatcher is null, cannot update staticCameraMovement");
                                }
                            }
                            else
                            {
                                UnityEngine.Debug.LogWarning("StaticCameraMovement reference is null");
                            }
                        }
                    }
                }
            }
            else if (line.Contains("RTLSTATE:"))
            {
                var match = rtlStateRegex.Match(line);
                if (match.Success)
                {
                    try 
                    {
                        bool newRTLState = bool.Parse(match.Groups[1].Value.ToLower());
                        UnityEngine.Debug.Log($"Parsed RTL state: {newRTLState}");
                        
                        // Only act if true (button press) or if it's a state change
                        if (newRTLState || newRTLState != isRTL)
                        {
                            // Use main thread dispatcher to trigger button click
                            if (mainThreadDispatcher != null)
                            {
                                mainThreadDispatcher.Enqueue(() => TriggerRTLButtonClick());
                                UnityEngine.Debug.Log("Dispatched RTL button click to main thread");
                            }
                            else
                            {
                                // Fall back to queue if dispatcher not available
                                lock (rtlQueueLock)
                                {
                                    rtlStateQueue.Enqueue(newRTLState);
                                    UnityEngine.Debug.Log($"Enqueued RTL state: {newRTLState}, queue count: {rtlStateQueue.Count}");
                                }
                            }
                            isRTL = newRTLState;
                        }
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogError($"Error processing RTL state: {e.Message}");
                    }
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"Failed to match RTLSTATE pattern in: {line}");
                }
            }
            else if (line.Contains("SPRAYSTATE:"))
            {
                var match = sprayStateRegex.Match(line);
                if (match.Success)
                {
                    try 
                    {
                        bool newSprayState = bool.Parse(match.Groups[1].Value.ToLower());
                        lock (sprayQueueLock)
                        {
                            sprayStateQueue.Enqueue(newSprayState);
                        }
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogError($"Error processing spray state: {e.Message}");
                    }
                }
            }
            else if (line.Contains("DRAWSTATE:"))
            {
                var match = drawStateRegex.Match(line);
                if (match.Success)
                {
                    try 
                    {
                        bool newDrawState = bool.Parse(match.Groups[1].Value.ToLower());
                        lock (drawQueueLock)
                        {
                            drawStateQueue.Enqueue(newDrawState);
                        }
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogError($"Error processing draw state: {e.Message}");
                    }
                }
            }
            else if (line.Contains("AUTOMATIONSTATE:"))
            {
                var match = automationStateRegex.Match(line);
                if (match.Success)
                {
                    try 
                    {
                        bool newAutomationState = bool.Parse(match.Groups[1].Value);
                        UnityEngine.Debug.Log($"Parsed Automation state: {newAutomationState}");
                        
                        // Only act if true (button press) or if it's a state change
                        if (newAutomationState || newAutomationState != isAutomation)
                        {
                            // Use main thread dispatcher to trigger automation
                            if (mainThreadDispatcher != null)
                            {
                                mainThreadDispatcher.Enqueue(() => TriggerAutomationStart());
                                UnityEngine.Debug.Log("Dispatched automation start to main thread");
                            }
                            else
                            {
                                // Fall back to queue if dispatcher not available
                                lock (automationQueueLock)
                                {
                                    automationStateQueue.Enqueue(newAutomationState);
                                    UnityEngine.Debug.Log($"Enqueued Automation state: {newAutomationState}, queue count: {automationStateQueue.Count}");
                                }
                            }
                            isAutomation = newAutomationState;
                        }
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogError($"Error processing automation state: {e.Message}");
                    }
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"Failed to match AUTOMATIONSTATE pattern in: {line}");
                }
            }
            else if (line.Contains("CAPTURESTATE:"))
            {
                var match = captureStateRegex.Match(line);
                if (match.Success)
                {
                    try 
                    {
                        bool newCaptureState = bool.Parse(match.Groups[1].Value);
                        UnityEngine.Debug.Log($"Parsed Capture state: {newCaptureState}");
                        
                        // Only act if true (button press) or if it's a state change
                        if (newCaptureState || newCaptureState != isCaptureState)
                        {
                            if (mainThreadDispatcher != null)
                            {
                                mainThreadDispatcher.Enqueue(() => TriggerCaptureState());
                                UnityEngine.Debug.Log("Dispatched capture state change to main thread");
                            }
                            else
                            {
                                lock (captureStateQueueLock)
                                {
                                    captureStateQueue.Enqueue(newCaptureState);
                                    UnityEngine.Debug.Log($"Enqueued Capture state: {newCaptureState}, queue count: {captureStateQueue.Count}");
                                }
                            }
                            isCaptureState = newCaptureState;
                        }
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogError($"Error processing capture state: {e.Message}");
                    }
                }
            }
            else if (line.Contains("INDICATORSTATE:"))
            {
                var match = indicatorStateRegex.Match(line);
                if (match.Success)
                {
                    try 
                    {
                        bool newIndicatorState = bool.Parse(match.Groups[1].Value);
                        UnityEngine.Debug.Log($"Parsed Indicator state: {newIndicatorState}");
                        
                        // Only act if true (button press) or if it's a state change
                        if (newIndicatorState || newIndicatorState != isIndicatorState)
                        {
                            if (mainThreadDispatcher != null)
                            {
                                mainThreadDispatcher.Enqueue(() => TriggerIndicatorState());
                                UnityEngine.Debug.Log("Dispatched indicator state change to main thread");
                            }
                            else
                            {
                                lock (indicatorStateQueueLock)
                                {
                                    indicatorStateQueue.Enqueue(newIndicatorState);
                                    UnityEngine.Debug.Log($"Enqueued Indicator state: {newIndicatorState}, queue count: {indicatorStateQueue.Count}");
                                }
                            }
                            isIndicatorState = newIndicatorState;
                        }
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogError($"Error processing indicator state: {e.Message}");
                    }
                }
            }
            else if (line.Contains("THERMALSTATE:"))
            {
                var match = thermalStateRegex.Match(line);
                if (match.Success)
                {
                    try 
                    {
                        bool newThermalState = bool.Parse(match.Groups[1].Value);
                        UnityEngine.Debug.Log($"Parsed Thermal state: {newThermalState}");
                        
                        // Only act if true (button press) or if it's a state change
                        if (newThermalState || newThermalState != isThermalState)
                        {
                            if (mainThreadDispatcher != null)
                            {
                                mainThreadDispatcher.Enqueue(() => TriggerThermalState());
                                UnityEngine.Debug.Log("Dispatched thermal state change to main thread");
                            }
                            else
                            {
                                lock (thermalStateQueueLock)
                                {
                                    thermalStateQueue.Enqueue(newThermalState);
                                    UnityEngine.Debug.Log($"Enqueued Thermal state: {newThermalState}, queue count: {thermalStateQueue.Count}");
                                }
                            }
                            isThermalState = newThermalState;
                        }
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogError($"Error processing thermal state: {e.Message}");
                    }
                }
            }
            else if (line.Contains("SELECTEDCAM:"))
            {
                var match = selectedCamRegex.Match(line);
                if (match.Success)
                {
                    try 
                    {
                        if (int.TryParse(match.Groups[1].Value, out int camValue))
                        {
                            lock (selectedCamQueueLock)
                            {
                                selectedCamQueue.Enqueue(camValue);
                                selectedCamValue = camValue;
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogError($"Error processing selected cam: {e.Message}");
                    }
                }
            }
        }
        finally
        {
            isProcessingValues = false;
        }
    }

    private void StopAllThreads()
    {
        running = false;
        UseAndroidInput = false;

        try
        {
            if (currentADBProcess != null)
            {
                if (!currentADBProcess.HasExited)
                {
                    currentADBProcess.Kill();
                }
                currentADBProcess.Dispose();
                currentADBProcess = null;
            }

            if (logcatThread != null && logcatThread.IsAlive)
            {
                logcatThread.Join(100); 
                if (logcatThread.IsAlive)
                {
                    logcatThread.Abort();
                }
                logcatThread = null;
            }

            leftJoystick = Vector2.zero;
            rightJoystick = Vector2.zero;
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Error stopping threads: {e.Message}");
        }
    }

    private async Task CheckDeviceConnectionAsync()
    {
        if (isConnecting || isDisconnecting || isReconnecting) return;

        try
        {
            using (Process checkProcess = new Process())
            {
                checkProcess.StartInfo.FileName = @"C:\Android\platform-tools\adb.exe";
                checkProcess.StartInfo.Arguments = "devices";
                checkProcess.StartInfo.UseShellExecute = false;
                checkProcess.StartInfo.RedirectStandardOutput = true;
                checkProcess.StartInfo.CreateNoWindow = true;
                
                checkProcess.Start();
                string output = await Task.Run(() => checkProcess.StandardOutput.ReadToEnd());
                await Task.Run(() => checkProcess.WaitForExit());
                
                bool isConnected = output.Split('\n').Any(line => 
                    line.Contains("device") && !line.Contains("List of devices"));
                
                if (!isConnected && UseAndroidInput)
                {
                    await HandleDisconnectionAsync();
                }
                else if (isConnected && !UseAndroidInput)
                {
                    await HandleReconnectionAsync();
                }
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Error checking device connection: {e.Message}");
            if (UseAndroidInput)
            {
                await HandleDisconnectionAsync();
            }
        }
    }

    private async Task HandleReconnectionAsync()
    {
        if (isConnecting || isReconnecting) return;
        isReconnecting = true;

        try
        {
            if (reconnectAttempts >= MAX_RECONNECT_ATTEMPTS)
            {
                UnityEngine.Debug.Log("Max reconnection attempts reached, resetting connection");
                reconnectAttempts = 0;
            }

            reconnectAttempts++;
            UnityEngine.Debug.Log($"Attempting reconnection (attempt {reconnectAttempts}/{MAX_RECONNECT_ATTEMPTS})");
            
            connectionTask = Task.Run(async () =>
            {
                try
                {
                    await CleanupAndRestartAsync();
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError($"Error in reconnection task: {e.Message}");
                }
            });
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Error in HandleReconnectionAsync: {e.Message}");
        }
        finally
        {
            isReconnecting = false;
        }
    }

    private async Task CleanupAndRestartAsync()
    {
        try
        {
            await Task.Run(() => StopAllThreads());

            leftJoystick = Vector2.zero;
            rightJoystick = Vector2.zero;
            staticCamPosition = Vector2.zero;
            if (staticCameraMovement != null && mainThreadDispatcher != null)
            {
                mainThreadDispatcher.Enqueue(() => {
                    staticCameraMovement.movementVector = Vector2.zero;
                });
            }
            isSpray = false;
            isDraw = false;
            isRTL = false;
            isAutomation = false;
            isCaptureState = false;
            isIndicatorState = false;
            isThermalState = false;
            selectedCamValue = 0;

            lock (rtlQueueLock) rtlStateQueue.Clear();
            lock (drawQueueLock) drawStateQueue.Clear();
            lock (sprayQueueLock) sprayStateQueue.Clear();
            lock (automationQueueLock) automationStateQueue.Clear();
            lock (captureStateQueueLock) captureStateQueue.Clear();
            lock (indicatorStateQueueLock) indicatorStateQueue.Clear();
            lock (thermalStateQueueLock) thermalStateQueue.Clear();
            lock (selectedCamQueueLock) selectedCamQueue.Clear();
            lock (staticCamQueueLock) staticCamQueue.Clear();

            running = true;

            if (await StartADBProcessAsync())
            {
                UnityEngine.Debug.Log("Connection re-established successfully");
                await Task.Yield();
                if (mainThreadDispatcher != null)
                {
                    mainThreadDispatcher.Enqueue(() => {
                        isTransitioning = true;
                        lastTransitionTime = Time.time;
                        needsControlTransition = true;
                    });
                }
                else
                {
                    UnityEngine.Debug.LogError("MainThreadDispatcher not found!");
                }
            }
            else
            {
                UnityEngine.Debug.LogError("Failed to re-establish connection");
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Error in CleanupAndRestartAsync: {e.Message}");
        }
    }

    private async Task HandleDisconnectionAsync()
    {
        isDisconnecting = true;
        try
        {
            await Task.Run(() => StopAllThreads());
            needsControlTransition = true;
        }
        finally
        {
            isDisconnecting = false;
        }
    }

    void OnDestroy()
    {
        connectionCts?.Cancel();
        connectionCts?.Dispose();
        StopAllThreads();
    }

    private async Task<bool> StartADBProcessAsync()
    {
        if (isConnecting) return false;
        isConnecting = true;

        try
        {
            var killTask = Task.Run(() => KillAllADBProcesses());
            var startServerTask = Task.Run(() => StartADBServer());
            
            await Task.WhenAll(killTask, startServerTask);

            if (!StartLogcatProcess())
            {
                UnityEngine.Debug.LogError("Failed to start logcat process");
                return false;
            }

            using (Process devicesProcess = new Process())
            {
                devicesProcess.StartInfo.FileName = @"C:\Android\platform-tools\adb.exe";
                devicesProcess.StartInfo.Arguments = "devices";
                devicesProcess.StartInfo.UseShellExecute = false;
                devicesProcess.StartInfo.RedirectStandardOutput = true;
                devicesProcess.StartInfo.CreateNoWindow = true;
                devicesProcess.Start();
                string output = await Task.Run(() => devicesProcess.StandardOutput.ReadToEnd());
                await Task.Run(() => devicesProcess.WaitForExit());
                
                bool isConnected = output.Split('\n').Any(line => 
                    line.Contains("device") && !line.Contains("List of devices"));
                
                if (!isConnected)
                {
                    UnityEngine.Debug.LogError("No device connected after starting ADB");
                    return false;
                }
            }

            UseAndroidInput = true;
            reconnectAttempts = 0;
            return true;
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Error in StartADBProcessAsync: {e.Message}");
            UseAndroidInput = false;
            return false;
        }
        finally
        {
            isConnecting = false;
        }
    }

    private void KillAllADBProcesses()
    {
        try
        {
            var killServerTask = Task.Run(() =>
            {
                using (Process killServer = new Process())
                {
                    killServer.StartInfo.FileName = @"C:\Android\platform-tools\adb.exe";
                    killServer.StartInfo.Arguments = "kill-server";
                    killServer.StartInfo.UseShellExecute = false;
                    killServer.StartInfo.CreateNoWindow = true;
                    killServer.Start();
                    killServer.WaitForExit();
                }
            });

            var killProcessesTask = Task.Run(() =>
            {
                using (Process taskkill = new Process())
                {
                    taskkill.StartInfo.FileName = "taskkill";
                    taskkill.StartInfo.Arguments = "/F /IM adb.exe";
                    taskkill.StartInfo.UseShellExecute = false;
                    taskkill.StartInfo.CreateNoWindow = true;
                    taskkill.Start();
                    taskkill.WaitForExit();
                }
            });

            Task.WaitAll(killServerTask, killProcessesTask);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Error killing ADB processes: {e.Message}");
        }
    }

    private void StartADBServer()
    {
        try
        {
            using (Process startServer = new Process())
            {
                startServer.StartInfo.FileName = @"C:\Android\platform-tools\adb.exe";
                startServer.StartInfo.Arguments = "start-server";
                startServer.StartInfo.UseShellExecute = false;
                startServer.StartInfo.CreateNoWindow = true;
                startServer.Start();
                startServer.WaitForExit();
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Error starting ADB server: {e.Message}");
            throw;
        }
    }

    public class MainThreadDispatcher : MonoBehaviour
    {
        private readonly Queue<Action> executionQueue = new Queue<Action>();
        private readonly object queueLock = new object();

        void Update()
        {
            lock (queueLock)
            {
                while (executionQueue.Count > 0)
                {
                    executionQueue.Dequeue().Invoke();
                }
            }
        }

        public void Enqueue(Action action)
        {
            lock (queueLock)
            {
                executionQueue.Enqueue(action);
            }
        }
    }

    private void RetryFindComponents()
    {
        UnityEngine.Debug.Log("Retrying to find missing components...");
        
        // Try to find RTL button if it's missing
        if (rtlButton == null)
        {
            var allButtons = FindObjectsOfType<UnityEngine.UI.Button>();
            foreach (var btn in allButtons)
            {
                if (btn.name.Contains("RTL") || btn.name.Contains("Return"))
                {
                    rtlButton = btn;
                    UnityEngine.Debug.Log($"Found RTL button on retry: {btn.name}");
                    break;
                }
            }
            
            // If still not found, look for a button with specific colors or icons that might be the RTL button
            if (rtlButton == null)
            {
                foreach (var btn in allButtons)
                {
                    var img = btn.GetComponent<UnityEngine.UI.Image>();
                    if (img != null && img.color.r > 0.7f && img.color.g < 0.3f) // Often RTL buttons are red
                    {
                        rtlButton = btn;
                        UnityEngine.Debug.Log($"Found possible RTL button by color: {btn.name}");
                        break;
                    }
                }
            }
        }
        
        // Try to find path follower if it's missing
        if (cachedPathFollower == null)
        {
            // First try normal find
            cachedPathFollower = FindObjectOfType<DronePathFollower>();
            
            // If still null and we have a drone reference, try to find it there
            if (cachedPathFollower == null && droneController != null)
            {
                cachedPathFollower = droneController.GetComponentInChildren<DronePathFollower>();
                if (cachedPathFollower != null)
                    UnityEngine.Debug.Log("Found DronePathFollower in drone controller's children");
            }
            
            // If still null and we have an automation reference, try to find it there
            if (cachedPathFollower == null && automation != null)
            {
                cachedPathFollower = automation.GetComponentInChildren<DronePathFollower>();
                if (cachedPathFollower != null)
                    UnityEngine.Debug.Log("Found DronePathFollower in automation's children");
            }
        }
        
    }

    private void TriggerRTLButtonClick()
    {
        if (rtlButton != null)
        {
            rtlButton.onClick.Invoke();
        }
    }

    private void TriggerAutomationStart()
    {
        if (cachedPathFollower != null)
        {
            cachedPathFollower.StartFollowingPath();
        }
    }

    private void TriggerIndicatorState()
    {
        if (indicatorButton == null || !indicatorButton.gameObject.activeInHierarchy) return;
        indicatorButton.onClick.Invoke();
    }

    private void TriggerCaptureState()
    {
        if (captureButton == null || !captureButton.gameObject.activeInHierarchy) return;
        captureButton.onClick.Invoke();
    }

    private void TriggerThermalState()
    {
        if (thermalButton == null || !thermalButton.gameObject.activeInHierarchy) return;
        thermalButton.onClick.Invoke();
    }
} 
 
 
 