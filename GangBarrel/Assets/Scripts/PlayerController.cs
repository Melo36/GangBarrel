using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;
    public Camera mainCamera;
    public AIDestinationSetter aiDestinationSetter;
    public Tilemap tileMap;
    public Inventory inventory;
    public GameObject worldSpaceCanvas;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI buttonTextMesh;
    [SerializeField] private TextMeshProUGUI currentModeTextMesh;
    public TextMeshProUGUI distanceTextPrefab;

    [Header("Settings")]
    public float bulletSpeed = 15;

    [Header("Plank Placement")]
    [SerializeField] private GameObject plankPrefab;
    [SerializeField] private Grid tilemapGrid;

    private bool shootMode = true;
    private bool isPlacing = false;
    private bool distanceFrozen = false;

    private TextMeshProUGUI distanceTextInstance;
    private GameObject plankInstance;

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (isPlacing)
        {
            UpdatePlacement();
        }

        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = GetMouseWorldPosition();

            if (shootMode)
            {
                ShootBullet(mousePosition);
            }
            else
            {
                HandleWalkMode(mousePosition);
            }
        }
        else if (!distanceFrozen && !shootMode)
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            UpdatePathVisualization(mousePosition);
            UpdateDistanceText(mousePosition);
        }
    }

    /// <summary>
    /// Toggles between Shoot and Walk modes.
    /// </summary>
    public void ToggleMode()
    {
        shootMode = !shootMode;

        buttonTextMesh.text = shootMode ? "Move" : "Shoot";
        currentModeTextMesh.text = shootMode ? "Currently: Move Mode" : "Currently: Shoot Mode";
        currentModeTextMesh.color = shootMode ? Color.green : Color.red;

        Debug.Log($"Mode changed to: {(shootMode ? "Shoot Mode" : "Move Mode")}");
    }

    private void HandleWalkMode(Vector3 mousePosition)
    {
        if (CanTraversePath(transform.position, mousePosition))
        {
            distanceFrozen = true;
            SetAITarget(mousePosition);
        }
    }

    private void ShootBullet(Vector3 targetPosition)
    {
        var bulletItem = inventory.items.FirstOrDefault(item => item.itemType == Item.ItemType.Bullet);

        if (bulletItem != null)
        {
            GameObject bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Physics.IgnoreCollision(bulletObject.GetComponent<Collider>(), GetComponentInChildren<Collider>());
            Destroy(bulletObject, 5f);

            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0.01f;
            bulletObject.GetComponent<Rigidbody>().velocity = direction * bulletSpeed;

            inventory.RemoveItem(bulletItem);
            Debug.Log("Bullet shot successfully!");
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

        if (!path.error)
        {
            DrawPath(path.vectorPath.ToArray());
        }
    }

    private void UpdateDistanceText(Vector3 targetPosition)
    {
        float distance = CalculatePathDistance(transform.position, targetPosition);

        if (distanceTextInstance == null)
        {
            // Instantiate the text as a child of the worldSpaceCanvas
            distanceTextInstance = Instantiate(distanceTextPrefab, worldSpaceCanvas.transform);
        }

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
        // Wait until the player is close enough to the target
        while (Vector3.Distance(transform.position, tempTarget.transform.position) > 0.5f)
        {
            yield return null; // Wait for the next frame
        }

        // Destroy the temporary target object
        Destroy(tempTarget);

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

        Debug.Log("Target reached. Distance text destroyed.");
    }

    
    private void SetAITarget(Vector3 targetPosition)
    {
        GameObject tempTarget = new GameObject("TempTarget");
        tempTarget.transform.position = targetPosition;

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
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer)
        {
            lineRenderer.positionCount = 0;
        }
    }

    private void DrawPath(Vector3[] pathPoints)
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer)
        {
            lineRenderer.positionCount = pathPoints.Length;
            lineRenderer.SetPositions(pathPoints);
        }
    }

    private void UpdatePlacement()
    {
        if (!plankInstance) return;

        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Vector3Int gridCell = tilemapGrid.WorldToCell(mouseWorldPosition);
        Vector3 snappedPosition = tilemapGrid.GetCellCenterWorld(gridCell);

        snappedPosition.y = plankInstance.transform.position.y;
        plankInstance.transform.position = snappedPosition;

        if (Input.GetKeyDown(KeyCode.Escape)) CancelPlankPlacement();

        if (Input.GetMouseButtonDown(0)) PlacePlank(gridCell);
    }

    public void StartPlankPlacement()
    {
        if (isPlacing) return;

        isPlacing = true;
        plankInstance = Instantiate(plankPrefab);
        plankInstance.AddComponent<Blinking>();
    }

    private void PlacePlank(Vector3Int gridCell)
    {
        // Place the plank in the game world
        Destroy(plankInstance.GetComponent<Blinking>());
        plankInstance.transform.position = tilemapGrid.GetCellCenterWorld(gridCell);
        plankInstance = null;
        isPlacing = false;

        // Update the graph to make the cell walkable
        UpdateGraphAtPosition(tilemapGrid.GetCellCenterWorld(gridCell));

        Debug.Log("Plank placed successfully!");
    }
    
    private void UpdateGraphAtPosition(Vector3 position)
    {
        // Define the bounds of the area to update
        Bounds bounds = new Bounds(position, new Vector3(1, 2, 1)); // Adjust size as needed

        // Create a GraphUpdateObject (GUO) for updating the graph
        GraphUpdateObject guo = new GraphUpdateObject(bounds);

        // Set the GUO to modify the walkability
        guo.modifyWalkability = true;
        guo.setWalkability = true;

        // Optionally, you can set the tag or penalty if needed
        // guo.tag = 1; // For example, set a tag for the plank area
        // guo.penalty = 0; // Adjust the penalty if required

        // Apply the GUO
        AstarPath.active.UpdateGraphs(guo);

        // If you want to force the update immediately (synchronously), uncomment the following line:
        // AstarPath.active.FlushGraphUpdates();

        Debug.Log("Graph updated at position: " + position);
    }



    private void CancelPlankPlacement()
    {
        if (plankInstance) Destroy(plankInstance);
        plankInstance = null;
        isPlacing = false;
        Debug.Log("Plank placement canceled.");
    }
}
