using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Chest
{
    public class ChestContent : MonoBehaviour
    {
        public List<Item> items;
        
        // objects displayed in the ui
        public List<GameObject> itemsUI;
        
        private PromptManager promptManager;
        
        private void Awake()
        {
            promptManager = FindObjectOfType<PromptManager>();
            promptManager.xButton.onClick.AddListener(HideChestContent);
        }

        public void ShowChestContent()
        {
            promptManager.chestContentPanelObject.SetActive(true);
        }

        public void HideChestContent()
        {
            promptManager.chestContentPanelObject.SetActive(false);
        }
        
        public void RemoveItem(Item item)
        {
            // Find the index of the item in the inventory
            int index = items.IndexOf(item);

            if (index >= 0)
            {
                // Remove the item from the inventory list
                items.RemoveAt(index);

                // Destroy the corresponding UI object
                Destroy(itemsUI[index]);

                // Remove the UI object from the itemsUI list
                itemsUI.RemoveAt(index);

                // Debugging confirmation
                Debug.Log($"Removed {item.name} from inventory.");
            }
            else
            {
                Debug.LogWarning("Item not found in inventory. Cannot remove.");
            }
        }
        
    }
}
