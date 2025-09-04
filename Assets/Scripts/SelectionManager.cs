using UnityEngine;
using System;
using DroneSimulator.API.Interfaces;
using DroneSimulator.API;
using DroneSimulator.API.Core;
using System.Threading.Tasks;
using System.Collections;
using DroneSimulator.API.Services;

[RequireComponent(typeof(LogoutManager))]
public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance;

    public EnvironmentOptionSO selectedEnvironment;
    public ScenarioOptionSO selectedScenario;
    public DroneOptionSO selectedDrone;

    public bool isSignIN = false;
    public string currentUserName;
    public bool isPro = false;
    public string userPlan;

    public int activeScreenIndex = 0;

    public Action<int> onScreenIndexChange;

    public IAPICalling apiCalling;

    private bool isLoggingOut = false;
    private bool logoutCompleted = false;
    private bool scenarioEndCompleted = false;
    private float logoutTimeout = 5.0f; // Increased timeout for both APIs


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Register to control application quit process on PC
        Application.wantsToQuit += OnWantsToQuit;
    }

    private void OnDestroy()
    {
        Application.wantsToQuit -= OnWantsToQuit;
    }

    public void UpdateActiveSceneIndex(int index) 
    {
        activeScreenIndex = index;
        onScreenIndexChange?.Invoke(index);
    }

    private bool OnWantsToQuit()
    {
        // Only attempt logout/scenario end if user is signed in and we haven't started process
        if (isSignIN && !isLoggingOut)
        {
            Debug.Log("Application wants to quit - starting end scenario and logout process...");
            isLoggingOut = true;
            logoutCompleted = false;
            scenarioEndCompleted = false;
            
            // Start logout and scenario end coroutine with timeout
            StartCoroutine(LogoutAndEndScenarioWithTimeout());
            
            // Prevent immediate quit to allow APIs to complete
            return false;
        }
        
        // Allow quit if not signed in or process already completed
        return true;
    }

    private IEnumerator LogoutAndEndScenarioWithTimeout()
    {
        Debug.Log("Calling end scenario and logout APIs on application quit...");
        
        // Call end scenario API first (before logout invalidates the token)
        Debug.Log("Step 1: Calling end scenario API...");
        var endScenarioTask = CallEndScenarioAPIAsync();
        
        float elapsedTime = 0f;
        float stepTimeout = 2.5f; // Give each API 2.5 seconds
        
        // Wait for end scenario to complete or timeout
        while (!scenarioEndCompleted && elapsedTime < stepTimeout)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        if (scenarioEndCompleted)
        {
            Debug.Log("End scenario API completed successfully");
        }
        else
        {
            Debug.LogWarning($"End scenario API timed out after {stepTimeout} seconds");
        }
        
        // Step 2: Call logout API after end scenario is done
        Debug.Log("Step 2: Calling logout API...");
        var logoutTask = CallLogoutAPIAsync();
        
        elapsedTime = 0f;
        
        // Wait for logout to complete or timeout
        while (!logoutCompleted && elapsedTime < stepTimeout)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // Handle the final results
        if (logoutCompleted && scenarioEndCompleted)
        {
            Debug.Log("Both end scenario and logout APIs completed successfully before quit");
        }
        else if (scenarioEndCompleted && !logoutCompleted)
        {
            Debug.LogWarning($"End scenario completed but logout timed out after {stepTimeout} seconds");
        }
        else if (!scenarioEndCompleted && logoutCompleted)
        {
            Debug.LogWarning($"Logout completed but end scenario timed out after {stepTimeout} seconds");
        }
        else
        {
            Debug.LogWarning($"Both APIs timed out after {stepTimeout} seconds each");
        }
        
        // Force application quit after API attempts
        Debug.Log("Quitting application...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    private async Task CallLogoutAPIAsync()
    {
        try
        {
            var logoutManager = GetComponent<LogoutManager>();
            
            // Get the login service directly for async control
            var loginService = APIManager.Instance.GetLoginService();
            if (loginService != null)
            {
                await loginService.LogoutAsync();
                logoutCompleted = true;
                Debug.Log("Logout API call completed successfully");
            }
            else
            {
                Debug.LogError("LoginService not available for logout");
                logoutCompleted = true; // Mark as completed to allow quit
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Logout API call failed: {ex.Message}");
            logoutCompleted = true; // Mark as completed even on failure to allow quit
        }
    }

    private async Task CallEndScenarioAPIAsync()
    {
        try
        {
            // Check if there's an active scenario to end
            var scenarioManager = ScenarioManager.Instance;
            if (scenarioManager != null && scenarioManager.IsScenarioActive)
            {
                Debug.Log("Active scenario found, ending scenario session...");
                
                var scenarioService = APIManager.Instance.GetScenarioService();
                if (scenarioService != null)
                {
                    // Use the current scenario settings from ScenarioManager
                    await scenarioService.EndScenarioAsync(
                        scenarioManager.LocationName, 
                        scenarioManager.ScenarioName, 
                        scenarioManager.DroneName
                    );
                    
                    scenarioEndCompleted = true;
                    Debug.Log("End scenario API call completed successfully");
                }
                else
                {
                    Debug.LogError("ScenarioService not available for ending scenario");
                    scenarioEndCompleted = true; // Mark as completed to allow quit
                }
            }
            else
            {
                Debug.Log("No active scenario found, skipping end scenario API call");
                scenarioEndCompleted = true; // Mark as completed since no scenario to end
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"End scenario API call failed: {ex.Message}");
            scenarioEndCompleted = true; // Mark as completed even on failure to allow quit
        }
    }

    private void OnApplicationQuit()
    {
        // Fallback - this will be called if wantsToQuit doesn't work
        if (isSignIN && !isLoggingOut)
        {
            Debug.Log("OnApplicationQuit fallback - attempting end scenario then logout...");
            try
            {
                // Step 1: Quick scenario end attempt if active (before logout)
                var scenarioManager = ScenarioManager.Instance;
                if (scenarioManager != null && scenarioManager.IsScenarioActive)
                {
                    var scenarioService = APIManager.Instance.GetScenarioService();
                    if (scenarioService != null)
                    {
                        Debug.Log("Fallback: Ending scenario session...");
                        // Fire and forget for fallback
                        _ = scenarioService.EndScenarioAsync(
                            scenarioManager.LocationName, 
                            scenarioManager.ScenarioName, 
                            scenarioManager.DroneName
                        );
                    }
                }
                
                // Step 2: Quick logout attempt (after scenario end)
                Debug.Log("Fallback: Logging out...");
                GetComponent<LogoutManager>().OnLogoutButtonClicked();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Fallback end scenario and logout failed: {ex.Message}");
            }
        }
    }
}
