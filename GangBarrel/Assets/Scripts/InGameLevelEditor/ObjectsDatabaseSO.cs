using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using Tile = UnityEngine.Tilemaps.Tile;

[CreateAssetMenu]
public class ObjectsDatabaseSO : ScriptableObject
{
    [Tooltip("Set this manually to the number of tiles in the level editor")]
    public int amountOfTiles;
    public List<ObjectData> objectsData;
}

[Serializable]
public class ObjectData
{
    [field: SerializeField]
    public string Name { get; private set; }
    
    [field: SerializeField]
    public int ID { get; private set; }

    [field: SerializeField] 
    public Vector2Int Size { get; private set; } = Vector2Int.one;
    
    [field: SerializeField]
    public GameObject Prefab { get; private set; }
    
    [field: SerializeField]
    public TileBase Tile { get; private set; }
}