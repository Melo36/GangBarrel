using UnityEngine;
using UniRx;
using System;
using TMPro;
using UnityEngine.UI;

public class Collectible : MonoBehaviour
{
    public static ReactiveCommand<Item> OnCollected = new ReactiveCommand<Item>();
    public Item.ItemType itemType;
    
    public Item item;
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger");
        if (other.CompareTag("Player"))
        {
            // Invoke the OnCollected event with the itemType
            OnCollected.Execute(item);
            Destroy(gameObject);
        }
    }
}