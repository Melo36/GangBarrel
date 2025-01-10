using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pathfinding;
using Inventory;

public class FuseManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject fusePrefab;
    [SerializeField] private Material fuseMaterial;
    [SerializeField] private GameObject fuseEndpointPrefab;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private InventoryManager playerInventory;
    public Item fuseItem;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject fPromptPrefab;
    [SerializeField] private float holdDuration = 2f;
    
    [SerializeField] private Image fillBar; // Changed from fillImage to fillBar for clarity

    [SerializeField] private GameObject worldSpaceCanvas;
    [SerializeField] private float promptHeightOffset = 1.0f;
    
    private GameObject currentPrompt;
    private Image fillImage;
    private float currentHoldTime;
    private bool isHoldingF;
    
    [Header("Settings")]
    [SerializeField] private float defaultBurnSpeed = 1f;
    [SerializeField] private float maxFuseLength = 10f;
    
    [Header("Preview Settings")]
    [SerializeField] private Color validPathColor = Color.green;
    [SerializeField] private Color invalidPathColor = Color.red;
    [SerializeField] private float previewLineHeight = 0.1f;
    
    private Vector3 lockedPlacementPosition; // Store position when F is pressed
    
    private List<Fuse> activeFuses = new List<Fuse>();
    private bool isPreviewValid;
    private BarrelExplosionController startBarrel;
    private Camera mainCamera;
    private Vector3 lastMousePosition;

    public GameObject[] explosiveBarrels;
    
    private FuseEndpoint startEndpoint; // New field
    
    public enum CurrentState
    {
        Init = 0,
        WaitStartPoint = 1,
        WaitEndPoint = 2
    }

    private CurrentState _currentState;
    public CurrentState currentState
    {
        get => _currentState;
        set
        {
            _currentState = value;
            if (_currentState != CurrentState.WaitEndPoint) {
                ClearPreview();
                ClearPrompt();
            }
        }
    }

    private void Awake()
    {
        explosiveBarrels = GameObject.FindGameObjectsWithTag("Barrel");
        mainCamera = Camera.main;
        
        if (fusePrefab == null)
        {
            Debug.LogError("Fuse Prefab is not assigned to FuseManager!");
        }

        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        
        currentState = CurrentState.Init;
    }

    public void StartFuseFromEndpoint(FuseEndpoint endpoint)
    {
        startEndpoint = endpoint;
        currentState = CurrentState.WaitEndPoint;
    
        // Use the exact endpoint position for the start of the new fuse
        Vector3 startPos = endpoint.GetPosition();
        lastMousePosition = startPos;
    
        // Initial preview update from the exact position
        UpdateFusePreview(GetMouseWorldPosition());
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == CurrentState.WaitEndPoint)
            {
                if (playerInventory != null)
                {
                    playerInventory.AddItem(new Item { itemType = Item.ItemType.Fuse });
                }
                currentState = CurrentState.Init;
            }
        }

        if (currentState == CurrentState.WaitEndPoint)
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            
            // Only update preview if not holding F
            if (!isHoldingF)
            {
                if (mousePosition != lastMousePosition)
                {
                    UpdateFusePreview(mousePosition);
                    UpdatePromptPosition(mousePosition);
                    lastMousePosition = mousePosition;
                }
            }

            // Handle F key hold
            if (Input.GetKeyDown(KeyCode.F) && isPreviewValid)
            {
                StartHolding();
            }
            else if (Input.GetKey(KeyCode.F) && isHoldingF)
            {
                UpdateHolding();
            }
            else if (Input.GetKeyUp(KeyCode.F))
            {
                CancelHolding();
            }
        }
    }

    private void StartHolding()
    {
        isHoldingF = true;
        currentHoldTime = 0f;
        if (fillBar != null)
        {
            fillBar.fillAmount = 0f;
        }
    }

    private void UpdateHolding()
    {
        currentHoldTime += Time.deltaTime;
        if (fillBar != null)
        {
            fillBar.fillAmount = currentHoldTime / holdDuration;
        }

        if (currentHoldTime >= holdDuration)
        {
            CompletePlacement();
        }
    }

    private void CancelHolding()
    {
        isHoldingF = false;
        currentHoldTime = 0f;
        if (fillBar != null)
        {
            fillBar.fillAmount = 0f;
        }
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        return plane.Raycast(ray, out float distance) ? ray.GetPoint(distance) : Vector3.zero;
    }

    private void UpdateFusePreview(Vector3 targetPosition)
    {
        if ((startBarrel == null && startEndpoint == null) || lineRenderer == null) return;

        Vector3 startPos = startEndpoint != null ? startEndpoint.GetPosition() : startBarrel.transform.position;
    
        var startNodeInfo = AstarPath.active.GetNearest(startPos, NNConstraint.Default);
        if (startNodeInfo.node == null) return;
        
        if (!CanTraversePath(startNodeInfo.position, targetPosition))
        {
            ClearPreview();
            return;
        }
        
        var endNodeInfo = AstarPath.active.GetNearest(targetPosition, NNConstraint.Default);
        if (endNodeInfo.node == null) return;

        lockedPlacementPosition = endNodeInfo.position;
        
        Path path = ABPath.Construct(startNodeInfo.position, lockedPlacementPosition);
        AstarPath.StartPath(path);
        path.BlockUntilCalculated();

        Debug.Log($"(FuseManager)path.vectorPath.Count = {path.vectorPath.Count}");
        
        if (!path.error)
        {
            float pathLength = CalculatePathLength(path.vectorPath);
            bool isWithinRange = pathLength <= maxFuseLength;

            lineRenderer.enabled = true;
        
            if (isWithinRange)
            {
                lineRenderer.startColor = validPathColor;
                lineRenderer.endColor = validPathColor;
                isPreviewValid = true;
            }
            else
            {
                lineRenderer.startColor = invalidPathColor;
                lineRenderer.endColor = invalidPathColor;
                isPreviewValid = false;
            }

            DrawPreviewPath(path.vectorPath.ToArray(), startPos);
        }
    }

    private void UpdatePromptPosition(Vector3 worldPosition)
    {
        if (currentPrompt == null)
        {
            // Instantiate as child of world space canvas
            currentPrompt = Instantiate(fPromptPrefab, worldSpaceCanvas.transform);
            fillBar = currentPrompt.transform.Find("Fillbar").GetComponent<Image>();
            if (fillBar == null)
            {
                Debug.LogError("Fillbar Image component not found in fPromptPrefab!");
            }
        }

        // Set the position in world space with height offset
        Vector3 promptPosition = worldPosition + new Vector3(0, promptHeightOffset, 0);
        currentPrompt.transform.position = promptPosition;

        // Make the prompt face the camera
        currentPrompt.transform.LookAt(mainCamera.transform);
        currentPrompt.transform.Rotate(0, 180, 0); // Rotate to face camera correctly
    }


    private void CompletePlacement()
    {
        if (!isPreviewValid) return;
        
        TryPlaceFuseEndpoint(lockedPlacementPosition); // Use locked position instead of last mouse position
        ClearPrompt();
        isHoldingF = false;
    }

    private void TryPlaceFuseEndpoint(Vector3 position)
    {
        if (!isPreviewValid) return;

        Vector3 startPos;
        if (startEndpoint != null)
        {
            startPos = startEndpoint.GetPosition();
        }
        else if (startBarrel != null)
        {
            startPos = startBarrel.transform.position;
        }
        else
        {
            return;
        }

        var startNodeInfo = AstarPath.active.GetNearest(startPos, NNConstraint.Default);
        if (startNodeInfo.node == null) return;

        Path path = ABPath.Construct(startPos, position);
        AstarPath.StartPath(path);
        path.BlockUntilCalculated();

        if (path.error) return;

        Vector3[] pathPoints = new Vector3[path.vectorPath.Count];
        //pathPoints[0] = startPos; // Start from actual start position
        for (int i = 0; i < path.vectorPath.Count; i++)
        {
            pathPoints[i] = path.vectorPath[i];
        }

        // Use the last point of the path for the endpoint position
        Vector3 endpointPosition = pathPoints[pathPoints.Length - 1];
        
        GameObject endpointObj = Instantiate(fuseEndpointPrefab, endpointPosition, Quaternion.identity);
        FuseEndpoint endpoint = endpointObj.GetComponent<FuseEndpoint>();

        Fuse fuse = CreateFinalFuse(pathPoints);
    
        if (endpoint != null && fuse != null)
        {
            endpoint.Initialize(fuse);
            endpointObj.transform.SetParent(fuse.transform);

            // If we started from another endpoint, connect the fuses
            if (startEndpoint != null)
            {
                startEndpoint.SetConnectedFuse(fuse);
            }
        }
        
        ClearPreview();
        currentState = CurrentState.Init;
        startEndpoint = null; // Reset start endpoint
    }

    private void DrawPreviewPath(Vector3[] points, Vector3 startPos)
    {
        if (lineRenderer == null) return;

        Vector3[] pathWithStart = new Vector3[points.Length + 1];
        pathWithStart[0] = startPos + Vector3.up * previewLineHeight;
    
        for (int i = 0; i < points.Length; i++)
        {
            pathWithStart[i + 1] = points[i] + Vector3.up * previewLineHeight;
        }

        lineRenderer.positionCount = pathWithStart.Length;
        lineRenderer.SetPositions(pathWithStart);
    }

    private float CalculatePathLength(System.Collections.Generic.List<Vector3> pathPoints)
    {
        float length = 0f;
        for (int i = 1; i < pathPoints.Count; i++)
        {
            length += Vector3.Distance(pathPoints[i - 1], pathPoints[i]);
        }
        return length;
    }

    private bool CanTraversePath(Vector3 start, Vector3 end)
    {
        var startNodeInfo = AstarPath.active.GetNearest(start, NNConstraint.Default);
        var endNodeInfo = AstarPath.active.GetNearest(end, NNConstraint.Default);
        
        if (startNodeInfo.node == null || endNodeInfo.node == null)
        {
            return false;
        }

        return PathUtilities.IsPathPossible(startNodeInfo.node, endNodeInfo.node);
    }

    // In FuseManager
    public void SetStartBarrel(BarrelExplosionController barrel)
    {
        startBarrel = barrel;
        Debug.Log($"Set start barrel: {barrel.name}");
    }

    private void ClearPreview()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    private void ClearPrompt()
    {
        if (currentPrompt != null)
        {
            Destroy(currentPrompt);
            currentPrompt = null;
            fillBar = null;
        }
    }

    public Fuse CreateFinalFuse(Vector3[] pathPoints)
    {
        ClearPreview();
    
        if (fusePrefab == null) return null;

        GameObject fuseObj = Instantiate(fusePrefab);
        Fuse fuse = fuseObj.GetComponent<Fuse>();

        if (fuse == null)
        {
            Debug.LogError("Fuse component not found on fusePrefab!");
            Destroy(fuseObj);
            return null;
        }

        // Pass the startBarrel (which could be from a barrel or null if from endpoint)
        fuse.Initialize(pathPoints, startBarrel);
        activeFuses.Add(fuse);

        return fuse;
    }

    public void ClearAllFuses()
    {
        ClearPreview();
        
        foreach (var fuse in activeFuses)
        {
            if (fuse != null)
                Destroy(fuse.gameObject);
        }
        activeFuses.Clear();
    }

    private void OnDestroy()
    {
        ClearAllFuses();
        ClearPrompt();
    }
}