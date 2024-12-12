using System;
using Pathfinding;
using UnityEngine;
using UniRx;
using TMPro;
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

    private void Start()
    {
        promptManager = FindObjectOfType<PromptManager>();
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

        
        
        // As the game object is no longer there, we can set this area to walkable again.
        UpdateGraphAtPosition(transform.position, true);
        
        // Hide the prompt just in case
        promptManager.StopInteractionPrompt();
    }
    
    // This is code DUPLICATE from ItemUsage.cs, find a better solution, where duplicate code is not necessary.
    private void UpdateGraphAtPosition(Vector3 position, bool walkable)
    {
        // Define the bounds of the area to update
        Bounds bounds = new Bounds(position, new Vector3(1, 2, 1)); // Adjust size as needed

        // Create a GraphUpdateObject (GUO) for updating the graph
        GraphUpdateObject guo = new GraphUpdateObject(bounds);

        // Set the GUO to modify the walkability
        guo.modifyWalkability = walkable;
        guo.setWalkability = walkable;

        // Optionally, you can set the tag or penalty if needed
        // guo.tag = 1; // For example, set a tag for the plank area
        // guo.penalty = 0; // Adjust the penalty if required

        // Apply the GUO
        AstarPath.active.UpdateGraphs(guo);

        // If you want to force the update immediately (synchronously), uncomment the following line:
        // AstarPath.active.FlushGraphUpdates();

        Debug.Log("Graph updated at position: " + position);
    }
}
