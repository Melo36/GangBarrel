using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using Pathfinding;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Every item has a different way of being used.
/// This class is responsible for handling the different behaviours
/// </summary>
public class ItemUsage : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public InventoryManager inventoryManager;
    public Camera mainCamera;
    public Tilemap tilemapGrid;
    
    private Item itemToPlace;
    private GameObject placementObject;
    
    // replace by the prefab object in the item
    public GameObject plankPrefab;
    public GameObject barrelPrefab;
    
    private GameObject plankInstance;
    
    private bool isPlacing;

    private void Update()
    {
        if (isPlacing)
        {
            UpdatePlacement();
        }
    }

    public void UseItem(Item item)
    {
        itemToPlace = item;
        switch (item.itemType)
        {
            case Item.ItemType.Barrel:
                Debug.Log("Place Barrel");
                break;
            case Item.ItemType.Bullet:
                Debug.Log("Shoot Bullet");
                break;
            case Item.ItemType.Plank:
                Debug.Log("Place Plank");
                break;
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
        plankInstance.transform.position -= new Vector3(0, 0.0f, 0f);
        plankInstance = null;
        isPlacing = false;

        var plank = inventoryManager.items.FirstOrDefault(item => item.itemType == Item.ItemType.Plank);
        
        // Update the graph to make the cell walkable
        UpdateGraphAtPosition(tilemapGrid.GetCellCenterWorld(gridCell));
        inventoryManager.RemoveItem(plank);

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
 
    private Vector3 GetMouseWorldPosition()
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        return plane.Raycast(ray, out float distance) ? ray.GetPoint(distance) : Vector3.zero;
    }
    
}
