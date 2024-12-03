using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        public List<Item> items = new List<Item>();
        public GameObject inventoryContentParent;

        // objects displayed in the ui
        public List<GameObject> itemsUI;

        public PlayerController playerController;

        /// <summary>
        /// Use the item.
        /// </summary>
        /// <returns>true if success, false if it did not work.</returns>
        public bool UseItem(Item item)
        {
            switch (item.itemType)
            {
                case Item.ItemType.Barrel:
                    Debug.Log("Use Barrel");
                    break;
                case Item.ItemType.Plank:
                    Debug.Log("Use Plank");
                    break;
                case Item.ItemType.Bullet:
                    Debug.Log("Load the bullet!");
                    break;
            }
            return true;
        }

        void OnEnable()
        {
            // Subscribe to the OnCollected ReactiveCommand
            Collectible.OnCollected
                .Subscribe(item =>
                {
                    Debug.Log($"Collected Item: {item.itemType}");
                    OnCollectibleCollected(item);
                })
                .AddTo(this); // Ensures disposal when the object is destroyed
        }

        private void OnCollectibleCollected(Item item)
        {
            // Add the item to the inventory list
            items.Add(item);

            // Instantiate the item prefab and update its UI components
            var newItemUI = CreateInventoryUI(item);
            itemsUI.Add(newItemUI);

            // Sort the inventory and UI lists
            SortInventory();
        }

        private GameObject CreateInventoryUI(Item item)
        {
            // Instantiate the prefab and set it as a child of the inventory content parent
            Debug.Log("Inventar ist " + inventoryContentParent);
            GameObject newItem = Instantiate(item.itemPrefab, inventoryContentParent.transform);

            if (item.itemType != Item.ItemType.Bullet)
            {
                var btn = newItem.GetComponent<Button>();
                btn.onClick.AddListener(() => playerController.StartPlankPlacement());
            }
        
            // Get the components directly from the children
            Image image = newItem.transform.GetChild(0).GetComponent<Image>();
            TextMeshProUGUI text = newItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            image.sprite = item.sprite;
            text.text = item.name;

            return newItem;
        }

        private void SortInventory()
        {
            // Define the order for sorting
            var order = new Dictionary<Item.ItemType, int>
            {
                { Item.ItemType.Bullet, 0 },
                { Item.ItemType.Plank, 1 },
                { Item.ItemType.Barrel, 2 }
            };

            // Create a sorted list of items with their corresponding UI
            var sortedItems = items
                .Select((item, index) => new { Item = item, UI = itemsUI[index] })
                .OrderBy(pair => order[pair.Item.itemType])
                .ToList();

            // Update the items and itemsUI lists with the sorted order
            items = sortedItems.Select(pair => pair.Item).ToList();
            itemsUI = sortedItems.Select(pair => pair.UI).ToList();

            // Update the UI hierarchy
            for (int i = 0; i < itemsUI.Count; i++)
            {
                itemsUI[i].transform.SetSiblingIndex(i);
            }
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
