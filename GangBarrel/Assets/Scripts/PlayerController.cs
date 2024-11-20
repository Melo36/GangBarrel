using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Camera mainCamera;
    public AIDestinationSetter aiDestinationSetter;
    public Tilemap tileMap; // Reference to your Tilemap component

    private float speed = 10;
    
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
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            
            // Check if the click was on the tilemap
            if (tileMap != null && IsClickOnTilemap(mousePosition))
            {
                Debug.Log($"CanTraversePath = {CanTraversePath(transform.position, mousePosition)}");
                if (CanTraversePath(transform.position, mousePosition))
                {
                    Debug.Log("This path is not traversable. Either game is lost or you need to find another way.");
                    SetAITarget(mousePosition);   
                }
            }
            else
            {
                ShootBullet(mousePosition);
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

    bool IsClickOnTilemap(Vector3 worldPosition)
    {
        Vector3Int cellPosition = tileMap.WorldToCell(worldPosition);
        return tileMap.HasTile(cellPosition);
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
        GameObject bullet = Instantiate(bulletPrefab);
        Destroy(bullet, 5f);
        
        mousePosition = new Vector3(mousePosition.x, transform.position.y, mousePosition.z);
        Vector3 direction = (mousePosition - transform.position).normalized;
        bullet.GetComponent<Rigidbody>().AddForce(direction * speed, ForceMode.VelocityChange);
    }
}
