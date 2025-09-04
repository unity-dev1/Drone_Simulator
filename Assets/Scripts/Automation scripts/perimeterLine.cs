using System.Collections.Generic;
using UnityEngine;

public class PerimeterLine : MonoBehaviour
{
    [SerializeField] List<Transform> nodes;
    [SerializeField] GameObject midPointPrefab;
    [SerializeField] GameObject nodePrefab;
    [SerializeField] EdgeCollider2D edgeColliderPrefab;

    LineRenderer lr;
    List<GameObject> midPoints = new List<GameObject>();
    List<EdgeCollider2D> edgeColliders = new List<EdgeCollider2D>();

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = nodes.Count;

        // Make sure all nodes have drag handlers
        foreach (Transform node in nodes)
        {
            if (node.GetComponent<NodeDragHandler>() == null)
            {
                NodeDragHandler handler = node.gameObject.AddComponent<NodeDragHandler>();
                // Set reference to this perimeter line
                handler.SetPerimeterLine(this);
            }
            else
            {
                // Set reference to this perimeter line for existing handlers
                node.GetComponent<NodeDragHandler>().SetPerimeterLine(this);
            }
        }

        CreateMidPoints();
        CreateEdgeColliders();
    }

    private void Update()
    {
        // Update line renderer
        if (nodes.Count > 0)
        {
            Vector3[] positions = new Vector3[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] != null)
                {
                    positions[i] = nodes[i].position;
                }
            }
            lr.positionCount = nodes.Count;
            lr.SetPositions(positions);

            // Update edge colliders to catch node movements
            UpdateEdgeColliders();
        }

        // Update midpoints based on nodes positions
        UpdateMidPoints();
    }

    private void CreateMidPoints()
    {
        // Clear existing midpoints
        foreach (var mp in midPoints)
        {
            if (mp != null) Destroy(mp);
        }
        midPoints.Clear();

        // Create midpoints between each pair of nodes
        for (int i = 0; i < nodes.Count; i++)
        {
            int nextIndex = (i + 1) % nodes.Count;
            Vector3 midPosition = (nodes[i].position + nodes[nextIndex].position) / 2f;




            GameObject midPoint = Instantiate(midPointPrefab, midPosition, Quaternion.identity, transform);

            // Add click handler component
            MidPointHandler handler = midPoint.GetComponent<MidPointHandler>();
            if (handler == null)
            {
                handler = midPoint.AddComponent<MidPointHandler>();
            }
            handler.Initialize(this, i);

            

            midPoints.Add(midPoint);

           
        }
    }

    private void UpdateMidPoints()
    {
        // Make sure we have the right number of midpoints
        if (midPoints.Count != nodes.Count)
        {
            CreateMidPoints();
            return;
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            int nextIndex = (i + 1) % nodes.Count;
            if (midPoints[i] != null && nodes[i] != null && nodes[nextIndex] != null)
            {
                midPoints[i].transform.position = (nodes[i].position + nodes[nextIndex].position) / 2f;
            }
        }
    }

    public void AddNodeAtMidPoint(int midPointIndex)
    {
        // Get the position of the clicked midpoint
        Vector3 midPointPosition = midPoints[midPointIndex].transform.position;

        // Create a new main node at this position
        GameObject newNode = Instantiate(nodePrefab, midPointPosition, Quaternion.identity, transform);

        // Add drag handler to the new node and set reference to this perimeter line
        NodeDragHandler handler = newNode.GetComponent<NodeDragHandler>();
        if (handler == null)
        {
            handler = newNode.AddComponent<NodeDragHandler>();
        }
        handler.SetPerimeterLine(this);

        // Insert the new node into the nodes list after the current index
        int nodeInsertIndex = (midPointIndex + 1) % nodes.Count;
        nodes.Insert(nodeInsertIndex, newNode.transform);

        // Update the line renderer
        lr.positionCount = nodes.Count;

        // Recreate all midpoints
        CreateMidPoints();

        // Recreate and update the edge colliders
        CreateEdgeColliders();
    }

    private void CreateEdgeColliders()
    {
        // Clear existing edge colliders
        foreach (var collider in edgeColliders)
        {
            if (collider != null) Destroy(collider.gameObject);
        }
        edgeColliders.Clear();

        // Create a separate edge collider for each segment
        for (int i = 0; i < nodes.Count; i++)
        {
            int nextIndex = (i + 1) % nodes.Count;

            // Create a new GameObject for this edge
            GameObject edgeObj = new GameObject($"EdgeCollider_{i}");
            edgeObj.transform.SetParent(transform);
            edgeObj.transform.localPosition = Vector3.zero;

            // Add edge collider
            EdgeCollider2D edgeCollider;
            if (edgeColliderPrefab != null)
            {
                // Instantiate from prefab
                edgeCollider = Instantiate(edgeColliderPrefab, edgeObj.transform);
            }
            else
            {
                // Create a new edge collider
                edgeCollider = edgeObj.AddComponent<EdgeCollider2D>();
            }

            // Set points for this segment
            Vector2[] points = new Vector2[2];

            // Convert world positions to local positions relative to the edge object
            Vector3 startLocalPos = edgeObj.transform.InverseTransformPoint(nodes[i].position);
            Vector3 endLocalPos = edgeObj.transform.InverseTransformPoint(nodes[nextIndex].position);

            points[0] = new Vector2(startLocalPos.x, startLocalPos.y);
            points[1] = new Vector2(endLocalPos.x, endLocalPos.y);

            edgeCollider.points = points;
            edgeColliders.Add(edgeCollider);
        }
    }

    public void UpdateEdgeColliders()
    {
        // Make sure we have the right number of edge colliders
        if (edgeColliders.Count != nodes.Count)
        {
            CreateEdgeColliders();
            return;
        }

        // Update each edge collider with current node positions
        for (int i = 0; i < nodes.Count; i++)
        {
            int nextIndex = (i + 1) % nodes.Count;

            if (edgeColliders[i] != null && nodes[i] != null && nodes[nextIndex] != null)
            {
                // Convert world positions to local positions relative to the edge collider's transform
                Vector3 startLocalPos = edgeColliders[i].transform.InverseTransformPoint(nodes[i].position);
                Vector3 endLocalPos = edgeColliders[i].transform.InverseTransformPoint(nodes[nextIndex].position);

                Vector2[] points = new Vector2[2];
                points[0] = new Vector2(startLocalPos.x, startLocalPos.y);
                points[1] = new Vector2(endLocalPos.x, endLocalPos.y);

                edgeColliders[i].points = points;
            }
        }
    }

    // Public method to get the list of edge colliders
    public List<EdgeCollider2D> GetEdgeColliders()
    {
        return edgeColliders;
    }

    // Public method to get the list of nodes
    public List<Transform> GetNodes()
    {
        return nodes;
    }
}