using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniSettings : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider volumeSlider;
    public TMP_Dropdown metricDropdown;
    public AudioSource targetAudioSource;

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("Volume", 1f);
        int savedMetric = PlayerPrefs.GetInt("MetricSystem", 0);

        if (targetAudioSource != null)
        {
            Debug.Log($"Audio Source Status - Is Playing: {targetAudioSource.isPlaying}, " +
                     $"Has Clip: {targetAudioSource.clip != null}, " +
                     $"Mute: {targetAudioSource.mute}, " +
                     $"Enabled: {targetAudioSource.enabled}");

            if (targetAudioSource.clip != null)
            {
                if (!targetAudioSource.isPlaying && savedVolume > 0f)
                {
                    targetAudioSource.Play();
                }
            }
            else
            {
                Debug.LogError("Audio Source has no clip assigned!");
            }
        }
        else
        {
            Debug.LogError("Target Audio Source is not assigned in the Inspector!");
        }

        volumeSlider.value = savedVolume;
        metricDropdown.value = savedMetric;
        ApplyVolume(savedVolume);

        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        metricDropdown.onValueChanged.AddListener(OnMetricChanged);
    }

    void OnVolumeChanged(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat("Volume", value);
    }

    void ApplyVolume(float value)
    {
        value = Mathf.Clamp01(value);
        
        if (targetAudioSource != null)
        {
            targetAudioSource.volume = value;
            
            if (!targetAudioSource.isPlaying && targetAudioSource.clip != null && value > 0f)
            {
                targetAudioSource.Play();
            }
            else if (targetAudioSource.isPlaying && value <= 0f)
            {
                targetAudioSource.Stop();
            }
        }
        else
        {
            Debug.LogWarning("Target Audio Source is not assigned!");
        }

        AudioListener.volume = value;
    }

    void OnMetricChanged(int index)
    {
        PlayerPrefs.SetInt("MetricSystem", index);
    }
}
