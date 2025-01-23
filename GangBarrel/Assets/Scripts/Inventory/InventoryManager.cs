using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
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
        [FormerlySerializedAs("itemPlacement")] public ItemUsage itemUsage;

        public List<GameObject> keys;
        
        private void Awake()
        {
            SyncCheckInventoryUI();
        }

        /// <summary>
        /// Checks whether the ui has the same amount of objects, as the items list, if no, delete all add them anew.
        /// </summary>
        private void SyncCheckInventoryUI()
        {
            if (items.Count == itemsUI.Count) return;
            
            // Delete all items from ui panel:
            if (itemsUI.Count > 0)
            {
                itemsUI.ForEach(Destroy);
                itemsUI.RemoveRange(0, itemsUI.Count - 1);
            }

            // Add them back 
            foreach (var item in items)
            {
                var newItemUI = CreateInventoryUI(item);
                itemsUI.Add(newItemUI);
            }

            SortInventory();
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

        public void OnCollectibleCollected(Item item)
        {
            AddItem(item);
        }

        public void AddItem(Item item)
        {
            // Keys are not added to the inventory
            if (item.itemType == Item.ItemType.Key)
                return;
            
            // Add the item to the inventory list
            items.Add(item);
            
            // Instantiate the item prefab and update its UI components
            var newItemUI = CreateInventoryUI(item);
            itemsUI.Add(newItemUI);

            // Sort the inventory and UI lists
            SortInventory();
        }

        internal GameObject CreateInventoryUI(Item item)
        {
            if(item == null)
                Debug.LogError("Item is null");
            if(item.uiItemPrefab == null)
                Debug.LogError("item.uiItemPrefab is null");
            if(inventoryContentParent == null)
                Debug.LogError("inventoryContentParent is null");
            
            // Instantiate the prefab and set it as a child of the inventory content parent
            GameObject newItem = Instantiate(item.uiItemPrefab, inventoryContentParent.transform);

            if (item.itemType != Item.ItemType.Bullet && item.itemType != Item.ItemType.Fuse)
            {
                var btn = newItem.GetComponent<Button>();
                btn.onClick.AddListener(() => itemUsage.StartItemUsage(item));
            }

            if (item.itemType == Item.ItemType.Fuse)
            {
                var btn = newItem.GetComponent<Button>();
                //btn.onClick.AddListener(() => playerController.fuseManager.currentState = FuseManager.CurrentState.WaitStartPoint);
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
                { Item.ItemType.Barrel, 2 },
                { Item.ItemType.Fuse, 3 },
                {Item.ItemType.Key, 4}
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
            }
            else
            {
                Debug.LogWarning("Item not found in inventory. Cannot remove.");
            }
        }

        public void RemoveLastItem()
        {
            if (items.Count <= 0)
                return;
            
            var lastIndex = items.Count - 1;
            
            items.RemoveAt(lastIndex);
            Destroy(itemsUI[lastIndex]);
            itemsUI.RemoveAt(lastIndex);
            
        }

    }
}
