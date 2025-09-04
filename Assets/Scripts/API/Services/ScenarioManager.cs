using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using DroneSimulator.API.Interfaces;
using DroneSimulator.API.Models;
using DroneSimulator.API.Core;

namespace DroneSimulator.API.Services
{
    public class ScenarioManager : MonoBehaviour
    {
        [Header("Scenario Settings")]
        [SerializeField] private string locationName = "Location654";
        [SerializeField] private string scenarioName = "Scenario2545";
        [SerializeField] private string droneName = "Drone1df";

        [Header("Auto Management")]
        [SerializeField] private bool autoStartOnSceneLoad = true;
        [SerializeField] private bool autoStopOnSceneUnload = true;
        [SerializeField] private bool autoStopOnReturnToPreviousScene = true;

        private IScenarioService scenarioService;
        private bool scenarioStarted = false;
        private string previousSceneName = "";

        public bool IsScenarioActive => scenarioStarted;

        public string LocationName 
        { 
            get => locationName; 
            set => locationName = value; 
        }
        
        public string ScenarioName 
        { 
            get => scenarioName; 
            set => scenarioName = value; 
        }
        
        public string DroneName 
        { 
            get => droneName; 
            set => droneName = value; 
        }

        private void Awake()
        {
            if (FindObjectsOfType<ScenarioManager>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            APIManager.Instance.InitializeServices();
            
            if (APIServiceLocator.Instance.HasService<IScenarioService>())
            {
                scenarioService = APIServiceLocator.Instance.GetService<IScenarioService>();
                Debug.Log("ScenarioService found and initialized successfully!");
            }
            else
            {
                Debug.LogError("ScenarioService not found! Make sure it's registered in the service locator.");
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            string currentSceneName = scene.name;
            
            if (autoStopOnReturnToPreviousScene && !string.IsNullOrEmpty(previousSceneName) && 
                currentSceneName == previousSceneName && scenarioStarted)
            {
                Debug.Log($"Returning to previous scene: {currentSceneName}. Stopping scenario.");
                _ = StopScenarioAsync();
            }
            else if (autoStartOnSceneLoad && !scenarioStarted)
            {
                Invoke(nameof(StartScenarioDelayed), 0.5f);
            }
            
            previousSceneName = currentSceneName;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (autoStopOnSceneUnload && scenarioStarted)
            {
                Debug.Log($"Scene unloaded: {scene.name}. Stopping scenario.");
                _ = StopScenarioAsync();
            }
        }

        private void StartScenarioDelayed()
        {
            if (this != null && gameObject != null)
            {
                _ = StartScenarioAsync();
            }
        }

        public void UpdateScenarioSettings(string newLocationName, string newScenarioName, string newDroneName)
        {
            locationName = newLocationName;
            scenarioName = newScenarioName;
            droneName = newDroneName;
            
            Debug.Log($"Scenario settings updated: Location={locationName}, Scenario={scenarioName}, Drone={droneName}");
        }

        public void UpdateLocationName(string newLocationName)
        {
            locationName = newLocationName;
            Debug.Log($"Location updated: {locationName}");
        }

        public void UpdateScenarioName(string newScenarioName)
        {
            scenarioName = newScenarioName;
            Debug.Log($"Scenario updated: {scenarioName}");
        }

        public void UpdateDroneName(string newDroneName)
        {
            droneName = newDroneName;
            Debug.Log($"Drone updated: {droneName}");
        }

        private async Task StartScenarioAsync()
        {
            if (scenarioService == null)
            {
                Debug.LogError("Scenario service not available");
                return;
            }

            if (scenarioStarted)
            {
                Debug.LogWarning("Scenario is already active. Skipping start request.");
                return;
            }

            Debug.Log($"Starting scenario: Location={locationName}, Scenario={scenarioName}, Drone={droneName}");
            
            try
            {
                var response = await scenarioService.StartScenarioAsync(locationName, scenarioName, droneName);
                
                if (response != null && response.data != null)
                {
                    scenarioStarted = true;
                    Debug.Log($"Scenario started successfully! ID: {response.data.id}");
                }
                else
                {
                    Debug.LogError("Failed to start scenario: Invalid response");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Scenario start failed: {ex.Message}");
            }
        }

        private async Task StopScenarioAsync()
        {
            if (scenarioService == null)
            {
                Debug.LogError("Scenario service not available");
                return;
            }

            if (!scenarioStarted)
            {
                Debug.LogWarning("No active scenario to stop. Skipping stop request.");
                return;
            }

            Debug.Log($"Stopping scenario: Location={locationName}, Scenario={scenarioName}, Drone={droneName}");
            
            try
            {
                var response = await scenarioService.EndScenarioAsync(locationName, scenarioName, droneName);
                
                if (response != null && response.data != null)
                {
                    scenarioStarted = false;
                    Debug.Log($"Scenario stopped successfully! ID: {response.data.id}");
                }
                else
                {
                    Debug.LogError("Failed to stop scenario: Invalid response");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Scenario stop failed: {ex.Message}");
            }
        }

        public static ScenarioManager Instance
        {
            get
            {
                return FindObjectOfType<ScenarioManager>();
            }
        }
    }
} 