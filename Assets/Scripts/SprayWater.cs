using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WaterSprayToggle : MonoBehaviour
{
    public DRONECONT dc;
    public List<ParticleSystem> spraySystems; // Particle systems to control
    public Button sprayToggleButton;          // UI Button
    public Image buttonImage;                 // Button Image component
    public Sprite sprayOnSprite;              // Sprite when spraying
    public Sprite sprayOffSprite;             // Sprite when not spraying
    public GameObject sprayUI;
    public GameObject SpraytankUI;

    public bool isSpraying = false;
    public bool shouldSpray = false;
    public bool sprayEnabled = false;
    public InputActionAsset droneInput;
    public FSJoystickInput joystickInput;


    void Start()
    {
        droneInput.Enable();
        sprayEnabled = true;

        if (sprayToggleButton != null)
            sprayToggleButton.onClick.AddListener(ToggleSpray);

        UpdateButtonSprite();
        if(SpraytankUI != null) SpraytankUI.SetActive(true);
        sprayUI.SetActive(true);

        Debug.Log("spray enabled");
    }

    // Add these variables at the class level
    // Add these variables at the class level
    private bool previousSprayButtonState = false;

    void Update()
    {
        // Handle keyboard input - Unity's input system already handles the toggle correctly
        if (droneInput.FindAction("spray").triggered)
        {
            ToggleSpray();
        }

        // Handle joystick input - toggle on state changes (both on->off and off->on)
        if (joystickInput != null)
        {
            // If the button state changed (either direction), toggle spray
            if (joystickInput.SprayButtonPressed != previousSprayButtonState)
            {
                ToggleSpray();
            }

            // Update previous state for next frame
            previousSprayButtonState = joystickInput.SprayButtonPressed;
        }
    }


    public void ToggleSpray()
    {
        if(!shouldSpray || !sprayEnabled) return ;
        isSpraying = !isSpraying;

        foreach (var ps in spraySystems)
        {
            if (isSpraying)
                ps.Play();
            else
                ps.Stop();
        }

        UpdateButtonSprite();
    }

   public void hideSpray()
    {
        if (SpraytankUI != null) SpraytankUI.SetActive(false);
        sprayUI.SetActive(false);
    }

    void UpdateButtonSprite()
    {
        if (buttonImage != null)
            buttonImage.sprite = isSpraying ? sprayOnSprite : sprayOffSprite;
    }
}
