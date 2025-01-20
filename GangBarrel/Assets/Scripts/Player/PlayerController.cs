using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Inventory;
using Pathfinding;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;
    public Camera mainCamera;
    public AIDestinationSetter aiDestinationSetter;
    public InventoryManager inventoryManager;
    public GameObject worldSpaceCanvas;
    public RoundManager roundManager;
    public LineRenderer lineRenderer;
    public FuseManager fuseManager;
    public Animator animator;
    public Rigidbody rigidbody;
    private PromptManager promptManager;
    private PlayerUI playerUI;
    public Button moveShootToggle;
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI buttonTextMesh;
    [SerializeField] private TextMeshProUGUI currentModeTextMesh;
    public TextMeshProUGUI distanceTextPrefab;
    
    [Header("Settings")]
    public float bulletSpeed = 15;
    public int maxActions = 5;
    private int remainingActions; // Track actions left this turn
    public bool isInTurn;
    public ReactiveProperty<PlayerState> currentState;
    public ReactiveProperty<int> health = new ReactiveProperty<int>(3); // 3/3 lives, if reaching 0, game over

    [Header("Plank Placement")]
    //[SerializeField] private GameObject plankPrefab;
    [SerializeField] private Grid tilemapGrid;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            health.Value--;
            Debug.Log("!!!!!!!!!!Player was hit by a bullet.");
        }
    }

    public enum PlayerState
    {
        Walk,
        Idle, 
        Shoot
    }
    
    private bool shootMode = true;
    private bool isPlacing = false;
    private bool distanceFrozen = false;
    
    private TextMeshProUGUI distanceTextInstance;
    private GameObject plankInstance;

    private void Awake()
    {
        playerUI = GetComponent<PlayerUI>();
        mainCamera = Camera.main;
        promptManager = FindObjectOfType<PromptManager>();
        currentState = new ReactiveProperty<PlayerState>(PlayerState.Idle);
        fuseManager = FindObjectOfType<FuseManager>();
        tilemapGrid = FindObjectOfType<Grid>();

        moveShootToggle.onClick.AddListener(ToggleMode);
        
        currentState.Subscribe(newState =>
        {
            switch (newState)
            {   
                case PlayerState.Idle:
                    animator.Play("Idle");
                    break;
                case PlayerState.Shoot:
                    animator.Play("Shoot");
                    break;
                case PlayerState.Walk:
                    animator.Play("walk");
                    break;
                default:
                    Debug.LogError("This is not a valid animation!");
                    break;
            }
        });
        
        health.Subscribe(newValue =>
        {
            playerUI.UpdateLifeUI(newValue);
            if (newValue == 0)
            {
                Time.timeScale = 0f;
                promptManager.ShowGameOverScreen();
            }
        });
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        
        HandleMouseInput();

        HandlePlayerState();
    }

    private void HandlePlayerState()
    {
        if (!shootMode)
        {
            if (rigidbody.velocity.magnitude > 0.1f)
            {
                currentState.Value = PlayerState.Walk;
            }
            else
            {
                currentState.Value = PlayerState.Idle;
            }   
        }
    }
    
    #region RoundBased-stuff
    
    /// <summary>
    /// When entering a combat zone, we want the player to stop at the border of the combat zone.
    /// From the player has a limited range to go.
    /// </summary>
    public void StopMovement()
    {
        aiDestinationSetter.target = transform;
        //StartCoroutine(SmoothStop());
        distanceFrozen = false;
    }

    private IEnumerator SmoothStop()
    {
        var agent = GetComponent<AIPath>(); // Get AIPath component
        var aiDestinationSetter = GetComponent<AIDestinationSetter>();

        // Validate components
        if (agent == null || aiDestinationSetter == null) yield break;

        float originalSpeed = agent.maxSpeed;
        float stopDuration = 1f; // Duration to decelerate
        float elapsedTime = 0f;

        // Gradually reduce speed to zero
        while (elapsedTime < stopDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / stopDuration;

            // Smoothly reduce maxSpeed
            agent.maxSpeed = Mathf.Lerp(originalSpeed, 0, t);

            yield return null;
        }

        // Reset the destination to prevent unwanted movement
        aiDestinationSetter.target = transform;
        
        // Restore maxSpeed to 1 (or any desired default value)
        agent.maxSpeed = 1f;
        
    }

    
    #endregion
    
    private void HandleMouseInput()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        
        if (roundManager.isCombatActive.Value && !isInTurn)
            return;

        Debug.Log("!!HandleMouseInput()");

        
        Vector3 mousePosition = GetMouseWorldPosition();
        float distance = CalculatePathDistance(transform.position, mousePosition);

        // Normal movement/shooting handling
        if (Input.GetMouseButtonDown(0))
        {
            if (shootMode)
            {
                ShootBullet(mousePosition);
            }
            else // Walk Mode
            {
                HandleWalkMode(mousePosition, distance);
            }
        }
        else if (!distanceFrozen && !shootMode)
        {
            UpdatePathVisualization(mousePosition);
            UpdateDistanceText(mousePosition, distance);
        }
    }

    /// <summary>
    /// Toggles between Shoot and Walk modes.
    /// </summary>
    public void ToggleMode()
    {
        shootMode = !shootMode;

        buttonTextMesh.text = shootMode ? "Move" : "Shoot";
        currentModeTextMesh.text = shootMode ? "Currently: Shoot Mode" : "Currently: Move Mode";
        currentModeTextMesh.color = shootMode ? Color.red : Color.green;
    }

    private void HandleWalkMode(Vector3 mousePosition, float distance)
    {
        if (CanTraversePath(transform.position, mousePosition))
        {
            distanceFrozen = true;

            if (roundManager.isCombatActive.Value && isInTurn)
            {
                if (!IsDistanceWithinRemainingActions(distance))
                    return;
            
                roundManager.DecrementActions(Mathf.CeilToInt(distance));
            }
        
            // Clear any existing distance text
            if (distanceTextInstance != null)
            {
                Destroy(distanceTextInstance.gameObject);
                distanceTextInstance = null;
            }
            // Set a new AI target and display updated distance text
            SetAITarget(mousePosition);
            UpdateDistanceText(mousePosition, distance);
        }
    }


    private async void ShootBullet(Vector3 targetPosition)
    {
        var bulletItem = inventoryManager.items.FirstOrDefault(item => item.itemType == Item.ItemType.Bullet);

        if (bulletItem != null)
        {
            GameObject bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Physics.IgnoreCollision(bulletObject.GetComponent<Collider>(), GetComponentInChildren<Collider>());
            Destroy(bulletObject, 5f);

            currentState.Value = PlayerState.Shoot;

            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0.4f;
            bulletObject.GetComponent<Rigidbody>().velocity = direction * bulletSpeed;

            inventoryManager.RemoveItem(bulletItem);
            roundManager.DecrementActions(1);
            Debug.Log("Bullet shot successfully!");

            // Wait for 2 seconds
            await Task.Delay(2000);

            // Set the state back to Idle
            currentState.Value = PlayerState.Idle;
            Debug.Log("Player state set back to Idle.");
        }
        else
        {
            Debug.LogWarning("No bullets left in the inventory.");
        }
    }

    private void UpdatePathVisualization(Vector3 targetPosition)
    {
        if (!CanTraversePath(transform.position, targetPosition))
        {
            ClearLineRenderer();
            return;
        }

        Path path = ABPath.Construct(transform.position, targetPosition);
        AstarPath.StartPath(path);
        path.BlockUntilCalculated();
        
        var distance = CalculatePathDistance(transform.position, targetPosition);
        if (roundManager.isCombatActive.Value)
        {
            if (IsDistanceWithinRemainingActions(distance))
            {
                lineRenderer.startColor = Color.green;
                lineRenderer.endColor = Color.green;
            }
            else
            {
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;
            }
        }
        
        if (!path.error)
        {
            DrawPath(path.vectorPath.ToArray());
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
    
    private void UpdateDistanceText(Vector3 targetPosition, float distance)
    {
        if (distanceTextInstance == null)
        {
            // Instantiate the text as a child of the worldSpaceCanvas
            distanceTextInstance = Instantiate(distanceTextPrefab, worldSpaceCanvas.transform);
            distanceTextInstance.color = Color.green;
        }

        distanceTextInstance.color = (roundManager.isCombatActive.Value && IsDistanceWithinRemainingActions(distance)) ? Color.green : Color.red;

        // Set the position in world space, adding a Y-offset to position it above the target position
        Vector3 textPosition = targetPosition + new Vector3(0, 1.0f, 0); // Adjust Y-offset as needed
        distanceTextInstance.transform.position = textPosition;

        // Make the text face the camera
        distanceTextInstance.transform.LookAt(mainCamera.transform);
        distanceTextInstance.transform.Rotate(0, 180, 0); // Rotate 180 degrees if the text faces away

        // Update the text content
        distanceTextInstance.text = $"{distance:F1} meters";
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

        // Destroy the distance text instance
        if (distanceTextInstance != null)
        {
            Destroy(distanceTextInstance.gameObject);
            distanceTextInstance = null;
        }

        // Reset distanceFrozen to allow new pathfinding
        distanceFrozen = false;

        // Clear the path visualization
        ClearLineRenderer();
    }

    
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

        if (distanceTextInstance != null)
        {
            distanceTextInstance.text += " (Locked)";
        }
    }

    private float CalculatePathDistance(Vector3 start, Vector3 end)
    {
        Path path = ABPath.Construct(start, end);
        AstarPath.StartPath(path);
        path.BlockUntilCalculated();

        if (path.error) return 0f;

        float totalDistance = 0f;
        for (int i = 1; i < path.vectorPath.Count; i++)
        {
            totalDistance += Vector3.Distance(path.vectorPath[i - 1], path.vectorPath[i]);
        }

        return totalDistance;
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

    private Vector3 GetMouseWorldPosition()
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        return plane.Raycast(ray, out float distance) ? ray.GetPoint(distance) : Vector3.zero;
    }

    private void ClearLineRenderer()
    {
        if (lineRenderer)
        {
            lineRenderer.positionCount = 0;
        }
    }

    private void DrawPath(Vector3[] pathPoints)
    {
        if (lineRenderer)
        {
            lineRenderer.positionCount = pathPoints.Length;
            lineRenderer.SetPositions(pathPoints);
        }
    }
    
}
