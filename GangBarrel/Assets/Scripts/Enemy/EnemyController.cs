using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviour
{
    [Header("Settings")]
    public int movementRange = 3; // Enemy's maximum movement range per turn
    private bool canMove = false;
    public bool isInTurn;
    public float shootingDistance;
    public float bulletSpeed = 15;
    
    [Header("References")]
    public Transform player;
    public Transform exitPoint;
    public LineRenderer pathLineRenderer; // Assign in the Inspector
    private List<Vector3[]> possiblePaths = new List<Vector3[]>();
    private Vector3[] chosenPath;
    public AIDestinationSetter aiDestinationSetter;
    public LineRenderer lineRenderer;
    public RoundManager roundManager;
    [SerializeField] private GameObject bulletPrefab;
    
    public enum EnemyBehaviour
    {
        ShortestPathTowardsAirpath,
        ShortestPathTowardsActualPath
    }

    
    
    public EnemyBehaviour enemyBehaviour;

    private void Awake()
    {
        roundManager = FindObjectOfType<RoundManager>();
        player = FindObjectOfType<PlayerController>().gameObject.transform;
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
                    dest = FindPointAlongPlayGoalPath();
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
    
    private Vector3 FindPointAlongPlayGoalPath()
    {
        var playerPos = roundManager.playerController.transform.position;
        var playerGoalPos = roundManager.playersGoal.position;
        var enemyPos = transform.position;

        // 1. Calculate direction vector from player to goal
        Vector3 playerToGoalDirection = (playerGoalPos - playerPos).normalized;
    
        // 2. Calculate point at shooting distance from player along that path
        Vector3 targetPoint = playerPos + (playerToGoalDirection * shootingDistance);

        // 3. Use A* to find path and move towards target point
        Path path = ABPath.Construct(enemyPos, targetPoint, null);
        AstarPath.StartPath(path);
        path.BlockUntilCalculated();

        if (path.error || path.vectorPath.Count == 0)
        {
            Debug.LogWarning("No valid path found to target point");
            return enemyPos;
        }

        Vector3 finalDestination = enemyPos;
        float totalDistance = 0;

        // Walk through path points until we hit our action limit
        for (int i = 0; i < path.vectorPath.Count - 1; i++)
        {
            Vector3 currentPoint = path.vectorPath[i];
            Vector3 nextPoint = path.vectorPath[i + 1];
            float segmentLength = Vector3.Distance(currentPoint, nextPoint);
        
            if (IsDistanceWithinRemainingActions(totalDistance + segmentLength))
            {
                totalDistance += segmentLength;
                finalDestination = nextPoint;
            }
            else
            {
                float remainingDistance = roundManager.remainingActions - totalDistance;
                if (remainingDistance > 0)
                {
                    Vector3 direction = (nextPoint - currentPoint).normalized;
                    finalDestination = currentPoint + direction * remainingDistance;
                }
                break;
            }
        }

        // After movement, check if we can shoot with remaining actions
        UseRemainingActionsForShooting(finalDestination);

        return finalDestination;
    }
    
    private void UseRemainingActionsForShooting(Vector3 finalPosition)
    {
        // Check if we reached our desired position (with some tolerance)
        float distanceToDesiredPosition = Vector3.Distance(transform.position, finalPosition);
        if (distanceToDesiredPosition <= 0.5f) // 0.5f is tolerance
        {
            // Check if we're at shooting distance from player
            float distanceToPlayer = Vector3.Distance(finalPosition, roundManager.playerController.transform.position);
            if (Mathf.Abs(distanceToPlayer - shootingDistance) <= 0.5f)
            {
                // While we still have actions, keep shooting
                if (roundManager.remainingActions > 0)
                {
                    Shoot();
                    roundManager.DecrementActions(1); // Assuming each shot costs 1 action
                }
            }
        }
    }
    
    /// <summary>
    /// True: enough actions
    /// False: not enough actions
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    private bool IsDistanceWithinRemainingActions(float distance)
    {
        return (Mathf.CeilToInt(distance) <= roundManager.remainingActions);
    }
    
    #endregion

    private void Shoot()
    {
        Debug.Log($"Enemy {gameObject.name} is shooting at player!");
        // Implement shooting logic here
        // For example:
        // - Check line of sight
        // - Apply damage
        // - Play effects
        // - Use action points
        
        GameObject bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Physics.IgnoreCollision(bulletObject.GetComponent<Collider>(), GetComponentInChildren<Collider>());
        Destroy(bulletObject, 5f);
        
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0.01f;
        bulletObject.GetComponent<Rigidbody>().velocity = direction * bulletSpeed;

        roundManager.DecrementActions(1);
        Debug.Log("Bullet shot successfully!");

        // Set the state back to Idle
        Debug.Log("Player state set back to Idle.");
        
    }

    
    private void OnTriggerEnter(Collider other)
    {
        // Enemy can die in explosion or by bullet.
        if (other.CompareTag("ExplosionTrigger") && other.CompareTag("Bullet"))
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
