using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;

public class RebindManager : MonoBehaviour
{
    [System.Serializable]
    public class RebindEntry
    {
        public string actionName;
        public InputActionReference actionReference;
        public Button rebindButton;
        public int bindingIndex = -1; // Optional: for composite parts like 1DAxis Positive/Negative
    }

    public RebindEntry[] entries;

    private void Start()
    {
        foreach (var entry in entries)
        {
            var action = entry.actionReference.action;
            int bindingIndex = entry.bindingIndex >= 0 ? entry.bindingIndex : GetFirstNonCompositeBindingIndex(action);

            // Load saved binding override if it exists
            string savedOverride = PlayerPrefs.GetString(action.name + bindingIndex, null);
            if (!string.IsNullOrEmpty(savedOverride) && bindingIndex != -1)
            {
                action.ApplyBindingOverride(bindingIndex, savedOverride);
            }

            UpdateBindingDisplay(entry);
            entry.rebindButton.onClick.AddListener(() => StartRebinding(entry));
        }
    }

    void UpdateBindingDisplay(RebindEntry entry)
    {
        var label = entry.rebindButton.GetComponentInChildren<TextMeshProUGUI>();
        var action = entry.actionReference.action;

        int bindingIndex = entry.bindingIndex >= 0 ? entry.bindingIndex : GetFirstNonCompositeBindingIndex(action);
        if (bindingIndex == -1) return;

        label.text = InputControlPath.ToHumanReadableString(
            action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice
        );
    }

    void StartRebinding(RebindEntry entry)
    {
        var action = entry.actionReference.action;
        int bindingIndex = entry.bindingIndex >= 0 ? entry.bindingIndex : GetFirstNonCompositeBindingIndex(action);

        if (bindingIndex == -1 || action.bindings[bindingIndex].isComposite)
        {
            Debug.LogWarning("Can't rebind this action directly. It may be a composite.");
            return;
        }

        var label = entry.rebindButton.GetComponentInChildren<TextMeshProUGUI>();
        label.text = "...";

        bool wasEnabled = action.enabled;
        if (wasEnabled)
        {
            action.Disable(); // 🔐 Disable before rebinding
        }

        action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(op =>
            {
                op.Dispose();

                PlayerPrefs.SetString(action.name + bindingIndex, action.bindings[bindingIndex].overridePath);
                PlayerPrefs.Save();

                UpdateBindingDisplay(entry);

                if (wasEnabled)
                {
                    action.Enable(); // ✅ Re-enable after rebinding
                }
            })
            .Start();
    }


    int GetFirstNonCompositeBindingIndex(InputAction action)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (!action.bindings[i].isComposite && !action.bindings[i].isPartOfComposite)
                return i;
        }
        return -1;
    }

    public void ResetAllBindings()
    {
        foreach (var entry in entries)
        {
            var action = entry.actionReference.action;
            int bindingIndex = entry.bindingIndex >= 0 ? entry.bindingIndex : GetFirstNonCompositeBindingIndex(action);
            if (bindingIndex >= 0)
            {
                action.RemoveBindingOverride(bindingIndex);
                PlayerPrefs.DeleteKey(action.name + bindingIndex);
                UpdateBindingDisplay(entry);
            }
        }
        PlayerPrefs.Save();
    }
}
