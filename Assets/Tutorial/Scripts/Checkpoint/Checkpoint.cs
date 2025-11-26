using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Accessibility;

public class Checkpoint : MonoBehaviour
{
    private TrackCheckpoint TrackCheckpoint;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        HideCheckpoint();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Drone>(out Drone drone))
        {
            TrackCheckpoint.PlayerThroughCheckpoint(this);      //Notify the main class when player goes through this checkpoint
        }
    }

    public void SetTrackCheckpoints(TrackCheckpoint trackCheckpoint)
    {
        this.TrackCheckpoint = trackCheckpoint;
    }

    public void ShowCheckpoint()
    {
        meshRenderer.enabled = true;
    }
     public void HideCheckpoint()
    {
        meshRenderer.enabled = false;
    }
}
