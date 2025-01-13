using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviour
{
    public int movementRange = 3; // Enemy's maximum movement range per turn
    public Transform player;
    public Transform exitPoint;
    public LineRenderer pathLineRenderer; // Assign in the Inspector

    private bool canMove = false;
    private List<Vector3[]> possiblePaths = new List<Vector3[]>();
    private Vector3[] chosenPath;
    public AIDestinationSetter aiDestinationSetter;

    public LineRenderer lineRenderer;
    
    public RoundManager roundManager;

    public bool isInTurn;

    public enum EnemyBehaviour
    {
        ShortestPathTowardsAirpath,
        ShortestPathTowardsActualPath
    }

    public EnemyBehaviour enemyBehaviour;

    private void Awake()
    {
        roundManager = FindObjectOfType<RoundManager>();
    }

    public void StartEnemyTurn()
    {
        if (roundManager.isCombatActive && isInTurn)
        {
            Debug.Log("Now the enemy should move towards that middle of the line");
            var dest = Vector3.zero;
            switch (enemyBehaviour)
            {
                case EnemyBehaviour.ShortestPathTowardsAirpath:
                    dest = FindRestrictedPointTowardsGoal();
                    break;
                case EnemyBehaviour.ShortestPathTowardsActualPath:
                    dest = FindOptimalInterceptionPoint();
                    break;
                default:
                    Debug.LogError("There is no further enemy behaviours implemented yet!");
                    break;
            }
            
            SetAITarget(dest);
        }
    }

    #region COPY_FROM_PLAYERCONTROLLER
    private void SetAITarget(Vector3 targetPosition)
    {
        // Safely destroy the previous target if it exists
        if (aiDestinationSetter.target != null)
        {
            // Check if the target still exists in the scene
            if (aiDestinationSetter.target.gameObject != null && aiDestinationSetter.target.gameObject.name == "TempTarget")
            {
                Destroy(aiDestinationSetter.target.gameObject);
            }

            // Clear the reference to avoid accessing a destroyed object
            aiDestinationSetter.target = null;
        }

        // Create a new temporary target object
        GameObject tempTarget = new GameObject("TempTarget");
        tempTarget.transform.position = targetPosition;

        // Set the new target for AI
        aiDestinationSetter.target = tempTarget.transform;

        // Start the coroutine to wait for the target to be reached
        StartCoroutine(WaitForTargetReached(tempTarget));
        
    }
    private IEnumerator WaitForTargetReached(GameObject tempTarget)
    {
        while (tempTarget != null && Vector3.Distance(transform.position, tempTarget.transform.position) > 0.5f)
        {
            yield return null; // Wait for the next frame
        }

        // Safely destroy the temporary target object if it still exists
        if (tempTarget != null)
        {
            Destroy(tempTarget);
        }

        // Clear the reference in the AI Destination Setter
        if (aiDestinationSetter.target != null && aiDestinationSetter.target.gameObject == tempTarget)
        {
            aiDestinationSetter.target = null;
        }

        // Clear the path visualization
        ClearLineRenderer();
        
        roundManager.DecrementActions(movementRange);
    }
    
    private void ClearLineRenderer()
    {
        if (lineRenderer)
        {
            lineRenderer.positionCount = 0;
        }
    }
    
    #endregion COPY_FROM_PLAYERCONTROLLER
    
    #region ADVANCED
    
    private Vector3 FindOptimalInterceptionPoint()
    {
        var playerPos = roundManager.playerController.transform.position;
        var playerGoalPos = roundManager.playersGoal.position;
        var enemyPos = transform.position;

        // Get the player's path to goal using A*
        var playerPathToGoal = ABPath.Construct(playerPos, playerGoalPos);
        AstarPath.StartPath(playerPathToGoal);
        playerPathToGoal.BlockUntilCalculated();

        if (playerPathToGoal.error || playerPathToGoal.vectorPath.Count == 0)
        {
            return playerPos; // Fallback to player position if no path exists
        }

        Vector3 bestInterceptionPoint = playerPos;
        float bestScore = float.MinValue;

        // Evaluate each point along the player's path
        for (int i = 0; i < playerPathToGoal.vectorPath.Count; i++)
        {
            Vector3 pathPoint = playerPathToGoal.vectorPath[i];
            
            // Calculate path from enemy to this point
            var enemyPath = ABPath.Construct(enemyPos, pathPoint);
            AstarPath.StartPath(enemyPath);
            enemyPath.BlockUntilCalculated();

            if (enemyPath.error) continue;

            // Count unwalkable nodes in the remaining path to goal from this point
            int unwalkableNodesCount = CountUnwalkableNodesInPath(pathPoint, playerGoalPos);
            
            // Calculate score based on multiple factors
            float score = CalculateInterceptionScore(
                enemyPath.vectorPath.Count,  // Length of enemy path to this point
                playerPathToGoal.vectorPath.Count - i,  // Remaining length of player path
                unwalkableNodesCount
            );

            if (score > bestScore)
            {
                bestScore = score;
                bestInterceptionPoint = pathPoint;
            }
        }

        return bestInterceptionPoint;
    }
    
    private struct PassageInfo
    {
        public Vector3 position;
        public float width;
        public int neighboringWalls;
    }
    
    private int CountUnwalkableNodesInPath(Vector3 startPos, Vector3 endPos)
    {
        // Get a rough rectangle of nodes between start and end positions
        var bounds = new Bounds();
        bounds.SetMinMax(
            Vector3.Min(startPos, endPos),
            Vector3.Max(startPos, endPos)
        );
        
        int unwalkableCount = 0;
        var graph = AstarPath.active.data.gridGraph;

        // Expand bounds slightly to include nearby nodes
        bounds.Expand(2f);

        // Check each node in the bounded area
        var nodes = graph.GetNodesInRegion(bounds);
        foreach (var node in nodes)
        {
            if (!node.Walkable)
            {
                unwalkableCount++;
            }
        }

        return unwalkableCount;
    }

    private float CalculateInterceptionScore(int enemyPathLength, int remainingPlayerPathLength, int unwalkableNodes, PassageInfo? passageInfo = null)
    {
        const float ENEMY_PATH_WEIGHT = 1.0f;
        const float PLAYER_PATH_WEIGHT = 0.8f;
        const float UNWALKABLE_WEIGHT = 1.2f;
        const float NARROW_PASSAGE_WEIGHT = 2.0f;

        float normalizedEnemyPath = 1.0f - (enemyPathLength / 100f);
        float normalizedPlayerPath = 1.0f - (remainingPlayerPathLength / 100f);
        float normalizedUnwalkable = unwalkableNodes / 20f;

        float score = (normalizedEnemyPath * ENEMY_PATH_WEIGHT) +
                      (normalizedPlayerPath * PLAYER_PATH_WEIGHT) +
                      (normalizedUnwalkable * UNWALKABLE_WEIGHT);

        // Add bonus for narrow passages
        if (passageInfo.HasValue)
        {
            float narrownessScore = (1.0f - (passageInfo.Value.width / (4f * AstarPath.active.data.gridGraph.nodeSize)));
            score += narrownessScore * NARROW_PASSAGE_WEIGHT;
        }

        return score;
    }
    
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ExplosionTrigger"))
        {
            roundManager.RemoveEnemyFromCombat(this);
            Destroy(gameObject);
        }
    }

    private Vector3 FindShortestPathToPlayerPathTowardsGoal()
    {
        Vector3 closestPoint = Vector3.zero;

        // Get positions
        var playerPos = roundManager.playerController.transform.position;
        var playerGoalPos = roundManager.playersGoal.position;
        var enemyPos = transform.position;

        // Calculate the direction vector of the player's path
        Vector3 playerPathDirection = playerGoalPos - playerPos;
        float playerPathLengthSquared = playerPathDirection.sqrMagnitude;

        if (playerPathLengthSquared == 0)
        {
            // The player's position and goal are the same
            return playerPos;
        }

        // Projection factor t to find the closest point
        float t = Vector3.Dot(enemyPos - playerPos, playerPathDirection) / playerPathLengthSquared;

        // Clamp t to [0, 1] if you want the closest point on the segment
        t = Mathf.Clamp01(t);

        // Calculate the closest point on the player's path
        closestPoint = playerPos + t * playerPathDirection;

        return closestPoint;
    }
    
    private Vector3 FindRestrictedPointTowardsGoal()
    {
        // Get the closest point on the player's path
        Vector3 targetPoint = FindShortestPathToPlayerPathTowardsGoal();

        // Get the enemy's current position
        Vector3 enemyPos = transform.position;

        // Calculate the distance to the target point
        float distanceToTarget = Vector3.Distance(enemyPos, targetPoint);

        // Get the maximum allowed distance (remaining actions)
        float maxDistance = roundManager.remainingActions;

        // If the target point is within the allowed range, return it directly
        if (distanceToTarget <= maxDistance)
        {
            return targetPoint;
        }

        // Otherwise, calculate a new point within the restricted range
        Vector3 direction = (targetPoint - enemyPos).normalized; // Direction toward the target
        return enemyPos + direction * maxDistance; // Move only the allowed distance
    }
    
    private int CalculatePathDistance(Vector3[] path)
    {
        if (path == null || path.Length < 2) return 0;

        int totalDistance = 0;

        for (int i = 0; i < path.Length - 1; i++)
        {
            totalDistance += Mathf.RoundToInt(Vector3.Distance(path[i], path[i + 1]));
        }

        return totalDistance;
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
