using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrackCheckpointUI : MonoBehaviour
{
    [SerializeField] private TrackCheckpoint trackCheckpoint;

    private void Start()
    {
         trackCheckpoint.OnPlayerCorrectCheckpoint += TrackCheckpoint_OnPlayerCorrectCheckpoint;
         trackCheckpoint.OnTrackComplete += TrackCheckpoint_OnTrackComplete;

        Show();
    }

    private void TrackCheckpoint_OnPlayerCorrectCheckpoint(object sender, EventArgs e)
    {
        Show();
    }

    private void TrackCheckpoint_OnTrackComplete(object sender, EventArgs e)
    {
     // add scene here
         Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }     
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
