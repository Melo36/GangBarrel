using System;
using System.Linq;
using Inventory;
using Pathfinding;
using UnityEngine;
using UniRx;
using UnityEngine.Tilemaps;

public class Collectible : MonoBehaviour
{
    public static ReactiveCommand<Item> OnCollected = new ReactiveCommand<Item>();
    public Item.ItemType itemType;
    public Item item;

    //[SerializeField] private Canvas promptCanvas; // Assign a Canvas in the Inspector
    //[SerializeField] private TextMeshProUGUI promptText; // Assign a TextMeshProUGUI component in the Inspector
    [SerializeField] private string buttonToPress = "E"; // Button to display in the prompt

    private bool playerInRange = false;

    private PromptManager promptManager;
    private ItemUsage itemUsage;

    public InventoryManager playerInventory;
    public GameManager.GameManager gameManager;
    private FuseManager fuseManager;
    
    private void OnEnable()
    {
        promptManager = FindObjectOfType<PromptManager>();
        itemUsage = FindObjectOfType<ItemUsage>();
        playerInventory = FindObjectOfType<InventoryManager>();
        fuseManager = FindObjectOfType<FuseManager>();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Collect();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered collectible range");
            playerInRange = true;
            
            // if the player has a fuse, then we call the promptManager from the FusePlacement class. 
            var fuseItem = playerInventory.items.FirstOrDefault(fuse => item.itemType == Item.ItemType.Fuse);
            if (fuseItem != null && itemType == Item.ItemType.Barrel)
                return;
            
            // Display the prompt
            promptManager.ShowInteractionPrompt($"Press E to pick up {item.itemType}.");
            promptManager.SetDescriptionTextForItem(item);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited collectible range");
            playerInRange = false;

            promptManager.StopInteractionPrompt();
        }
    }

    private void Collect()
    {
        // No collection when in WaitEndPoint state.
        if (fuseManager.currentState == FuseManager.CurrentState.WaitEndPoint)
            return;
        
        // Invoke the OnCollected event with the item
        OnCollected.Execute(item);
        if (item.itemType == Item.ItemType.Key)
        {
            playerInventory.keys.Add(gameObject);
            gameObject.SetActive(false);
            // Hide the prompt just in case
            promptManager.StopInteractionPrompt();
            return;
        }
        
        // Destroying the gameobject and updating the graph.
        Destroy(gameObject);

        bool walkable = true;

        Vector3Int gridCell = itemUsage.tilemapGrid.WorldToCell(transform.position);
        TileBase baseTile = itemUsage.tilemapGrid.GetTile(gridCell);

        if (baseTile is CustomTile customTile && customTile.TileType == TileType.Water)
        {
            walkable = false;
        }
        
        // As the game object is no longer there, we can set this area to walkable again.
        itemUsage.UpdateGraphAtPosition(transform.position, walkable);
        
        // Hide the prompt just in case
        promptManager.StopInteractionPrompt();
    }
    
}
