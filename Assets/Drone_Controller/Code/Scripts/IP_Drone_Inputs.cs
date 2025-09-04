using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Xanmine
{
    [RequireComponent(typeof(PlayerInput))]
    public class IP_Drone_Inputs : MonoBehaviour
    {
        #region Variables
        private Vector2 cyclic;
        private float pedals;
        private float throttle;
        private PlayerInput playerInput;
        private Joystick fsEmulator;
        private bool useJoystick = false;
        public TextMeshProUGUI fssimulator;
        public TextMeshProUGUI control;
        #endregion

        public Vector2 Cyclic { get => cyclic; }
        public float Pedals { get => pedals; }
        public float Throttle { get => throttle; }

        #region Main Methods
        void Start()
        {
            playerInput = GetComponent<PlayerInput>();

            // Find and configure FS emulator
            FindFSEmulator();
        }

        void FindFSEmulator()
        {
            foreach (var device in InputSystem.devices)
            {
                if (device.name.Contains("FS") || device.name.Contains("FlySky") || device.name.Contains("Simulator"))
                {
                    //Debug.Log($"Found FS emulator: {device.name}");
                    fssimulator.text = device.name;
                    fsEmulator = device as Joystick;
                    useJoystick = true;
                    break;
                }
            }

            if (fsEmulator == null)
            {
                Debug.LogWarning("FS emulator not found. Using keyboard controls");
            }
        }

        void Update()
        {
            if (useJoystick && fsEmulator != null)
            {
                // Read from FS emulator
                // Map the "stick" to cyclic
                cyclic.x = fsEmulator.stick.x.ReadValue();
                cyclic.y = fsEmulator.stick.y.ReadValue();

                // Map "z" to pedals
                pedals = fsEmulator.GetChildControl("rx") is AxisControl zAxis ?
                    zAxis.ReadValue() : 0f;

                // Map "rx" to throttle
                throttle = fsEmulator.GetChildControl("z") is AxisControl rxAxis ?
                    rxAxis.ReadValue() : 0f;

                //Debug.Log("Using FS emulator: cyclic-" + cyclic + ", pedals-" + pedals + ", throttle-" + throttle);
                control.text = "Using FS emulator: cyclic-" + cyclic + ", pedals-" + pedals + ", throttle-" + throttle;

            }
            else
            {
                // Keep the existing keyboard input through PlayerInput
                //Debug.Log("Using keyboard: cyclic-" + cyclic + ", pedals-" + pedals + ", throttle-" + throttle);
                control.text = "Using FS emulator: cyclic-" + cyclic + ", pedals-" + pedals + ", throttle-" + throttle;
            }
        }
        #endregion

        #region Input Methods
        // These will still work when using keyboard through PlayerInput
        private void OnCyclic(InputValue value)
        {
            if (!useJoystick)
                cyclic = value.Get<Vector2>();
        }

        private void OnPedals(InputValue value)
        {
            if (!useJoystick)
                pedals = value.Get<float>();
        }

        private void OnThrottle(InputValue value)
        {
            if (!useJoystick)
                throttle = value.Get<float>();
        }
        #endregion
    }
}