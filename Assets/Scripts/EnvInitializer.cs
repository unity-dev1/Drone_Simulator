using System;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("Environment Objects")]
    public GameObject agricultureEnvironment;
    public GameObject schoolGroundEnvironment;
    public GameObject militaryBaseEnvironment;
    public GameObject HVLineSolarPanelEnvironment;
    public GameObject factoryEnvironment;
    public GameObject bridgeRoadEnvironment;
    public GameObject cityEnvironment;

    [Header("Drone Objects")]
    public GameObject djim350Drone;
    public GameObject agricultureDrone;
    public GameObject racingDrone;
    public GameObject fighterDrone;
    public GameObject crystalballDrone;
    public GameObject marvicDrone;

    [Header("Camera Parents")]
    public Transform viewCameras;
    public Transform groundCameras;
    public Transform agricultureCameras;
    public Transform militaryBaseCameras;
    public Transform hvLinesCameras;
    public Transform factoryCameras;
    public Transform bridgeRoadCameras;
    public Transform cityCameras;

    [Header("Scenario Targets")]
    public int defaultenvIndex = 0;
    public int defaultdroneIndex = 0;

    [Header("Scenario Targets")]
    public MonoBehaviour shapesScript;

    void Start()
    {
        Debug.Log(SelectionManager.Instance);

        // Use indices instead of names
        int envIndex = SelectionManager.Instance?.selectedEnvironment?.index ?? defaultenvIndex; // Default to Defense Zone (2)
        int droneIndex = SelectionManager.Instance?.selectedDrone?.index ?? defaultdroneIndex; // Default to Agriculture (0)
        string scenarioName = SelectionManager.Instance?.selectedScenario?.scenarioName ?? "Shapes"; // Keep scenarios as names

        Debug.Log($"Environment Index: {envIndex}");
        Debug.Log($"Drone Index: {droneIndex}");
        Debug.Log($"Scenario: {scenarioName}");

        // Activate environments based on index
        // 0 = Ground, 1 = Agriculture, 2 = Defense Zone
        agricultureEnvironment.SetActive(envIndex == 1);
        schoolGroundEnvironment.SetActive(envIndex == 0);
        militaryBaseEnvironment.SetActive(envIndex == 2);
        HVLineSolarPanelEnvironment.SetActive(envIndex == 3);
        factoryEnvironment.SetActive(envIndex == 4);
        bridgeRoadEnvironment.SetActive(envIndex == 5);
        cityEnvironment.SetActive(envIndex == 6);

        // Activate drone based on index
        // 0 = Agriculture, 1 = DJI MATRICE 350 RTK
        djim350Drone.SetActive(droneIndex == 1);
        agricultureDrone.SetActive(droneIndex == 0);
        racingDrone.SetActive(droneIndex == 2);
        fighterDrone.SetActive(droneIndex == 3);
        crystalballDrone.SetActive(droneIndex == 4);
        marvicDrone.SetActive(droneIndex == 5);

        // Set camera views using index
        UpdateViewCameras(envIndex);

        // Activate scenario script - keeping scenario as string name as requested
        if (scenarioName == "Shapes" && shapesScript != null)
        {
            shapesScript.enabled = true;
        }
    }

    void UpdateViewCameras(int environmentIndex)
    {
        Transform sourceParent = null;

        // 0 = Ground, 1 = Agriculture, 2 = Defense Zone (Military Base)
        if (environmentIndex == 1)
            sourceParent = agricultureCameras;
        else if (environmentIndex == 0)
            sourceParent = groundCameras;
        else if (environmentIndex == 2)
            sourceParent = militaryBaseCameras;
        else if (environmentIndex == 3)
            sourceParent = hvLinesCameras;
        else if (environmentIndex == 4)
            sourceParent = factoryCameras;
        else if (environmentIndex == 5)
            sourceParent = bridgeRoadCameras;
        else if (environmentIndex == 6)
            sourceParent = cityCameras;

        if (sourceParent == null || viewCameras == null)
        {
            Debug.LogWarning($"Missing camera source or view camera root for environment index: {environmentIndex}");
            return;
        }

        foreach (Transform sourceChild in sourceParent)
        {
            Transform targetChild = viewCameras.Find(sourceChild.name);
            if (targetChild != null)
            {
                targetChild.position = sourceChild.position;
                targetChild.rotation = sourceChild.rotation;
            }
        }
    }
}