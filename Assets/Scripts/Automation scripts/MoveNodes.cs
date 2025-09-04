using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class NodeDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector3 offset;
    private Camera mainCamera;
    private PerimeterLine perimeterLine;
    [SerializeField] private HatchPattern pt;
    private RectTransform rectTransform;

    // Boundary constraints (local coordinates for RectTransform)
    public float MIN_X = -625f;
    public float MAX_X = 625f;
    public float MIN_Y = -350f;
    public float MAX_Y = 350f;

    private int nodeIndex = -1; // To track this node's index in the perimeter

    private void Awake()
    {
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }

        if (pt == null)
            pt = FindAnyObjectByType<HatchPattern>();
    }

    private void OnEnable()
    {
        SetBoundaryConstraint();
    }

    private void SetBoundaryConstraint() 
    {
        EnvironmentOptionSO env = SelectionManager.Instance?.selectedEnvironment;
        MIN_X = env.hatchMinX;
        MAX_X = env.hatchMaxX;
        MIN_Y = env.hatchMinY;
        MAX_Y = env.hatchMaxY;
    }


    // Method to set perimeter line reference
    public void SetPerimeterLine(PerimeterLine perimeter)
    {
        perimeterLine = perimeter;
        // Find and store this node's index in the perimeter nodes list
        if (perimeterLine != null)
        {
            List<Transform> nodes = perimeterLine.GetNodes();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == this.transform)
                {
                    nodeIndex = i;
                    break;
                }
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Calculate offset from mouse to object position
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, transform.position.z - mainCamera.transform.position.z));
        offset = transform.position - worldPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Move the node to follow the mouse
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, transform.position.z - mainCamera.transform.position.z));
        Vector3 targetPosition = worldPos + offset;

        // Convert to local position (relative to parent)
        Vector3 localPos = transform.parent.InverseTransformPoint(targetPosition);

        // Clamp position within boundaries
        localPos.x = Mathf.Clamp(localPos.x, MIN_X, MAX_X);
        localPos.y = Mathf.Clamp(localPos.y, MIN_Y, MAX_Y);

        // Convert back to world position
        targetPosition = transform.parent.TransformPoint(localPos);

        // Set the new position
        transform.position = targetPosition;

        // Update the edge colliders after moving
        if (perimeterLine != null)
        {
            perimeterLine.UpdateEdgeColliders();
        }

        // Generate pattern only once per drag frame
        if (pt != null)
        {
            pt.GeneratePattern();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Ensure edge colliders are updated after drag is complete
        if (perimeterLine != null)
        {
            perimeterLine.UpdateEdgeColliders();
        }
    }
}