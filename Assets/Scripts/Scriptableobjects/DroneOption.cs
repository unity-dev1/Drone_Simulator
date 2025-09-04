using UnityEngine;

[CreateAssetMenu(fileName = "NewDroneOption", menuName = "Menu/Drone Option")]
public class DroneOptionSO : ScriptableObject
{
    public int index;
    public string droneName;  // e.g., "DroneA"
    public string info;       // Extra info to display on the button
    public Sprite image;      // The image for this option
    public string details; // Detailed info for popup
    public bool isPro;
}
