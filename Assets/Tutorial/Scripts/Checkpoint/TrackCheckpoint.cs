using System;
using Newtonsoft.Json.Bson;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TrackCheckpoint : MonoBehaviour
{
    public event EventHandler OnPlayerCorrectCheckpoint;
    public event EventHandler OnPlayerWrongCheckpoint;
    public event EventHandler OnTrackComplete;

    public List<Checkpoint> checkpointList = new List<Checkpoint>();
    private int nextCheckpointSingleIndex;
    private void Awake()
    {
        Transform checkpointTransform = transform.Find("Checkpoints");

        // Checks for all the checkpoints in the Chechpoints GameObject
        foreach(Transform checkpointTracnsform in checkpointTransform)
        {
            Checkpoint checkpoint = checkpointTracnsform.GetComponent<Checkpoint>();

            checkpoint.SetTrackCheckpoints(this);

            checkpointList.Add(checkpoint);
        }

        nextCheckpointSingleIndex = 0;     
    }

    private void Start()
    {
        ShowCurrentCheckpoint();
    }
 
    public void PlayerThroughCheckpoint(Checkpoint checkpoint)
    {
        if (checkpointList.IndexOf(checkpoint) ==  nextCheckpointSingleIndex)
        {
            // Correct checkpoint
            Debug.Log("Correct");
            nextCheckpointSingleIndex = (nextCheckpointSingleIndex + 1) % checkpointList.Count;

            if (nextCheckpointSingleIndex == 0) // Completed the track
            {
                // Trigger track completion event
                Debug.Log("Track completed!");
                OnTrackComplete?.Invoke(this, EventArgs.Empty);
                HideCurrentCheckpoint(checkpoint);
            }else
            {
                OnPlayerCorrectCheckpoint?.Invoke(this, EventArgs.Empty);

                ShowNextCheckpoint();
                HideCurrentCheckpoint(checkpoint);
            }
        }
        else
        {
            // Wrong checkpoint
            Debug.Log("Wrong");
            OnPlayerWrongCheckpoint?.Invoke(this, EventArgs.Empty);
        }
    }

    private void HideCurrentCheckpoint(Checkpoint checkpoint)
    {
        Checkpoint hideCurrentCheckpoint = checkpointList[checkpointList.IndexOf(checkpoint)];
        hideCurrentCheckpoint.HideCheckpoint();
    } 
    private void ShowNextCheckpoint()
    {
        Checkpoint showNextCheckpoint = checkpointList[nextCheckpointSingleIndex];
        showNextCheckpoint.ShowCheckpoint();
    } 
    private void ShowCurrentCheckpoint()
    {
        Checkpoint showCurentCheckpoint = checkpointList[nextCheckpointSingleIndex];
        showCurentCheckpoint.ShowCheckpoint();
    }
}
