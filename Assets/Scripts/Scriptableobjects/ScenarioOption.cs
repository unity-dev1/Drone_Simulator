using UnityEngine;

[CreateAssetMenu(fileName = "NewScenarioOption", menuName = "Menu/Scenario Option")]
public class ScenarioOptionSO : ScriptableObject
{
    public int index;
    public Sprite image;             // The image for this option (and for its scenarios)
    public string scenarioName;  // e.g., "Free Flight"
    public string info;          // Info text for the scenario
    public string sceneName;     // The scene to load for this scenario
    public string details; // Detailed info for popup
    public bool isPro;
}
