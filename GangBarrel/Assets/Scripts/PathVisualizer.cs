using UnityEngine;
using Pathfinding;
using System.Linq; // Required for LINQ

public class PathVisualizer : MonoBehaviour {
    private LineRenderer lineRenderer;
    private Seeker seeker;
    public Material lineMaterial;
    
    void Start() {
        lineRenderer = GetComponent<LineRenderer>();
        seeker = GetComponent<Seeker>();
        seeker.pathCallback += OnPathComplete;
        lineMaterial.renderQueue = 3001; // see path to everything.
    }

    void OnPathComplete(Path p) {
        if (p.error) return;

        // Modify the path in one line using LINQ
        Vector3[] modifiedPath = p.vectorPath
            .Select(position => new Vector3(position.x, position.y + 0.1f, position.z)) // Add y-offset
            .ToArray();

        // Set the modified path to the LineRenderer
        lineRenderer.positionCount = modifiedPath.Length;
        lineRenderer.SetPositions(modifiedPath);
    }
}