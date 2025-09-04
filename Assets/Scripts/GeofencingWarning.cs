using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum CollidingDirection
{
    None,
    Left,
    Right,
    Front,
    Back,
}

public class GeofencingWarning : MonoBehaviour
{
    [Header("UI and Visuals")]
    [SerializeField] private GameObject geoFencingWarningUI;
    [SerializeField] private GameObject geofencingErrorUI;
    [SerializeField] private Material borderMat;

    [Header("Geofence Settings")]
    [SerializeField] private float checkDistance = 0.3f;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private Color safeColor = Color.green;

    [SerializeField] private GeoFencingEffect fencingEffect;
    [SerializeField] private List<EffectOrientation> effectOrientations;

    public CollidingDirection collidingDirection;
    private Dictionary<int, GeoFencingEffectDirection> geofencingEffectOrientation = new Dictionary<int, GeoFencingEffectDirection>();

    public Action<CollidingDirection, int> onGetDirection;

    private void OnEnable()
    {
        InitializeOrientation();
        onGetDirection += GetCollisionDirection;
    }

    private void OnDisable()
    {
        onGetDirection -= GetCollisionDirection;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7)
        {
            if (geoFencingWarningUI != null)
                geoFencingWarningUI.SetActive(true);
            FencingWarning();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 7)
        {
            Vector3 closestPoint = other.ClosestPoint(transform.position);
            float distance = Vector3.Distance(transform.position, closestPoint);

            if (distance < checkDistance)
            {
                if (borderMat != null)
                    borderMat.color = warningColor;
                if (geoFencingWarningUI != null)
                    geoFencingWarningUI.SetActive(false);
                if (geofencingErrorUI != null)
                    geofencingErrorUI.SetActive(true);

                FinalWarning();
            }
            else
            {
                if (borderMat != null)
                    borderMat.color = safeColor;
                if (geoFencingWarningUI != null)
                    geoFencingWarningUI.SetActive(true);
                if (geofencingErrorUI != null)
                    geofencingErrorUI.SetActive(false);

                FencingWarning();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 7)
        {
            if (geoFencingWarningUI != null)
                geoFencingWarningUI.SetActive(false);
            if (borderMat != null)
                borderMat.color = safeColor;
            if (fencingEffect != null)
            {
                fencingEffect.gameObject.SetActive(false);
            }
            collidingDirection = CollidingDirection.None;
        }
    }

    private void FencingWarning() 
    {
        if (fencingEffect != null)
        {
            fencingEffect.gameObject.SetActive(true);
            fencingEffect.WarningEffect();
        }
    }

    private void FinalWarning()
    {
        if (fencingEffect != null)
        {
            fencingEffect.gameObject.SetActive(true);
            fencingEffect.FinalWarning();
        }
    }

    private void InitializeOrientation() 
    {
        if (effectOrientations == null) return;
        
        foreach (EffectOrientation orientation in effectOrientations) 
        {
            if (orientation != null && orientation.directions != null)
            {
                geofencingEffectOrientation[orientation.droneIndex] = orientation.directions;
            }
        }
    }

    private void GetCollisionDirection(CollidingDirection direction, int selectedDroneIndex)
    {
        collidingDirection = direction;

        if (fencingEffect == null || !geofencingEffectOrientation.ContainsKey(selectedDroneIndex))
            return;

        var effectDirection = geofencingEffectOrientation[selectedDroneIndex];

        switch (collidingDirection)
        {
            case CollidingDirection.Left:
                fencingEffect.transform.localEulerAngles = effectDirection.leftEffectRotation;
                fencingEffect.transform.localPosition = effectDirection.leftEffectPosition;
                break;
            case CollidingDirection.Right:
                fencingEffect.transform.localEulerAngles = effectDirection.rightEffectRotation;
                fencingEffect.transform.localPosition = effectDirection.rightEffectPosition;
                break;
            case CollidingDirection.Front:
                fencingEffect.transform.localEulerAngles = effectDirection.frontEffectRotation;
                fencingEffect.transform.localPosition = effectDirection.frontEffectPosition;
                break;
            case CollidingDirection.Back:
                fencingEffect.transform.localEulerAngles = effectDirection.backEffectRotation;
                fencingEffect.transform.localPosition = effectDirection.backEffectPosition;
                break;
        }
    }
}

[Serializable]
public class EffectOrientation 
{
    public int droneIndex;
    public GeoFencingEffectDirection directions;
}

[Serializable]
public class GeoFencingEffectDirection 
{
    public Vector3 leftEffectRotation;
    public Vector3 rightEffectRotation;
    public Vector3 frontEffectRotation;
    public Vector3 backEffectRotation;

    public Vector3 leftEffectPosition;
    public Vector3 rightEffectPosition;
    public Vector3 frontEffectPosition;
    public Vector3 backEffectPosition;
}

