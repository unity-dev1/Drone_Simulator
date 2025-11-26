using UnityEngine;
using TMPro;
using System.Collections;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("Task Setup")]
    [TextArea]
    public string[] tutorialTasks;   // Set task list in Inspector
    private int currentTaskIndex = 0;
    [SerializeField]
    private GameObject[] gifs;

    [Header("UI Reference")]
    public TextMeshProUGUI taskText;  // Assign TMP text in Inspector
    public GameObject tutorialUI;     // UI Image container

    [Header("References")]
    public DRONECONT droneController;
    public FSJoystickInput joystickInput;

    [Header("🎯 Checkpoint References")]
    [SerializeField] private TrackCheckpoint trackCheckpoint;

    private bool taskInProgress = false;
    [Header("🛬 Landing Pad Reference")]
    private LandingPad landingPad;

    [Header("🎥 Gimbal Camera")]
    [SerializeField] private Camera gimbalCamera;
    [SerializeField] private GameObject highlightedObject;
    [SerializeField] private float focusDistance = 20f;


    [Header("Yaw Arrows")]
    [SerializeField] private GameObject leftYawArrow;
    [SerializeField] private GameObject rightYawArrow;

    [Header("🧠 Practice System")]
    [SerializeField] private GameObject practiceUI;
    [SerializeField] private DRAWSHAPESTutorial drawSystem;
    private int testAttemptCount = 0;
    [HideInInspector] public DRAWSHAPESTutorial.ShapeType playerSelectedShape = DRAWSHAPESTutorial.ShapeType.None;

    [Header("Button")]
    [SerializeField] private GameObject startStopButton;
    public GameObject[] checkpointColliders;

    private DroneYawTrail droneYawTrail;

    private Coroutine activeTaskCoroutine;

    private bool hasEnteredGimbalMode = false;
    private bool hasFocusedObject = false;
    private bool checkpointObjectiveCompleted = false;
    private bool squareTaskCompleted = false;

    private bool isCompletingStep = false;

    public static EventHandler OnPlayerWrongCheckpoint;
    public static Action OnTrackComplete;

    private void Awake()
    {
        leftYawArrow.SetActive(false);
        rightYawArrow.SetActive(false); 
        highlightedObject.SetActive(false);
        startStopButton.SetActive(false);
    }
    private void Start()
    {
        StartCoroutine(AssignDroneReferences());
        landingPad = GameObject.Find("Landing_Pad").GetComponent<LandingPad>();

        // ✅ Subscribe to normal track
        if (trackCheckpoint != null)
        {
            trackCheckpoint.OnTrackComplete += HandleTrackComplete;
            trackCheckpoint.OnPlayerWrongCheckpoint += HandleWrongCheckpoint;
        }

        // Show first task’s GIF immediately
        ShowCurrentTaskGif();

        // Deactivate all checkpoint colliders initially
        for (int i = 0; i < checkpointColliders.Length; i++)
        {
            checkpointColliders[i].gameObject.SetActive(false);
        }

        droneYawTrail=GameObject.Find("Drone_01").GetComponent<DroneYawTrail>();
    }
    // Called when normal track is completed
    private void HandleTrackComplete(object sender, EventArgs e)
    {
        checkpointObjectiveCompleted = true;

        Debug.Log("✅ TutorialManager: Track completed!");
        // If player is on the square task
        if (currentTaskIndex == 14)
        {
            squareTaskCompleted = false;

            // Tell player to retry
            if (taskText != null)
                taskText.text = "❌ Invalid square! Please try again.";
        }
    }
   
    // Called when square track is completed
    private void HandleSquareComplete(object sender, EventArgs e)
    {
        squareTaskCompleted = true;
        Debug.Log("✅ TutorialManager: Square task completed!");
    }

    // Wrong checkpoint (works for both)
    private void HandleWrongCheckpoint(object sender, EventArgs e)
    {
        Debug.Log("❌ Wrong checkpoint triggered!");
    }
    private IEnumerator AssignDroneReferences()
    {
        yield return null; // wait one frame

        GameObject droneObj = GameObject.FindGameObjectWithTag("Player");
        if (droneObj != null)
        {
            droneController = droneObj.GetComponent<DRONECONT>();
            joystickInput = droneObj.GetComponent<FSJoystickInput>();
        }
        else
        {
            Debug.LogError("No Drone with tag 'Player' found in the scene!");
        }

        if (tutorialTasks.Length > 0 && taskText != null)
        {
            taskText.text = tutorialTasks[0];
        }
    }

    private void Update()
    {
        if (currentTaskIndex == 7)
        {
            leftYawArrow.SetActive(true);
        }
        else if (currentTaskIndex == 8) 
        {
            leftYawArrow.SetActive(false);
            rightYawArrow.SetActive(true);
        }
        else
        {
            rightYawArrow.SetActive(false);
        }


        // Enter gimbal mode with Key 8
        if (currentTaskIndex == 9 && !hasEnteredGimbalMode)
        {
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                hasEnteredGimbalMode = true;
                Debug.Log("🎥 Gimbal Mode Entered!");

                highlightedObject.SetActive(true);

                // ✅ complete step 9 right away
                StartCompleteTask();
            }
        }

        // Focus on highlighted object check
        if (currentTaskIndex == 10 && !hasFocusedObject)
        {
            Ray ray = new Ray(gimbalCamera.transform.position, gimbalCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 50f))
            {
                if (hit.collider.gameObject == highlightedObject)
                {
                    hasFocusedObject = true;
                    Debug.Log("✅ Highlighted Object Focused!");
                    StartCompleteTask();
                }
            }
        }

        if (currentTaskIndex == 11)
        {
            highlightedObject.SetActive(false);
        }

        if(currentTaskIndex == 12)
        {
            drawSystem.startButton.SetActive(true);
            
            foreach (var btn in drawSystem.shapeButtons)
                btn.SetActive(true);

            drawSystem.exitPracticeButton.SetActive(true);
        }

        if(currentTaskIndex == 13)
        {
            startStopButton.SetActive(true);
        }
        if (currentTaskIndex == 15) 
        { 
            startStopButton.SetActive(false); 
        }   

        if (droneController == null) return;
        if (currentTaskIndex >= tutorialTasks.Length || taskInProgress) return;

        switch (currentTaskIndex)
        {
            case 0: // Arm the drone
                if (droneController.startupDone)
                    StartCoroutine(CompleteTask());
                break;

            case 1: // Ascend
                if (droneController.finalVertical > 0.1f && !checkpointColliders[0].activeSelf)
                    StartCoroutine(CompleteTask());
                break;

            case 2: // Descend
                if (droneController.finalVertical < -0.1f  && !checkpointColliders[1].activeSelf)
                    StartCoroutine(CompleteTask());
                break;

            case 3: // Pitch forward (↑)
                if (droneController.finalHorizontalZ > 0.1f && !checkpointColliders[2].activeSelf)
                    StartCoroutine(CompleteTask());
                break;

            case 4: // Pitch backward (↓)
                if (droneController.finalHorizontalZ < -0.1f && !checkpointColliders[3].activeSelf)
                    StartCoroutine(CompleteTask());

                break;

            case 5: // Roll left (←)
                if (droneController.finalHorizontalX < -0.1f && !checkpointColliders[4].activeSelf)
                    StartCoroutine(CompleteTask());
                break;

            case 6: // Roll right (→)
                if (droneController.finalHorizontalX > 0.1f && !checkpointColliders[5].activeSelf)
                    StartCoroutine(CompleteTask());
                break;

            case 7: // Yaw clockwise (A)
                if (droneController.finalYaw < -0.1f && droneYawTrail.hasCompletedAnticlockwise==true)
                    StartCoroutine(CompleteTask());
                break;

            case 8: // Yaw anticlockwise (D)
                if (droneController.finalYaw > 0.1f && droneYawTrail.hasCompletedClockwise == true)
                    StartCoroutine(CompleteTask());
                break;
            case 9: // Enter Gimbal Mode 
               // activeTaskCoroutine = StartCoroutine(StartCheckGimbalMode(StartCompleteTask));
                break;
            case 10: //Focus on the Hightloghted Object
               // activeTaskCoroutine = StartCoroutine(StartCheckFocusObject(StartCompleteTask));;
                break;
            case 11: // Start Drone Course
                activeTaskCoroutine = StartCoroutine(StartCheckTrackObjective(StartCompleteTask));
                break;
            case 12: // Practice Mode
                practiceUI.SetActive(true);
                break;
            case 13: // Press Draw Square
                // its called from the button
                break;
            case 14:
                {
                    // Read once to avoid races
                    bool finished = drawSystem.shapeFinished;
                    bool valid = drawSystem.shapeValid;

                    if (finished)
                    {
                        // Reset first so we don't re-enter this branch next frame
                        drawSystem.shapeFinished = false;

                        if (valid)
                        {
                            Debug.Log("✅ Square valid — completing task 14");
                            StartCompleteTask();                // advance to task 15
                        }
                        else
                        {
                            Debug.Log("❌ Invalid shape — back to practice (task 13)");
                            currentTaskIndex = 13;              // retry flow
                        }
                    }
                    break;
                }
               /* if (drawSystem.shapeFinished)
                {
                    if (drawSystem.shapeValid)
                    {
                        Debug.Log("✅ Moving to case 14");
                        StartCoroutine(CompleteTask()); // proceed
                    }
                    else
                    {
                        Debug.Log("❌ Retry drawing square/rectangle");
                        currentTaskIndex = 12; // back to retry
                    }

                    drawSystem.shapeFinished = false; // reset
                }
                break;*/

            case 15: // Land (grounded again)
                if (droneController.inGround && landingPad.isLanding)
                    StartCoroutine(CompleteTask());
                break;
            case 16: // Disarm (turn off)
                if (!droneController.startupDone)
                    StartCoroutine(CompleteTask());
                break;
        }
    }

    private void ActivateCheckpoint(int index)
    {
        // Only activate if not already active
        if (!checkpointColliders[index].activeSelf)
        {
            // Turn off all others first
            for (int i = 0; i < checkpointColliders.Length; i++)
                checkpointColliders[i].SetActive(false);

            checkpointColliders[index].SetActive(true);
        }
    }
    private void StartCompleteTask()
    {
        // Prevent parallel coroutines
        if (isCompletingStep || taskInProgress) return;

        // If a previous waiter is still around, stop it
        if (activeTaskCoroutine != null)
        {
            StopCoroutine(activeTaskCoroutine);
            activeTaskCoroutine = null;
        }

        activeTaskCoroutine = StartCoroutine(CompleteTask());
    }

    private IEnumerator CompleteTask()
    {
        if (isCompletingStep) yield break;   // ✅ prevent double entry

        isCompletingStep = true;

        taskInProgress = true;

        yield return new WaitForSeconds(0.2f);

        // Disable current GIF
        ShowGIFs(-1);

        // Move to next task
        currentTaskIndex++;

        if (currentTaskIndex < tutorialTasks.Length)
        {
            taskText.text = tutorialTasks[currentTaskIndex];
            ShowCurrentTaskGif();

            // --- Activate checkpoint only after task completion ---
            if (currentTaskIndex - 1 >= 0 && currentTaskIndex - 1 < checkpointColliders.Length)
            {
                ActivateCheckpoint(currentTaskIndex - 1);
            }
        }
        else
        {
            if (tutorialUI != null)
                Destroy(tutorialUI);
        }

        taskInProgress = false;

        isCompletingStep = false;

        activeTaskCoroutine = null;
    }

    private IEnumerator StartCheckGimbalMode(Action onComplete)
    {
        yield return new WaitUntil(() => hasEnteredGimbalMode);
        onComplete?.Invoke();
    }
    private IEnumerator StartCheckFocusObject(Action onComplete)
    {
        yield return new WaitUntil(() => hasFocusedObject);
        onComplete?.Invoke();
    }

      private IEnumerator StartCheckTrackObjective(Action onComplete)
      {
          trackCheckpoint.gameObject.SetActive(true);
          yield return new WaitUntil(() => checkpointObjectiveCompleted);
          trackCheckpoint.gameObject.SetActive(false);
          onComplete?.Invoke();
      }

    // ─────────── Checkpoint Hooks ───────────
    private void OnEnable() => OnTrackComplete += CheckpointReached;
    private void OnDisable() => OnTrackComplete -= CheckpointReached;

    public void CheckpointReached()
    {
        if (currentTaskIndex == 11 && !checkpointObjectiveCompleted)
        {
            checkpointObjectiveCompleted = true;
            Debug.Log("Checkpoint Objective Completed!");
        }
        if (currentTaskIndex == 14 && !squareTaskCompleted)
        {
            squareTaskCompleted = true;
            Debug.Log("Square Task Completed!");
        }
    }

    private void ShowCurrentTaskGif()
    {
        if (currentTaskIndex < 0)
        {
            ShowGIFs(-1);
            return;
        }

        if (currentTaskIndex == 9) // Landing → no GIF
        {
            ShowGIFs(-1);
            return;
        }

        if (currentTaskIndex == 16) // Disarm → gif_9
        {
            ShowGIFs(9);
            return;
        }

        if (currentTaskIndex < gifs.Length)
            ShowGIFs(currentTaskIndex);
        else
            ShowGIFs(-1);
    }

    private void ShowGIFs(int index)
    {
        for (int i = 0; i < gifs.Length; i++)
            gifs[i].SetActive(i == index);
    }
    public void OnToggleDrawingPressed()
    {
        if (currentTaskIndex == 13 && !taskInProgress)
        {
            // Ensure we're not in practice mode anymore
            if (practiceUI != null) practiceUI.SetActive(false);

            if (drawSystem != null)
            {
                // Stop any in-progress practice drawing and clear guides
                drawSystem.StopDrawing();
                drawSystem.ClearGuideline();

                // Enter TEST mode (square only) and reset result flags
                drawSystem.isPracticeMode = false;
                drawSystem.selectedShape = DRAWSHAPESTutorial.ShapeType.Square;

                // Clear any leftover result from practice
                drawSystem.shapeFinished = false;
                drawSystem.shapeValid = false;
            }

            IncrementTestAttempts();
            Debug.Log("🎨 Draw Square button pressed, moving to task 14 (validation)!");
            StartCompleteTask(); // 13 -> 14
        }
        /* if (currentTaskIndex == 13 && !taskInProgress && !isCompletingStep)
         {
             IncrementTestAttempts(); // ✅ Count attempt
             Debug.Log("🎨 Draw Square button pressed, completing task 13!");
             StartCoroutine(CompleteTask());
         }*/
    }
    public void OnStartDrawingTest()
    {
        practiceUI.SetActive(false);
        drawSystem.isPracticeMode = false;
        drawSystem.selectedShape = DRAWSHAPESTutorial.ShapeType.Square; // test is always square
        testAttemptCount = 0; // reset for this test
        StartCoroutine(CompleteTask()); // move to next task
    }
    public void OnExitPracticeButton()
    {
        Debug.Log("🚪 Exiting Practice Mode and moving to next task...");

        // Hide all practice-related UI
        if (drawSystem != null)
        {
            drawSystem.startButton.SetActive(false);
            drawSystem.stopButton.SetActive(false);

            foreach (var btn in drawSystem.shapeButtons)
                btn.SetActive(false);

            drawSystem.ClearGuideline();  // hide guideline if any
          //  drawSystem.StopDrawing();     // stop current drawing safely
        }

        if (practiceUI != null)
            practiceUI.SetActive(false);

        // Reset states
        drawSystem.isPracticeMode = false;
        playerSelectedShape = DRAWSHAPESTutorial.ShapeType.None;

        // ✅ Move to next task in tutorial
        StartCoroutine(CompleteTask());

        Debug.Log("✅ Practice exited — moving to next task index.");
    }

    public void OnSelectPracticeShape(int shapeIndex)
    {
        switch (shapeIndex)
        {
            case 0:
                playerSelectedShape = DRAWSHAPESTutorial.ShapeType.Square;
                break;
            case 1:
                playerSelectedShape = DRAWSHAPESTutorial.ShapeType.Circle;
                break;
            case 2:
                playerSelectedShape = DRAWSHAPESTutorial.ShapeType.Figure8;
                break;
        }

        // Tell the draw system which shape to practice
        drawSystem.selectedShape = playerSelectedShape;
        drawSystem.isPracticeMode = true;

        // Optionally, show outline or guideline here
        drawSystem.ShowPracticeOutline();
    }
    public void IncrementTestAttempts()
    {
        testAttemptCount++;
        Debug.Log($"🧮 Drawing Test Attempt: {testAttemptCount}");
    }
}
