using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEnvironmentOption", menuName = "Menu/Environment Option")]
public class EnvironmentOptionSO : ScriptableObject
{
    public int index;
    public string environmentName;   // e.g., "Agriculture"
    public string info;              // Info text for the environment
    public Sprite image;             // The image for this option (and for its scenarios)
    public List<ScenarioOptionSO> availableScenarios; // Scenarios for this environment
    public List<DroneOptionSO> availableDrones;       // Drones available for this environment
    public string details; // Detailed info for popup.

    public float hatchMinX;
    public float hatchMaxX;
    public float hatchMinY;
    public float hatchMaxY;

    public bool isPro;

}
