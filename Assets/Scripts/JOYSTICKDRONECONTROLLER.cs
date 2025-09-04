using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class FSJoystickInput : MonoBehaviour
{
    public bool UseJoystick { get; private set; } = false;
    public Joystick Emulator { get; private set; }

    private Vector2 cyclic = Vector2.zero;
    public Vector2 Cyclic => cyclic;

    public float Pedals { get; private set; } = 0f;
    public float Throttle { get; private set; } = 0f;

    public bool DrawingButtonPressed { get; private set; } = true;
    public bool SprayButtonPressed { get; private set; } = false;

    public string DeviceName => (UseJoystick && Emulator != null) ? Emulator.name : ( FindAnyObjectByType<PCReceiver>().UseAndroidInput ? "Mobile Controller":"No Device Connected"); //Emulator.name in case you need name

    private float joystickConnectedTime = -1f;
    private const float inputDelayAfterConnect = 0.5f;


    public void Initialize()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        FindEmulator();
    }

    public void Cleanup()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private bool prevButton7State = false;
    private bool prevButton8State = false;

    public void ReadInput()
    {
        if (UseJoystick && Emulator != null)
        {
            // Wait for 0.5s after connection
            if (Time.time - joystickConnectedTime < inputDelayAfterConnect)
                return;

            float x = Emulator.stick.x.ReadValue();
            float y = Emulator.stick.y.ReadValue();
            cyclic = new Vector2(x, y);

            Pedals = Emulator.TryGetChildControl("rx") is AxisControl rx ? rx.ReadValue() : 0f;
            Throttle = Emulator.TryGetChildControl("z") is AxisControl z ? z.ReadValue() : 0f;

            if (Emulator.TryGetChildControl("trigger") is ButtonControl b7 && (b7.wasPressedThisFrame || b7.wasReleasedThisFrame))
            {
                Debug.Log("Button trigger Pressed");
                DrawingButtonPressed = !DrawingButtonPressed;
            }

            if (Emulator.TryGetChildControl("button6") is ButtonControl b8 && (b8.wasPressedThisFrame || b8.wasReleasedThisFrame))
            {
                Debug.Log("Button 6 Pressed");
                SprayButtonPressed = !SprayButtonPressed;
            }
        }
        else
        {
            cyclic = Vector2.zero;
            Pedals = 0f;
            Throttle = 0f;
            DrawingButtonPressed = false;
            SprayButtonPressed = false;
        }
    }




    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if ((change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected) &&
            !UseJoystick &&
            (device.name.Contains("FS") || device.name.Contains("FlySky") || device.name.Contains("Simulator")))
        {
            Emulator = device as Joystick;
            if (Emulator != null)
            {
                UseJoystick = true;
                joystickConnectedTime = Time.time;  // mark connect time
                Debug.Log($"Joystick connected: {Emulator.name}");
            }
        }

        if ((change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected) && device == Emulator)
        {
            Emulator = null;
            UseJoystick = false;
            Debug.Log("Joystick disconnected.");
        }
    }


    private void CheckAndConfigureDevice(InputDevice device)
    {
        if (!UseJoystick && (device.name.Contains("FS") || device.name.Contains("FlySky") || device.name.Contains("Simulator")))
        {
            Joystick potential = device as Joystick;
            if (potential != null)
            {
                Emulator = potential;
                UseJoystick = true;
                Debug.Log($"Joystick connected: {device.name}");
            }
        }
    }

    private void FindEmulator()
    {
        foreach (var device in InputSystem.devices)
        {
            CheckAndConfigureDevice(device);
            if (UseJoystick) break;
        }

        if (!UseJoystick)
            Debug.LogWarning("No joystick detected. Keyboard control active.");
    }
}
