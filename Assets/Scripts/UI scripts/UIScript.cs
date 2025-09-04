using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using DroneSimulator.API.Services;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject environmentMenu;  
    public GameObject scenarioMenu;     
    public GameObject droneMenu;        

    [Header("UI Containers (Parents)")]
    public Transform environmentPanel;  
    public Transform scenarioPanel;     
    public Transform dronePanel;        

    [Header("Button Prefabs")]
    public GameObject environmentButtonPrefab;
    public GameObject scenarioButtonPrefab;
    public GameObject droneButtonPrefab;

    [Header("More Info Panel")]
    public GameObject moreInfoPanel;
    public TextMeshProUGUI moreInfoText;

    [Header("Scriptable Object Data")]
    public List<EnvironmentOptionSO> environments; 

    // Store the currently selected environment.
    private EnvironmentOptionSO currentEnvironment;
    private SelectionManager selectionManager;  

    const string proInfo = "Buy";
    const string availableInfo = "Select";

    const string url = "https://www.dronesimulator.pro/product";



    private void Awake()
    {
        // Find or create SelectionManager
        if (SelectionManager.Instance == null)
        {
            Debug.LogWarning("SelectionManager.Instance is null. Creating a new instance.");
            GameObject selectionManagerObj = new GameObject("SelectionManager");
            selectionManager = selectionManagerObj.AddComponent<SelectionManager>();
            DontDestroyOnLoad(selectionManagerObj);
        }
        else
        {
            selectionManager = SelectionManager.Instance;
        }
    }

    void ShowMoreInfo(string text)
    {
        if (moreInfoPanel != null && moreInfoText != null)
        {
            moreInfoPanel.SetActive(true);
            moreInfoText.text = text;
        }
    }

    void ShowProInfo(string message)
    {
        moreInfoPanel.SetActive(true);
        moreInfoText.text = message;
    }

    public void LocationStart()
    {
        if (environmentMenu == null || scenarioMenu == null || droneMenu == null)
        {
            Debug.LogError("Menu panels are not properly assigned!");
            return;
        }

        // At start, only the environment menu is active.
        environmentMenu.SetActive(true);
        scenarioMenu.SetActive(false);
        droneMenu.SetActive(false);
        PopulateEnvironmentButtons();
    }

    // Clears all children from a UI container.
    void ClearPanel(Transform panel)
    {
        if (panel == null) return;
        
        foreach (Transform child in panel)
        {
            Destroy(child.gameObject);
        }
    }

    // Populate the Environment menu.
    void PopulateEnvironmentButtons()
    {
        selectionManager.UpdateActiveSceneIndex(2);
        if (environmentPanel == null || environmentButtonPrefab == null)
        {
            Debug.LogError("Environment panel or button prefab is not assigned!");
            return;
        }

        ClearPanel(environmentPanel);
        foreach (var env in environments)
        {
            if (env == null) continue;

            GameObject btnObj = Instantiate(environmentButtonPrefab, environmentPanel);
            MenuButtonUI btnUI = btnObj.GetComponent<MenuButtonUI>();
            if (btnUI == null)
            {
                Debug.LogError("MenuButtonUI component not found on button prefab!");
                continue;
            }

            btnUI.headingText.text = env.environmentName;
            btnUI.infoText.text = env.info;
            btnUI.isPro = env.isPro;
            SetupProInfo(btnUI);
            btnUI.button.onClick.AddListener(() => OnEnvironmentSelected(env));
            if (env.image != null)
                btnUI.optionImage.sprite = env.image;
            btnUI.moreButton.onClick.AddListener(() => ShowMoreInfo(env.details));
        }
    }

    void OnEnvironmentSelected(EnvironmentOptionSO env)
    {
        if (!selectionManager.isPro && env.isPro)
        {
            Application.OpenURL(url);
            return;
        }

        if (env == null)
        {
            Debug.LogError("Selected environment is null!");
            return;
        }

        currentEnvironment = env;

        // Save the selection.
        if (selectionManager != null)
        {
            selectionManager.selectedEnvironment = env;
            Debug.Log("Environment Selected: " + env.environmentName);
        }
        else
        {
            Debug.LogError("SelectionManager is not available!");
            return;
        }

        ScenarioManager.Instance.LocationName = env.environmentName;
        // Hide environment menu and show scenario menu.
        ActivateScenario(env);
    }

   

    // Populate the Scenario menu using the Environment's available scenarios.
    void PopulateScenarioButtons(EnvironmentOptionSO env)
    {
        selectionManager.UpdateActiveSceneIndex(3);
        if (scenarioPanel == null || scenarioButtonPrefab == null || env == null)
        {
            Debug.LogError("Scenario panel, button prefab, or environment is not assigned!");
            return;
        }

        ClearPanel(scenarioPanel);
        foreach (var scenario in env.availableScenarios)
        {
            if (scenario == null) continue;

            GameObject btnObj = Instantiate(scenarioButtonPrefab, scenarioPanel);
            MenuButtonUI btnUI = btnObj.GetComponent<MenuButtonUI>();
            if (btnUI == null)
            {
                Debug.LogError("MenuButtonUI component not found on scenario button prefab!");
                continue;
            }

            btnUI.headingText.text = scenario.scenarioName;
            btnUI.infoText.text = scenario.info;
            btnUI.isPro = scenario.isPro;
            SetupProInfo(btnUI);
            btnUI.button.onClick.AddListener(() => OnScenarioSelected(scenario));
            if (scenario.image != null)
                btnUI.optionImage.sprite = scenario.image;
            btnUI.moreButton.onClick.AddListener(() => ShowMoreInfo(scenario.details));
        }
    }

    public void ScenarioSelection(EnvironmentOptionSO env) 
    {
        if (selectionManager.selectedEnvironment != null) 
        {
            ActivateScenario(env);
            return;
        } 
        OnEnvironmentSelected(env);
    }

    private void ActivateScenario(EnvironmentOptionSO env)
    {
        if (environmentMenu != null && scenarioMenu != null)
        {
            environmentMenu.SetActive(false);
            scenarioMenu.SetActive(true);
            PopulateScenarioButtons(env);
        }
    }

    void OnScenarioSelected(ScenarioOptionSO scenario)
    {
        if (!selectionManager.isPro && scenario.isPro)
        {
            Application.OpenURL(url);
            return;
        }

        if (scenario == null)
        {
            Debug.LogError("Selected scenario is null!");
            return;
        }

        // Save the selected scenario.
        if (selectionManager != null)
        {
            selectionManager.selectedScenario = scenario;
            ScenarioManager.Instance.ScenarioName = scenario.name;
            Debug.Log("Scenario Selected: " + scenario.scenarioName);
        }
        else
        {
            Debug.LogError("SelectionManager is not available!");
            return;
        }

        // Hide scenario menu and show drone menu.
        if (scenarioMenu != null && droneMenu != null)
        {
            scenarioMenu.SetActive(false);
            droneMenu.SetActive(true);
            PopulateDroneButtons(currentEnvironment);
        }
    }

    // Populate the Drone menu using the Environment's available drones.
    void PopulateDroneButtons(EnvironmentOptionSO env)
    {

        selectionManager.UpdateActiveSceneIndex(4);
        if (dronePanel == null || droneButtonPrefab == null || env == null)
        {
            Debug.LogError("Drone panel, button prefab, or environment is not assigned!");
            return;
        }

        ClearPanel(dronePanel);
        foreach (var drone in env.availableDrones)
        {
            if (drone == null) continue;

            GameObject btnObj = Instantiate(droneButtonPrefab, dronePanel);
            MenuButtonUI btnUI = btnObj.GetComponent<MenuButtonUI>();
            if (btnUI == null)
            {
                Debug.LogError("MenuButtonUI component not found on drone button prefab!");
                continue;
            }

            btnUI.headingText.text = drone.droneName;
            btnUI.infoText.text = drone.info;
            btnUI.isPro = drone.isPro;
            SetupProInfo(btnUI);

            btnUI.button.onClick.AddListener(() => OnDroneSelected(drone));
            if (drone.image != null)
                btnUI.optionImage.sprite = drone.image;
            btnUI.moreButton.onClick.AddListener(() => ShowMoreInfo(drone.details));
        }
    }

    void OnDroneSelected(DroneOptionSO drone)
    {
        if (!selectionManager.isPro && drone.isPro) 
        {
            Application.OpenURL(url);
            return;
        }


        if (drone == null)
        {
            Debug.LogError("Selected drone is null!");
            return;
        }

        // Save the drone selection.
        if (selectionManager != null)
        {
            selectionManager.selectedDrone = drone;
            Debug.Log("Drone Selected: " + drone.droneName);
            ScenarioManager.Instance.DroneName = drone.droneName;
            // With all selections made, load the scene specified by the selected scenario.
            if (selectionManager.selectedScenario != null)
            {
                SceneManager.LoadScene(selectionManager.selectedScenario.sceneName);
            }
            else
            {
                Debug.LogError("Selected scenario is null!");
            }
        }
        else
        {
            Debug.LogError("SelectionManager is not available!");
        }
    }

    private void SetupProInfo(MenuButtonUI btnUI)
    {
        if (selectionManager.isPro)
        {
            btnUI.button.GetComponentInChildren<TextMeshProUGUI>().text = availableInfo;
        }
        else
        {
            btnUI.button.GetComponentInChildren<TextMeshProUGUI>().text = btnUI.isPro ? proInfo : availableInfo;
        }
    }
}
