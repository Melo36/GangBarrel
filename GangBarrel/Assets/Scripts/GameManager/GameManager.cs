using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace GameManager
{
    /// <summary>
    /// This class saves important game data.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] public SerializedDictionary<GameObject, GameObject> keyChestPairs;

        private void Start()
        {
            AstarPath active = AstarPath.active;
            if (active == null)
            {
                Debug.LogError("No AstarPath component found!");
                return;
            }
            Debug.Log("Astar.active.Scan()");
            AstarPath.active.Scan();
        }

        public void AddKeyChestPair(GameObject key, GameObject chest)
        {
            keyChestPairs ??= new SerializedDictionary<GameObject, GameObject>();
            
            keyChestPairs.Add(chest, key);
            Debug.Log($"keyChestPairs.Count (from GameManager) = {keyChestPairs.Count}");
            Debug.Log($"Successfully added the key: {key.name}");
        }
    
        public GameObject GetKeyForChest(GameObject chest)
        {
            if (keyChestPairs == null)
            {
                Debug.LogError("Key-chest pairs dictionary is null!");
                return null;
            }

            Debug.Log($"keyChestPairs.Count = {keyChestPairs.Count}");
            
            foreach (var pair in keyChestPairs)
            {
                Debug.Log($"pair.Value = {pair.Value}");
                if (ReferenceEquals(pair.Value, chest)) // Check if the chest matches
                {
                    Debug.Log("Return the key.");
                    return pair.Key; // Return the corresponding key
                }
            }

            Debug.LogWarning("No key found for the given chest.");
            return null;
        }
    
    }
}
