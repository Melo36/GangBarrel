using System;
using UnityEngine;
using System.Linq;
using Inventory;

public class FusePlacement : MonoBehaviour
{
    public InventoryManager playerInventory;
    public PromptManager promptManager;
    public FuseManager fuseManager;
    private bool isPlayerInRange;

    private void OnEnable()
    {
        playerInventory = FindObjectOfType<InventoryManager>();
        promptManager = FindObjectOfType<PromptManager>();
        fuseManager = FindObjectOfType<FuseManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("FusePlacement");
            var fuseItem = playerInventory.items.FirstOrDefault(item => item.itemType == Item.ItemType.Fuse);
            if (fuseItem != null)
            {
                Debug.Log("Press F to place fuse, E to pick it up.");
                isPlayerInRange = true;
                promptManager.ShowInteractionPrompt("Press F to place fuse, E to pick up");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // when leaving the place during placing mode, then we add the item back and reset the currentState
            if (fuseManager.currentState is FuseManager.CurrentState.WaitStartPoint or FuseManager.CurrentState.WaitEndPoint)
            {
                playerInventory.AddItem(fuseManager.fuseItem);
                fuseManager.currentState = FuseManager.CurrentState.Init;
            }
            isPlayerInRange = false;
            promptManager.StopInteractionPrompt();
        }
    }

    private void Update()
    {
        if (!isPlayerInRange) return;
        if (fuseManager.currentState == FuseManager.CurrentState.WaitEndPoint) return; // Disable during placement

        if (Input.GetKeyDown(KeyCode.F) && fuseManager.currentState == FuseManager.CurrentState.Init)
        {
            var fuseItem = playerInventory.items.FirstOrDefault(item => item.itemType == Item.ItemType.Fuse);
            if (fuseItem != null)
            {
                fuseManager.SetStartBarrel(GetComponent<BarrelExplosionController>());
                fuseManager.currentState = FuseManager.CurrentState.WaitEndPoint;
                playerInventory.RemoveItem(fuseItem);
            }
        }
    }
}