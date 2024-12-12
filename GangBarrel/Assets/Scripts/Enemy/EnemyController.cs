using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float movementRange = 3f; // Enemy's maximum movement range per turn
    public Transform player;
    public Transform exitPoint;
    public LineRenderer pathLineRenderer; // Assign in the Inspector

    private bool canMove = false;
    private List<Vector3[]> possiblePaths = new List<Vector3[]>();
    private Vector3[] chosenPath;

    public void EnableMovement(float rangeLimit)
    {
        movementRange = rangeLimit;
    }

    public void StartTurn()
    {
        canMove = true;
        StartCoroutine(ThinkAndMove());
    }

    public void EndTurn()
    {
        canMove = false;
        // Notify the RoundManager that the enemy's turn has ended
        FindObjectOfType<RoundManager>().EndEnemyTurn();
    }

    private IEnumerator ThinkAndMove()
    {
        Debug.Log("Enemy is thinking...");
        possiblePaths.Clear();

        // Generate 3-5 possible paths to evaluate
        int pathCount = Random.Range(3, 6);
        for (int i = 0; i < pathCount; i++)
        {
            VisualizePath();
            yield return new WaitForSeconds(1f); // Show each path for 1 second
        }

        // Select the final path
        chosenPath = SelectBestPath();

        // Clear all visualized paths
        pathLineRenderer.positionCount = 0;

        Debug.Log("Enemy chooses a path and moves.");
        ExecutePath(chosenPath);
    }

    private void VisualizePath()
    {
        // Randomize a target position within the movement range
        Vector3 randomTarget = transform.position + new Vector3(
            Random.Range(-movementRange, movementRange),
            0,
            Random.Range(-movementRange, movementRange)
        );

        // Check if the random target is valid
        if (!CanTraversePath(transform.position, randomTarget))
            return;

        // Calculate the path
        Path path = ABPath.Construct(transform.position, randomTarget);
        AstarPath.StartPath(path);
        path.BlockUntilCalculated();

        if (!path.error)
        {
            // Save the path for evaluation
            possiblePaths.Add(path.vectorPath.ToArray());

            // Visualize the path
            DrawPath(path.vectorPath.ToArray());
        }
    }

    private Vector3[] SelectBestPath()
    {
        // Example logic: Choose the path closest to the player
        return possiblePaths
            .OrderBy(path => Vector3.Distance(path.Last(), player.position))
            .FirstOrDefault();
    }

    private void ExecutePath(Vector3[] path)
    {
        if (path == null || path.Length == 0)
        {
            Debug.LogWarning("No valid path to execute.");
            EndTurn();
            return;
        }

        StartCoroutine(MoveAlongPath(path));
    }

    private IEnumerator MoveAlongPath(Vector3[] path)
    {
        foreach (var point in path)
        {
            // Move to the next point in the path
            while (Vector3.Distance(transform.position, point) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, point, Time.deltaTime * 2f); // Adjust speed as needed
                yield return null;
            }
        }

        Debug.Log("Enemy reached its destination.");
        EndTurn();
    }

    private bool CanTraversePath(Vector3 start, Vector3 end)
    {
        GraphNode startNode = AstarPath.active.GetNearest(start).node;
        GraphNode endNode = AstarPath.active.GetNearest(end).node;

        if (startNode == null || endNode == null || !startNode.Walkable || !endNode.Walkable)
        {
            return false;
        }

        return PathUtilities.IsPathPossible(startNode, endNode);
    }

    private void DrawPath(Vector3[] pathPoints)
    {
        if (pathLineRenderer != null)
        {
            pathLineRenderer.positionCount = pathPoints.Length;
            pathLineRenderer.SetPositions(pathPoints);
        }
    }
}
