// SettingsMenu.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider audioSlider;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown displayModeDropdown;
    public TMP_Dropdown metricSystemDropdown;

    Resolution[] resolutions;

    void Start()
    {
        // Audio
        audioSlider.onValueChanged.AddListener(SetVolume);

        // Resolution
        resolutions = new Resolution[]
        {
            new Resolution { width = 1920, height = 1080 },
            new Resolution { width = 1600, height = 900 },
            new Resolution { width = 1366, height = 768 },
            new Resolution { width = 1280, height = 720 },
            new Resolution { width = 1024, height = 576 }
        };

        resolutionDropdown.ClearOptions();
        foreach (var res in resolutions)
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(res.width + " x " + res.height));

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        displayModeDropdown.onValueChanged.AddListener(SetDisplayMode);
        metricSystemDropdown.onValueChanged.AddListener(SetMetricSystem);

        // Load saved settings (optional)
        audioSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
        resolutionDropdown.value = PlayerPrefs.GetInt("Resolution", 0);
        displayModeDropdown.value = PlayerPrefs.GetInt("DisplayMode", 0);
        metricSystemDropdown.value = PlayerPrefs.GetInt("MetricSystem", 0);
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("Volume", value);
    }

    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        FullScreenMode mode = (displayModeDropdown.value == 0) ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        Screen.SetResolution(res.width, res.height, mode);
        PlayerPrefs.SetInt("Resolution", index);
    }


    public void SetDisplayMode(int index)
    {
        Resolution res = resolutions[resolutionDropdown.value];
        FullScreenMode mode = (index == 0) ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        Screen.SetResolution(res.width, res.height, mode);
        PlayerPrefs.SetInt("DisplayMode", index);
    }


    public void SetMetricSystem(int index)
    {
        PlayerPrefs.SetInt("MetricSystem", index);
        // 0 = Metric, 1 = Imperial. Use this wherever needed.
    }
}
