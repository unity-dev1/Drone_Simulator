using UnityEngine;
using UnityEngine.Events;

public class CameraSwitcher : MonoBehaviour
{
    public Unity.Cinemachine.CinemachineCamera[] cameras; // assign all 6 in Inspector
    private int currentIndex = -1;

    public UnityEvent<bool> onThermalEffect;

    public PCReceiver pcReceiver;

    void Update()
    {
        if (pcReceiver.UseAndroidInput)
        {
            SwitchToCamera(pcReceiver.selectedCamValue);
        }
        else 
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToCamera(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToCamera(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToCamera(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchToCamera(3);
            if (Input.GetKeyDown(KeyCode.Alpha5)) SwitchToCamera(4);
            if (Input.GetKeyDown(KeyCode.Alpha6)) SwitchToCamera(5);
            if (Input.GetKeyDown(KeyCode.Alpha7)) SwitchToCamera(6);
            if (Input.GetKeyDown(KeyCode.Alpha8)) SwitchToCamera(7);
        }
    }

    void SwitchToCamera(int index)
    {
        if (currentIndex == index) return;

        

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].Priority = (i == index) ? 10 : 0; // set high priority to the active one
        }

        currentIndex = index;

        onThermalEffect?.Invoke(currentIndex == 7);
    }
}

