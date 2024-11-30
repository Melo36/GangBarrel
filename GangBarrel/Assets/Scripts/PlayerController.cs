using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Camera mainCamera;
    public AIDestinationSetter aiDestinationSetter;
    public Tilemap tileMap; // Reference to your Tilemap component

    public Inventory inventory;
    
    public float bulletSpeed = 15;

    private bool shootMode = true; // Start in shooting mode

    public bool CanTraversePath(Vector3 start, Vector3 end)
    {
        // Get the nearest nodes to the start and end positions
        GraphNode startNode = AstarPath.active.GetNearest(start).node;
        GraphNode endNode = AstarPath.active.GetNearest(end).node;

        // Check if both nodes are walkable
        if (startNode == null || endNode == null || !startNode.Walkable || !endNode.Walkable)
        {
            return false;
        }

        // Check if a path exists between the nodes
        return PathUtilities.IsPathPossible(startNode, endNode);
    }

    [SerializeField] private TextMeshProUGUI buttonTextMesh;
    [SerializeField] private TextMeshProUGUI currentModeTextMesh;
    
    /// <summary>
    /// Toggles the mode between shoot and walk. 
    /// </summary>
    public void ToggleMode()
    {
        buttonTextMesh.text = shootMode ? "Move" : "Shoot";
        currentModeTextMesh.text = shootMode ? "Currently: Shoot Mode" : "Currently: Move Mode";
        currentModeTextMesh.color = shootMode ? Color.red : Color.green;
        shootMode = !shootMode; // Toggle the mode
        Debug.Log($"Mode changed: {(shootMode ? "Shoot Mode" : "Move Mode")}");
    }
    
    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        
        if (isPlacing)
        {
            UpdatePlacement();
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = GetMouseWorldPosition();

            if (shootMode)
            {
                ShootBullet(mousePosition);
            }
            else
            {
                // Traverse logic: 
                Debug.Log($"CanTraversePath = {CanTraversePath(transform.position, mousePosition)}");
                if (CanTraversePath(transform.position, mousePosition))
                {
                    Debug.Log("This path is not traversable. Either game is lost or you need to find another way.");
                    SetAITarget(mousePosition);
                }
            
                // Check if the click was on the tilemap
                if (tileMap != null && IsClickOnTilemap(mousePosition, out CustomTile clickedTile))
                {
                    // click tile logic
                    if (clickedTile != null)
                    {
                        Debug.Log($"clickedTile.GetNeighborCountOfType(TileType.Water)): {clickedTile.HasNeighborOfType(0, TileType.Water)}");
                        Vector3Int tilePos = tileMap.WorldToCell(mousePosition);
                        clickedTile.DebugNeighbors(tilePos, tileMap);
                    }
                }
            }
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        // Project the mouse ray onto the y=0 plane
        Plane plane = new Plane(Vector3.up, Vector3.zero); // Plane at y=0
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero; // Fallback if no valid point is found
    }


    bool IsClickOnTilemap(Vector3 worldPosition, out CustomTile clickedTile)
    {
        clickedTile = null;

        Vector3Int cellPosition = tileMap.WorldToCell(worldPosition);

        // Check if a tile exists at the clicked position
        if (tileMap.HasTile(cellPosition))
        {
            TileBase tileBase = tileMap.GetTile(cellPosition);

            // Check if the tile is of type CustomTile
            if (tileBase is CustomTile customTile)
            {
                clickedTile = customTile;
                return true;
            }
        }

        return true;
    }

    void SetAITarget(Vector3 targetPosition)
    {
        // Create a temporary GameObject at the target position
        GameObject tempTarget = new GameObject("TempTarget");
        tempTarget.transform.position = targetPosition;

        // Set the AI destination to the temporary object's transform
        aiDestinationSetter.target = tempTarget.transform;

        // Optionally, destroy the temporary object after some time
        Destroy(tempTarget, 5f); // Adjust the time as needed
    }

    void ShootBullet(Vector3 mousePosition)
    {
        // Find the first bullet item in the inventory
        var bulletItem = inventory.items.FirstOrDefault(item => item.itemType == Item.ItemType.Bullet);

        if (bulletItem != null)
        {
            // Instantiate the bullet object
            GameObject bulletObject = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Physics.IgnoreCollision(bulletObject.GetComponent<Collider>(), GetComponentInChildren<Collider>());
            Destroy(bulletObject, 5f);

            // Calculate direction and set bullet velocity
            Vector3 direction = (mousePosition - transform.position).normalized;
            direction.Set(direction.x, 0.01f, direction.z);
            bulletObject.GetComponent<Rigidbody>().velocity = direction * bulletSpeed;

            // Remove the bullet item from the inventory
            inventory.RemoveItem(bulletItem);

            Debug.Log("Bullet shot successfully!");
        }
        else
        {
            Debug.LogWarning("No bullets left in the inventory.");
        }
    }

    
    [SerializeField] private GameObject plankPrefab; // Plank prefab
    [SerializeField] private Grid tilemapGrid; // Reference to the Tilemap's Grid component
    //[SerializeField] private float placementBlinkInterval = 0.5f; // Blinking interval

    private GameObject plankInstance; // The preview plank being placed
    private bool isPlacing = false;

    public void StartPlankPlacement()
    {
        if (isPlacing) return;

        isPlacing = true;

        // Instantiate a preview plank
        plankInstance = Instantiate(plankPrefab);
        plankInstance.AddComponent<Blinking>(); // Attach blinking script for visual feedback
    }
    
    private void UpdatePlacement()
    {
        if (!isPlacing || plankInstance == null) return;

        // Get the mouse position in world space
        Vector3 mouseWorldPosition = GetMouseWorldPosition();

        // Snap to the nearest grid cell
        Vector3Int gridCell = tilemapGrid.WorldToCell(mouseWorldPosition);
        Vector3 snappedPosition = tilemapGrid.GetCellCenterWorld(gridCell);

        // Adjust for the 3D environment (maintain the plank's height)
        snappedPosition.y = plankInstance.transform.position.y;

        // Update plank position
        plankInstance.transform.position = snappedPosition;

        // Handle orientation (optional)
        OrientPlank(gridCell);

        // Cancel placement if ESC is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlankPlacement();
        }

        // Place the plank when the left mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            PlacePlank(gridCell);
        }
    }

    /* Intention: Check if the plank can be placed there, i.e. if it is between to islands.
    private bool CanPlacePlank(Vector3Int gridCell)
    {
        var gridGraph = AstarPath.active.data.gridGraph;

        // Get the node at the current grid cell
        Vector3 cellWorldPosition = tilemapGrid.GetCellCenterWorld(gridCell);
        Vector3 graphSpacePosition = gridGraph.transform.InverseTransform(cellWorldPosition);
        int x = Mathf.RoundToInt(graphSpacePosition.x);
        int z = Mathf.RoundToInt(graphSpacePosition.z);

        // Check if the coordinates are within bounds
        if (x < 0 || x >= gridGraph.width || z < 0 || z >= gridGraph.depth) return false;

        var centerNode = gridGraph.GetNode(x, z);
        if (centerNode == null || centerNode.Walkable) return false; // Must be non-passable

        // Find passable neighbors
        List<GraphNode> passableNeighbors = new List<GraphNode>();
        centerNode.GetConnections((neighbor) =>
        {
            if (neighbor.Walkable)
            {
                passableNeighbors.Add(neighbor);
            }
        });

        // Ensure there are exactly two passable nodes
        return passableNeighbors.Count == 2;
    }
    */

    private void OrientPlank(Vector3Int gridCell)
    {
        var gridGraph = AstarPath.active.data.gridGraph;
        Vector3 cellWorldPosition = tilemapGrid.GetCellCenterWorld(gridCell);
        Vector3 graphSpacePosition = gridGraph.transform.InverseTransform(cellWorldPosition);
        int x = Mathf.RoundToInt(graphSpacePosition.x);
        int z = Mathf.RoundToInt(graphSpacePosition.z);

        var centerNode = gridGraph.GetNode(x, z);

        // Find the two passable neighbors
        List<Vector3Int> passableNeighbors = new List<Vector3Int>();
        centerNode.GetConnections((neighbor) =>
        {
            if (neighbor.Walkable)
            {
                var neighborPos = (GridNodeBase)neighbor;
                passableNeighbors.Add(new Vector3Int(neighborPos.XCoordinateInGrid, neighborPos.ZCoordinateInGrid, 0));
            }
        });

        if (passableNeighbors.Count == 2)
        {
            Vector3Int delta = passableNeighbors[1] - passableNeighbors[0];
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                // Horizontal
                plankInstance.transform.rotation = Quaternion.Euler(0, 90, 0);
            }
            else
            {
                // Vertical
                plankInstance.transform.rotation = Quaternion.identity;
            }
        }
    }

    private void PlacePlank(Vector3Int gridCell)
    {
        var gridGraph = AstarPath.active.data.gridGraph;
        Vector3 cellWorldPosition = tilemapGrid.GetCellCenterWorld(gridCell);
        Vector3 graphSpacePosition = gridGraph.transform.InverseTransform(cellWorldPosition);
        int x = Mathf.RoundToInt(graphSpacePosition.x);
        int z = Mathf.RoundToInt(graphSpacePosition.z);

        var centerNode = gridGraph.GetNode(x, z);

        if (centerNode != null)
        {
            Debug.Log($"x,z={x},{z}");
            Debug.Log($"centerNode.Walkable = {centerNode.Walkable}");

            // Make the node passable
            centerNode.Walkable = true;

            // Optional: Find the GameObject at this position and set its layer
            RaycastHit hit;
            if (Physics.Raycast(cellWorldPosition + Vector3.up * 10, Vector3.down, out hit, Mathf.Infinity))
            {
                GameObject hitObject = hit.collider.gameObject;

                // Change the layer of the object under the plank
                hitObject.layer = LayerMask.NameToLayer("Default");
                Debug.Log($"Changed layer of {hitObject.name} to Default.");
            }

            // Update the A* graph dynamically
            AstarPath.active.UpdateGraphs(new Bounds(cellWorldPosition, Vector3.one * gridGraph.nodeSize));
        }
        else
        {
            Debug.LogWarning($"Node at ({x}, {z}) not found in the grid.");
        }

        // Finalize plank placement
        Destroy(plankInstance.GetComponent<Blinking>()); // Remove blinking effect
        plankInstance = null;
        isPlacing = false;

        // Remove plank from inventory
        var plankItem = inventory.items.FirstOrDefault(item => item.itemType == Item.ItemType.Plank);
        if (plankItem != null)
        {
            inventory.RemoveItem(plankItem);
        }

        Debug.Log("Plank placed successfully!");
    }
    
    private void CancelPlankPlacement()
    {
        if (plankInstance != null)
        {
            Destroy(plankInstance);
        }
        plankInstance = null;
        isPlacing = false;
        Debug.Log("Plank placement canceled.");
    }
    
}
