using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using Pathfinding;
using UniRx;
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
    
    public GameObject placementObjectInstance;
    
    private bool isPlacing;

    private ReactiveProperty<bool> canPlace = new ReactiveProperty<bool>(true);
    
    private Item placingItem;

    private GridGraph gridGraph;
    
    private void Awake()
    {
        gridGraph = AstarPath.active.data.gridGraph;
        mainCamera = Camera.main;
        /*
        canPlace.Subscribe(b =>
        {
            placementObjectInstance.GetComponent<Renderer>().material.color = b ? Color.white : Color.red;
        });
        */
    }

    private void Update()
    {
        if (isPlacing)
        {
            UpdatePlacement();
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

        canPlace.Value = gridGraph.GetNearest(snappedPosition).node.Walkable;
        if(itemToPlace.itemType == Item.ItemType.Plank) 
            RotatePlankDependingOnNeighbours(snappedPosition);

        var rend = placementObjectInstance.GetComponent<Renderer>();
        if (rend != null)
        {
            placementObjectInstance.GetComponent<Renderer>().material.color = canPlace.Value ? Color.white : Color.red;   
        }
        
        if (Input.GetMouseButtonDown(0)) PlaceItem(gridCell);
    }

    private void RotatePlankDependingOnNeighbours(Vector3 snappedPosition)
    {
        // Positions
        var positionEast = new Vector3(snappedPosition.x + 1, snappedPosition.y, snappedPosition.z);
        var positionWest = new Vector3(snappedPosition.x - 1, snappedPosition.y, snappedPosition.z);
        
        // Walk ability
        var canWalkEast = gridGraph.GetNearest(positionEast).node.Walkable;
        var canWalkWest = gridGraph.GetNearest(positionWest).node.Walkable;
        
        if(canWalkEast && canWalkWest)
            placementObjectInstance.transform.rotation = Quaternion.Euler(0,0,0);
        else 
            placementObjectInstance.transform.rotation = Quaternion.Euler(0,90,0);
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

        itemToPlace = item;
        
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
        // No placement on areas, where we cannot place it
        if (!canPlace.Value && itemToPlace.itemType != Item.ItemType.Plank) 
            return;
        // Place the item in the game world
        Destroy(placementObjectInstance.GetComponent<Blinking>());
        placementObjectInstance.transform.position = tilemapGrid.GetCellCenterWorld(gridCell);
        
        var item = inventoryManager.items.FirstOrDefault(i => i.itemType == placingItem.itemType);
        
        if (item != null && item.itemType == Item.ItemType.Plank)
        {
            var cellCenterWorld = tilemapGrid.GetCellCenterWorld(gridCell);
            UpdateGraphAtPosition(cellCenterWorld, true);

            // We need to set the water objects layer to "Default", as otherwise rescanning the map leads to the nodes being unwalkable again!
            var water = FindClosestObjectWithTag("Water", placementObjectInstance.transform);
            if (water != null)
            {
                water.layer = LayerMask.NameToLayer("Default");
            }
            
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
    
    private GameObject FindClosestObjectWithTag(string targetTag, Transform fromPosition)
    {
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(targetTag);
    
        if (taggedObjects.Length == 0)
            return null;
        
        GameObject closest = null;
        float closestDistance = Mathf.Infinity;
    
        foreach (GameObject obj in taggedObjects)
        {
            float distance = Vector3.Distance(fromPosition.position, obj.transform.position);
            if (distance < closestDistance)
            {
                closest = obj;
                closestDistance = distance;
            }
        }
    
        return closest;
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
    
    public void CancelItemPlacement()
    {
        if (placementObjectInstance) Destroy(placementObjectInstance);
        placementObjectInstance = null;
        isPlacing = false;
        
        // When the item placement is cancelled, we need to add 
        
        Debug.Log("Plank placement canceled.");
    }
 
    public Vector3 GetMouseWorldPosition()
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        return plane.Raycast(ray, out float distance) ? ray.GetPoint(distance) : Vector3.zero;
    }
    
}
