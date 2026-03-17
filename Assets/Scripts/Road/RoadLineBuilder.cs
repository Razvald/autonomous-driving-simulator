using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class RoadLineBuilder : MonoBehaviour
{
    public List<Transform> points; // Точки дороги
    public float width = 5f;

    private LineRenderer lr;
    private EdgeCollider2D edge;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        edge = GetComponent<EdgeCollider2D>();

        BuildLine();
        BuildCollider();
    }

    public void BuildLine()
    {
        if (points.Count < 2) return;

        lr.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++)
            lr.SetPosition(i, points[i].position);

        lr.startWidth = width;
        lr.endWidth = width;
    }

    void BuildCollider()
    {
        List<Vector2> colliderPoints = new List<Vector2>();
        foreach (var p in points)
            colliderPoints.Add(p.position);

        edge.points = colliderPoints.ToArray();
        edge.edgeRadius = width / 2f;
    }
}