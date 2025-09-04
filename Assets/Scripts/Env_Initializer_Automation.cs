using UnityEngine;
using UnityEngine.UI;

public class Env_Initializer_Automation : MonoBehaviour
{
    [Header("Environment Objects")]
    public GameObject agricultureEnvironment;
    public GameObject schoolGroundEnvironment;
    public GameObject militaryEnvironment;
    public GameObject hvLineEnvironment;
    public GameObject cityEnvironment;
    public GameObject bridgeEnvironment;

    [Header("Drone Objects")]
    public GameObject DJIM350Drone;
    public GameObject agricultureDrone;
    public GameObject racingDrone;
    public GameObject fighterDrone;
    public GameObject crystalballDrone;
    public GameObject marvicDrone;

    [Header("Background Images")]
    public GameObject groundBackground;
    public GameObject agriBackground;
    public GameObject militaryBackground;
    public GameObject solarPanelBackground;
    public GameObject hvLinesBackground;
    public GameObject cityBackground;
    public GameObject bridgeBackground;

    [Header("Camera Parents")]
    public Transform viewCameras;
    public Transform groundCameras;
    public Transform agricultureCameras;
    public Transform militaryCameras;
    public Transform hvLinesCameras;
    public Transform cityCameras;
    public Transform bridgeCameras;

    [Header("Scenario Targets")]
    public int defaultenvIndex;
    public int defaultdroneIndex;

    public Vector3 groundPos = new Vector3(-110, 189, 42.3f);
    public Vector3 groundScale = new Vector3(2.1f, 2.1f, 2.1f);
    public float groundaltMin = 5f;
    public float groundaltmax = 20f;

    public Vector3 agriPos = new Vector3(-102, 397.6f, -107.5f);
    public Vector3 agriScale = new Vector3(4.4f, 4.4f, 4.4f);
    public float agrialtMin = 5f;
    public float agrialtMax = 20f;

    public Vector3 militaryPos = new Vector3(100f, 300f, -150f);
    public Vector3 militaryScale = new Vector3(3f, 3f, 3f);
    public float milaltMin = 0f;
    public float milatMax = 50f;

    public Vector3 hvLinesPos = new Vector3(100f, 300f, -150f);
    public Vector3 hvLinesScale = new Vector3(3f, 3f, 3f);
    public float hvLinesMin = 0f;
    public float hvLinesMax = 50f;

    public Vector3 cityPos = new Vector3(100f, 300f, -150f);
    public Vector3 cityScale = new Vector3(3f, 3f, 3f);
    public float cityMin = 0f;
    public float cityMax = 50f;

    public Vector3 bridgePos = new Vector3(100f, 300f, -150f);
    public Vector3 bridgeScale = new Vector3(3f, 3f, 3f);
    public float bridgeMin = 0f;
    public float bridgeMax = 50f;

    [Header("Scenario Targets")]
    public HatchPattern hp;
    public HatchPattern hvLinePattern;

