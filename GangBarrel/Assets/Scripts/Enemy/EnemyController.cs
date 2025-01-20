using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Pathfinding;
using UniRx;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class EnemyController : MonoBehaviour
{
    [Header("Settings")]
    public int maxActions = 3; // Enemy's maximum movement range per turn
    private int currentActions;
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
        if (roundManager.isCombatActive.Value && isInTurn)
        {
            currentActions = maxActions;
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

        // If we have reached our desired target and still have remaining actions, then we can shoot
        if (currentActions != 0)
        {
            Debug.Log($"!!!!!!SHOOOOOTING!!!!!! {currentActions} times!!");
            Shoot();
        }
        else
        {
            roundManager.EndCurrentTurn();
        }
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
        
        // 3. Use A* to find path and move towards target point 
        Path pathPlayer2PlayerGoal = ABPath.Construct(playerPos, playerGoalPos, null);
        AstarPath.StartPath(pathPlayer2PlayerGoal);
        pathPlayer2PlayerGoal.BlockUntilCalculated();
        
        // Compute the farthest point, so enemy can walk towards it 
        var farthestPointFromPlayerInEnemyShootingRange = pathPlayer2PlayerGoal.vectorPath
            .Take(pathPlayer2PlayerGoal.vectorPath.Count - 1)  // Exclude last element
            .Where((item, index) => 
                Vector3.Distance(pathPlayer2PlayerGoal.vectorPath[index + 1], playerPos) > shootingDistance)
            .FirstOrDefault();
        
        // can enemy walk towards that point, considering the remaining action points
        Path pathEnemy2DesiredTarget = ABPath.Construct(transform.position, farthestPointFromPlayerInEnemyShootingRange, null);
        AstarPath.StartPath(pathEnemy2DesiredTarget);
        pathEnemy2DesiredTarget.BlockUntilCalculated();

        float travelDistance = 0f;
        
        if (pathEnemy2DesiredTarget.vectorPath.Count >= 2)
        {
            for (int i = 0; i < pathEnemy2DesiredTarget.vectorPath.Count - 1; i++)
            {
                var segment = Vector3.Distance(pathEnemy2DesiredTarget.vectorPath[i + 1],
                    pathEnemy2DesiredTarget.vectorPath[i]);
                if (Mathf.CeilToInt(segment + travelDistance) <= maxActions)
                {
                    travelDistance += segment;
                    currentActions -= Mathf.CeilToInt(segment);
                }
                else
                {
                    currentActions -= Mathf.CeilToInt(travelDistance);
                    roundManager.DecrementActions(Mathf.CeilToInt(travelDistance));
                    return pathEnemy2DesiredTarget.vectorPath[i];
                }
            }    
        }
        currentActions -= Mathf.CeilToInt(travelDistance);
        roundManager.DecrementActions(Mathf.CeilToInt(travelDistance));
        return pathEnemy2DesiredTarget.vectorPath[^1];
    }

    private bool InShootingRangeTowardsPlayer(Vector3 pos)
    {
        return Vector3.Distance(pos, player.transform.position) >= shootingDistance;
    }
    
    /*
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
    */
    
    private void UseRemainingActionsForShooting(Vector3 finalPosition)
    {
        Debug.Log("UseRemainingActionsForShooting!");
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

    /// <summary>
    /// Enemy starts shooting if he is in his desired position and has nothing else to do.
    /// </summary>
    private void Shoot()
    {
        Vector3 direction = (player.position - transform.position).normalized;

        Vector3 offset = direction * 1.5f;
        
        // Perform initial shot
        GameObject bulletObject = Instantiate(bulletPrefab, transform.position + offset, Quaternion.identity);
        Physics.IgnoreCollision(bulletObject.GetComponent<Collider>(), GetComponentInChildren<Collider>());
        Destroy(bulletObject, 5f);
        
        direction.y = 0.01f;
        bulletObject.GetComponent<Rigidbody>().velocity = direction * bulletSpeed;

        roundManager.DecrementActions(1);
        Debug.Log("Bullet shot successfully!");

        // If we still have actions, schedule next shot
        if (roundManager.remainingActions > 0)
        {
            Observable.Timer(TimeSpan.FromSeconds(2))
                .Subscribe(_ => Shoot())
                .AddTo(this);
        }
        else
        {
            roundManager.EndCurrentTurn();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enemy shot.");

        // Enemy can die in explosion or by bullet.
        if (other.CompareTag("ExplosionTrigger") || other.CompareTag("Bullet"))
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
}
