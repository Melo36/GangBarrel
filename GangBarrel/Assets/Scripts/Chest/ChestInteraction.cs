using System;
using System.Collections;
using System.Collections.Generic;
using Chest;
using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChestInteraction : MonoBehaviour
{
    public InventoryManager playerInventory;
    public GameObject chestOpen;
    public GameManager.GameManager gameManager;
    public PromptManager promptManager;

    public bool chestOpened;
    public ChestContent chestContent;
    public GameObject uiItemPrefab;
    
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager.GameManager>();
        playerInventory = FindObjectOfType<InventoryManager>();
        promptManager = FindObjectOfType<PromptManager>();
        chestContent = GetComponent<ChestContent>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // If chest has been opened, we show the panel
            if (chestOpened)
            {
                chestContent.ShowChestContent();
                return;
            }
            
            var neededKey = gameManager.GetKeyForChest(gameObject);
            if (playerInventory.keys.Contains(neededKey))
            {
                Debug.Log("Player has the needed key. Open!");
                GetComponent<MeshFilter>().sharedMesh = chestOpen.GetComponent<MeshFilter>().sharedMesh;
                chestContent.ShowChestContent();
                AddChestContentItems();
                chestOpened = true;
            }
            else
            {
                promptManager.descriptionText.text = "This is a item box, which holds some useful items!";
                promptManager.ShowInteractionPrompt("You need to find the key!");
            }
        }
    }

    private void AddChestContentItems()
    {
        foreach (var item in chestContent.items)
        {
            // Instantiate the prefab and set it as a child of the inventory content parent
            GameObject newItem = Instantiate(item.uiItemPrefab, promptManager.chestContentParentObject.transform);
            
            var btn = newItem.GetComponent<Button>();
            btn.onClick.AddListener(() => AddItemToInventory(item));
        
            // Get the components directly from the children
            Image image = newItem.transform.GetChild(0).GetComponent<Image>();
            TextMeshProUGUI text = newItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                    
            chestContent.itemsUI.Add(newItem);

            image.sprite = item.sprite;
            text.text = item.name;
        }
    }
    
    private void AddItemToInventory(Item item)
    {
        Debug.Log("Add Item To Inventory!!");
        // remove item from chest, add it to the inventory of the player.
        chestContent.RemoveItem(item);
        playerInventory.OnCollectibleCollected(item);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (chestOpened)
            {
                chestContent.HideChestContent();
            }
            promptManager.StopInteractionPrompt();
        }
    }
}