    void Start()
    {
        Debug.Log("Env_Initializer_Automation Start method called");

        if (SelectionManager.Instance == null)
        {
            Debug.LogWarning("SelectionManager.Instance is null. Using default values.");
            InitializeWithDefaults();
            return;
        }

        Debug.Log(SelectionManager.Instance);

        // Use indices instead of names
        int envIndex = SelectionManager.Instance?.selectedEnvironment?.index ?? defaultenvIndex;
        int droneIndex = SelectionManager.Instance?.selectedDrone?.index ?? defaultdroneIndex;

        Debug.Log($"Environment Index: {envIndex}");
        Debug.Log($"Drone Index: {droneIndex}");

        if (hp == null)
        {
            Debug.LogError("HatchPattern reference is null. Cannot continue initialization.");
            return;
        }

        // 0 = Ground, 1 = Agriculture, 2 = Defense Zone
        if (envIndex == 0)
        {
            Debug.Log("Ground");
            hp.ground = schoolGroundEnvironment;
            hp.dronePos = groundPos;
            hp.droneScale = groundScale;
            if (groundBackground) groundBackground.SetActive(true);
            hp.altitude = groundaltMin;
            hp.altitudemaxValue = groundaltmax;
        }
        else if (envIndex == 1)
        {
            Debug.Log("Agriculture");
            hp.ground = agricultureEnvironment;
            hp.dronePos = agriPos;
            hp.droneScale = agriScale;
            if (agriBackground) agriBackground.SetActive(true);
            hp.altitude = agrialtMin;
            hp.altitudemaxValue = agrialtMax;
        }
        else if (envIndex == 2)
        {
            Debug.Log("Defense Zone");
            hp.ground = militaryEnvironment;
            hp.dronePos = militaryPos;
            hp.droneScale = militaryScale;
            if (militaryBackground) militaryBackground.SetActive(true);
            hp.altitude = milaltMin;
            hp.altitudemaxValue = milatMax;
        }
        else if (envIndex == 3)
        {
            Debug.Log("HV Lines Zone");


            if (SelectionManager.Instance.selectedScenario.index == 3)
            {
                hp.ground = hvLineEnvironment;
                hp.dronePos = hvLinesPos;
                hp.droneScale = hvLinesScale;
                hp.altitude = hvLinesMin;
                hp.altitudemaxValue = hvLinesMax;
                if (solarPanelBackground) solarPanelBackground.SetActive(true);
            }

            if (SelectionManager.Instance.selectedScenario.index == 4)
            {
                hvLinePattern.ground = hvLineEnvironment;
                hvLinePattern.dronePos = hvLinesPos;
                hvLinePattern.droneScale = hvLinesScale;
                if (hvLinesBackground) hvLinesBackground.SetActive(true);
                hvLinePattern.altitude = hvLinesMin;
                hvLinePattern.altitudemaxValue = hvLinesMax;
            }
        }
        else if (envIndex == 5)
        {
            Debug.Log("Railway Zone");
            hvLinePattern.ground = bridgeEnvironment;
            hvLinePattern.dronePos = bridgePos;
            hvLinePattern.droneScale = bridgeScale;
            if (bridgeBackground) bridgeBackground.SetActive(true);
            hvLinePattern.altitude = bridgeMin;
            hvLinePattern.altitudemaxValue = bridgeMax;
        }
        else if (envIndex == 6)
        {
            Debug.Log("City");
            hp.ground = cityEnvironment;
            hp.dronePos = cityPos;
            hp.droneScale = cityScale;
            if (cityBackground) cityBackground.SetActive(true);
            hp.altitude = cityMin;
            hp.altitudemaxValue = cityMax;
        }

        // 0 = Agriculture, 1 = DJI MATRICE 350 RTK (assuming from your numbering)
        if (DJIM350Drone) DJIM350Drone.SetActive(droneIndex == 1);
        if (agricultureDrone) agricultureDrone.SetActive(droneIndex == 0);
        if (racingDrone) racingDrone.SetActive(droneIndex == 2);
        if (fighterDrone) fighterDrone.SetActive(droneIndex == 3);
        if (crystalballDrone) crystalballDrone.SetActive(droneIndex == 4);
        if (marvicDrone) marvicDrone.SetActive(droneIndex == 5);

        switch (droneIndex)
        {
            case 0:
                if (SelectionManager.Instance.selectedScenario.index == 4 ||
                    SelectionManager.Instance.selectedEnvironment.index == 5)
                {
                    hvLinePattern.drone = agricultureDrone;
                }
                else
                {
                    hp.drone = agricultureDrone;
                }
                break;
            case 1:
                if (SelectionManager.Instance.selectedScenario.index == 4 ||
                    SelectionManager.Instance.selectedEnvironment.index == 5)
                {
                    hvLinePattern.drone = DJIM350Drone;
                }
                else
                {
                    hp.drone = DJIM350Drone;
                }

                break;
            case 2:
                if (SelectionManager.Instance.selectedScenario.index == 4 ||
                    SelectionManager.Instance.selectedEnvironment.index == 5)
                {
                    hvLinePattern.drone = racingDrone;
                }
                else
                {
                    hp.drone = racingDrone;
                }
                break;
            case 3:
                if (SelectionManager.Instance.selectedScenario.index == 4 ||
                    SelectionManager.Instance.selectedEnvironment.index == 5)
                {
                    hvLinePattern.drone = fighterDrone;

                }
                else
                {
                    hp.drone = fighterDrone;

                }
                break;
            case 4:
                if (SelectionManager.Instance.selectedScenario.index == 4 ||
                    SelectionManager.Instance.selectedEnvironment.index == 5)
                {
                    hvLinePattern.drone = crystalballDrone;

                }
                else
                {
                    hp.drone = crystalballDrone;

                }
                break;
            case 5:
                if (SelectionManager.Instance.selectedScenario.index == 4 ||
                    SelectionManager.Instance.selectedEnvironment.index == 5)
                {
                    hvLinePattern.drone = marvicDrone;

                }
                else
                {
                    hp.drone = marvicDrone;
                }
                break;
        }

        // Set camera views using index
        UpdateViewCameras(envIndex);
    }

