using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using Pathfinding;
using Unity.VisualScripting;
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
    
    private GameObject placementObjectInstance;
    
    private bool isPlacing;

    private Item placingItem;
    
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
        if (!placementObjectInstance) return;

        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Vector3Int gridCell = tilemapGrid.WorldToCell(mouseWorldPosition);
        Vector3 snappedPosition = tilemapGrid.GetCellCenterWorld(gridCell);

        snappedPosition.y = placementObjectInstance.transform.position.y;
        placementObjectInstance.transform.position = snappedPosition;

        if (Input.GetKeyDown(KeyCode.Escape)) CancelItemPlacement();

        if (Input.GetMouseButtonDown(0)) PlaceItem(gridCell);
    }

    /// <summary>
    /// For items that can be placed, this function makes sense.
    ///
    /// (The (bullet) shooting is implemented in the PlayerController, even though it is an item.)
    /// 
    /// </summary>
    /// <param name="item"></param>
    public void StartItemUsage(Item item)
    {
        if (isPlacing) return;
        
        if (item.itemType is Item.ItemType.Bullet)
            return;

        if (item.itemType is Item.ItemType.Fuse)
        {
            // TODO: 1. Let the barrels blink
            // TODO: 2. Wait for click on one of those barrels or wait for exit action (esc)
            // TODO: 3. When any barrel has been clicked the player can move the fuse and it is visualized (use Fuse.cs here as well)
            // TODO: 4. If range is not exceeded and player does not exit action, pressing on a walkable tile, which is not interrupted creates the fuse.
        }
        
        var placementPrefab = item.itemPrefab;
        placingItem = item;
        
        isPlacing = true;
        placementObjectInstance = Instantiate(placementPrefab);
        placementObjectInstance.AddComponent<Blinking>();
    }

    private void PlaceItem(Vector3Int gridCell)
    {
        // Place the item in the game world
        Destroy(placementObjectInstance.GetComponent<Blinking>());
        placementObjectInstance.transform.position = tilemapGrid.GetCellCenterWorld(gridCell);
        placementObjectInstance.transform.position -= new Vector3(0, 0.0f, 0f);
        
        var item = inventoryManager.items.FirstOrDefault(i => i.itemType == placingItem.itemType);
        
        if (item != null && item.itemType == Item.ItemType.Plank)
        {
            UpdateGraphAtPosition(tilemapGrid.GetCellCenterWorld(gridCell), true);
            Destroy(placementObjectInstance.GetComponent<Collectible>());
        }
        
        placementObjectInstance = null;
        isPlacing = false;
        
        // Update the graph to make the cell walkable
        if (item != null && item.itemType != Item.ItemType.Plank)
        {
            UpdateGraphAtPosition(tilemapGrid.GetCellCenterWorld(gridCell), false);    
        }
        
        inventoryManager.RemoveItem(item);
    }
    
    public void UpdateGraphAtPosition(Vector3 position, bool walkable)
    {
        // Define the bounds of the area to update
        Bounds bounds = new Bounds(position, new Vector3(1, 2, 1)); // Adjust size as needed

        // Create a GraphUpdateObject (GUO) for updating the graph
        GraphUpdateObject guo = new GraphUpdateObject(bounds);

        // Set the GUO to modify the walkability
        guo.modifyWalkability = walkable;
        guo.setWalkability = walkable;

        // Apply the GUO
        AstarPath.active.UpdateGraphs(guo);

        AstarPath.active.FlushGraphUpdates();

        Debug.Log("Graph updated at position: " + position);
    }
    
    private void CancelItemPlacement()
    {
        if (placementObjectInstance) Destroy(placementObjectInstance);
        placementObjectInstance = null;
        isPlacing = false;
        Debug.Log("Plank placement canceled.");
    }
 
    public Vector3 GetMouseWorldPosition()
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        return plane.Raycast(ray, out float distance) ? ray.GetPoint(distance) : Vector3.zero;
    }
    
}
