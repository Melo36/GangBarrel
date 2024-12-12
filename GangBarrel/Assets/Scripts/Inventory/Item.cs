using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public enum ItemType
    {
        Barrel,
        Bullet,
        Plank
    }
    public Sprite sprite;
    public ItemType itemType;
    public string name; // for displaying stuff
    public string description;
    
    // UI-object to be instantiated
    public GameObject uiItemPrefab;
    
    // 3D GameObject prefab (which can be placed)
    public GameObject itemPrefab;

}

