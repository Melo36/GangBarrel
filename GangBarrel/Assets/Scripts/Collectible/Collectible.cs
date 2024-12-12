using System;
using Pathfinding;
using UnityEngine;
using UniRx;
using TMPro;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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

    private void Start()
    {
        promptManager = FindObjectOfType<PromptManager>();
        itemUsage = FindObjectOfType<ItemUsage>();
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
        // Invoke the OnCollected event with the item
        OnCollected.Execute(item);
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
