using System;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using UnityEngine;

public class FuseEndpoint : MonoBehaviour
{
    public PromptManager promptManager;
    private Fuse attachedFuse;
    private bool isPlayerInRange;
    private bool isLit = false;
    private Fuse connectedToFuse; // The fuse this endpoint is connected to (if any)
    private bool isConnectedToAnotherFuse => connectedToFuse != null;

    private FuseManager fuseManager;
    
    // Track connected fuses and their endpoints
    private List<(Fuse fuse, FuseEndpoint endpoint)> connections = new List<(Fuse fuse, FuseEndpoint endpoint)>();
    
    private void OnEnable()
    {
        promptManager = FindObjectOfType<PromptManager>();
        fuseManager = FindObjectOfType<FuseManager>();
    }

    public void Initialize(Fuse fuse)
    {
        attachedFuse = fuse;
    }

    public void AddConnection(Fuse fuse, FuseEndpoint endpoint)
    {
        connections.Add((fuse, endpoint));
    }
    
    public void SetConnectedFuse(Fuse fuse)
    {
        connectedToFuse = fuse;
        if (fuse != null)
        {
            fuse.SetConnectedEndpoint(this);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (!isLit)
            {
                // Check if player has a fuse
                var fuseItem = other.GetComponent<InventoryManager>()?.items.FirstOrDefault(item => item.itemType == Item.ItemType.Fuse);
                if (fuseItem != null)
                {
                    promptManager.ShowInteractionPrompt("Press R to light fuse or E to attach new fuse");
                }
                else
                {
                    // Only show light prompt if this isn't connected to another fuse
                    if (!isConnectedToAnotherFuse)
                    {
                        promptManager.ShowInteractionPrompt("Press R to light fuse");
                    }
                }
            }
        }

        if (other.CompareTag("ExplosionTrigger"))
        {
            attachedFuse.LightFuse();
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            promptManager.StopInteractionPrompt();
        }
    }

    private void Update()
    {
        if (!isPlayerInRange) return;

        if (Input.GetKeyDown(KeyCode.R) && !isLit && !isConnectedToAnotherFuse)
        {
            isLit = true;
            attachedFuse.LightFuse();
        }
        else if (Input.GetKeyDown(KeyCode.E) && !isLit)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            var inventory = player.GetComponent<InventoryManager>();
            var fuseItem = inventory?.items.FirstOrDefault(item => item.itemType == Item.ItemType.Fuse);
            
            if (fuseItem != null)
            {
                var fuseManager = FindObjectOfType<FuseManager>();
                if (fuseManager != null)
                {
                    fuseManager.StartFuseFromEndpoint(this);
                    inventory.RemoveItem(fuseItem);
                }
            }
        }
    }

    public void TriggerConnectedFuse()
    {
        if (attachedFuse != null && !isLit)
        {
            isLit = true;
            attachedFuse.LightFuse();
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}