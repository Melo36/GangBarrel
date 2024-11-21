using System;
using System.Collections;
using System.Collections.Generic;
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
        Plane plane = new Plane(Vector3.up, 0);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
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

        return false;
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
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        
        GameObject bullet = Instantiate(bulletPrefab);
        Destroy(bullet, 5f);

        mousePosition = new Vector3(mousePosition.x, mousePosition.y, mousePosition.z);
        Vector3 direction = (mousePosition - transform.position).normalized;
        direction.Set(direction.x, 0.01f, direction.z);
        bullet.GetComponent<Rigidbody>().velocity = direction * bulletSpeed;
    }
}
