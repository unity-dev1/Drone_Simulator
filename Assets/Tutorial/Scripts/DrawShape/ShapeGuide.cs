using System.Drawing;
using UnityEngine;

[ExecuteInEditMode]
public class ShapeGuide : MonoBehaviour
{
    public enum GuideType { Square, Circle, Figure8 }
    public GuideType shapeType;
    public int segments = 60;
    private LineRenderer lr;
    public float size = 15f;
    private Vector3[] points;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        UpdateShape();
    }

    void OnValidate() => UpdateShape();

    void UpdateShape()
    {
        if (lr == null) return;

        switch (shapeType)
        {
            case GuideType.Square:
                points = new Vector3[]
                {
                    new Vector3(-size, 0, -size),
                    new Vector3(size, 0, -size),
                    new Vector3(size, 0, size),
                    new Vector3(-size, 0, size)
                };
                break;

            case GuideType.Circle:
                points = new Vector3[segments];
                for (int i = 0; i < segments; i++)
                {
                    float angle = i / (float)segments * Mathf.PI * 2f;
                    points[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * size;
                }
                break;

            case GuideType.Figure8:
                points = new Vector3[segments];
                for (int i = 0; i < segments; i++)
                {
                    float t = i / (float)segments * Mathf.PI * 2f;
                    float x = Mathf.Sin(t);
                    float z = Mathf.Sin(t) * Mathf.Cos(t);
                    points[i] = new Vector3(x * size, 0, z * size);
                }
                break;
        }

        lr.positionCount = points.Length;
        lr.SetPositions(points);
    }
}