    private void InitializeWithDefaults()
    {
        Debug.Log("Initializing with default values");

        if (hp == null)
        {
            Debug.LogError("HatchPattern reference is null. Cannot continue initialization.");
            return;
        }

        if (defaultenvIndex == 0)
        {
            Debug.Log("Ground");
            hp.ground = schoolGroundEnvironment;
            hp.dronePos = groundPos;
            hp.droneScale = groundScale;
            if (groundBackground) groundBackground.SetActive(true);
            hp.altitude = groundaltMin;
            hp.altitudemaxValue = groundaltmax;
        }
        else if (defaultenvIndex == 1)
        {
            Debug.Log("Agriculture");
            hp.ground = agricultureEnvironment;
            hp.dronePos = agriPos;
            hp.droneScale = agriScale;
            if (agriBackground) agriBackground.SetActive(true);
            hp.altitude = agrialtMin;
            hp.altitudemaxValue = agrialtMax;
        }
        else if (defaultenvIndex == 2)
        {
            Debug.Log("Military Zone");
            hp.ground = militaryEnvironment;
            hp.dronePos = militaryPos;
            hp.droneScale = militaryScale;
            if (militaryBackground) militaryBackground.SetActive(true);
            hp.altitude = milaltMin;
            hp.altitudemaxValue = milatMax;
        }
        else if (defaultenvIndex == 3)
        {
            Debug.Log("HvLines Zone");
            hp.ground = hvLineEnvironment;
            hp.dronePos = hvLinesPos;
            hp.droneScale = hvLinesScale;

            if (SelectionManager.Instance.selectedScenario.index == 3)
            {
                if (solarPanelBackground) solarPanelBackground.SetActive(true);
            }

            if (SelectionManager.Instance.selectedScenario.index == 4)
            {
                if (hvLinesBackground) hvLinesBackground.SetActive(true);
            }

            hp.altitude = hvLinesMin;
            hp.altitudemaxValue = hvLinesMax;
        }
        else if (defaultenvIndex == 5)
        {
            Debug.Log("Railway Zone");
            hp.ground = bridgeEnvironment;
            hp.dronePos = bridgePos;
            hp.droneScale = bridgeScale;
            if (bridgeBackground) bridgeBackground.SetActive(true);
            hp.altitude = bridgeMin;
            hp.altitudemaxValue = bridgeMax;
        }
        else if (defaultenvIndex == 6)
        {
            Debug.Log("City Env");
            hp.ground = cityEnvironment;
            hp.dronePos = cityPos;
            hp.droneScale = cityScale;
            if (cityBackground) cityBackground.SetActive(true);
            hp.altitude = cityMin;
            hp.altitudemaxValue = cityMax;
        }

        // 0 = Agriculture, 1 = DJI MATRICE 350 RTK
        if (DJIM350Drone) DJIM350Drone.SetActive(defaultdroneIndex == 1);
        if (agricultureDrone) agricultureDrone.SetActive(defaultdroneIndex == 0);

        if (racingDrone) racingDrone.SetActive(defaultdroneIndex == 2);
        if (fighterDrone) fighterDrone.SetActive(defaultdroneIndex == 3);
        if (crystalballDrone) crystalballDrone.SetActive(defaultdroneIndex == 4);
        if (marvicDrone) marvicDrone.SetActive(defaultdroneIndex == 5);

        if (defaultdroneIndex == 0)
        {
            hp.drone = agricultureDrone;
        }
        else
        {
            hp.drone = DJIM350Drone;
        }

        // Set camera views using index
        UpdateViewCameras(defaultenvIndex);
    }

    void UpdateViewCameras(int environmentIndex)
    {
        Debug.Log($"UpdateViewCameras called with index: {environmentIndex}");

        Transform sourceParent = null;

        // 0 = Ground, 1 = Agriculture, 2 = Defense Zone
        if (environmentIndex == 1 && agricultureCameras != null)
            sourceParent = agricultureCameras;
        else if (environmentIndex == 0 && groundCameras != null)
            sourceParent = groundCameras;
        else if (environmentIndex == 2 && militaryCameras != null)
            sourceParent = militaryCameras;
        else if (environmentIndex == 3 && hvLinesCameras != null)
            sourceParent = hvLinesCameras;
        else if (environmentIndex == 5 && bridgeCameras != null)
            sourceParent = bridgeCameras;
        else if (environmentIndex == 6 && cityCameras != null)
            sourceParent = cityCameras;

        if (sourceParent == null || viewCameras == null)
        {
            Debug.LogWarning($"Missing camera source or view camera root. sourceParent: {(sourceParent != null ? sourceParent.name : "null")}, viewCameras: {(viewCameras != null ? viewCameras.name : "null")}");
            return;
        }

        int updatedCount = 0;
        foreach (Transform sourceChild in sourceParent)
        {
            if (sourceChild == null) continue;

            Transform targetChild = viewCameras.Find(sourceChild.name);
            if (targetChild != null)
            {
                targetChild.position = sourceChild.position;
                targetChild.rotation = sourceChild.rotation;
                updatedCount++;
            }
            else
            {
                Debug.LogWarning($"Could not find matching camera '{sourceChild.name}' in viewCameras");
            }
        }

        Debug.Log($"Updated {updatedCount} camera positions and rotations");
    }
}