using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

public enum TileType
{
    SmallRock,
    BigRock,
    CaveEntrance,
    CaveInside,
    Fog,
    Water,
    Plank,
    BarrelBeer,
    BarrelNuclear,
    BarrelFog,
    Rockfall, // rocks that fell down inside a cave - these are not passable
}

public enum NeighborDirection
{
    North,
    NorthEast,
    East,
    SouthEast,
    South,
    SouthWest,
    West,
    NorthWest
}

public enum TileDisplayMode
{
    Sprite,
    GameObject
}

[CreateAssetMenu(fileName = "New Custom Tile", menuName = "Tiles/CustomTile")]
public class CustomTile : TileBase
{
    [SerializeField] private TileType tileType;
    [SerializeField] public TileDisplayMode displayMode;
    [SerializeField] private Sprite sprite;
    [SerializeField] public GameObject prefab;
    [SerializeField] private Vector3 objectOffset = Vector3.zero;
    [SerializeField] private Vector3 objectRotation = Vector3.zero;
    [SerializeField] private Vector3 objectScale = Vector3.one;
    
    private CustomTile[] neighbors = new CustomTile[8];
    
    private static readonly Dictionary<NeighborDirection, Vector3Int> NeighborOffsets = new Dictionary<NeighborDirection, Vector3Int>
    {
        { NeighborDirection.North, new Vector3Int(0, 1, 0) },
        { NeighborDirection.NorthEast, new Vector3Int(1, 1, 0) },
        { NeighborDirection.East, new Vector3Int(1, 0, 0) },
        { NeighborDirection.SouthEast, new Vector3Int(1, -1, 0) },
        { NeighborDirection.South, new Vector3Int(0, -1, 0) },
        { NeighborDirection.SouthWest, new Vector3Int(-1, -1, 0) },
        { NeighborDirection.West, new Vector3Int(-1, 0, 0) },
        { NeighborDirection.NorthWest, new Vector3Int(-1, 1, 0) }
    };
    
    public TileType TileType
    {
        get => tileType;
        set => tileType = value;
    }

    private GameObject spawnedObject;

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        foreach (NeighborDirection direction in Enum.GetValues(typeof(NeighborDirection)))
        {
            Vector3Int neighborPos = position + NeighborOffsets[direction]; // Use dictionary to get the offset
            if (tilemap.GetTile(neighborPos) is CustomTile)
            {
                tilemap.RefreshTile(neighborPos);
            }
        }
    }

    public void DebugNeighbors(Vector3Int position, Tilemap tilemap)
    {
        UpdateNeighbors(position, tilemap); // Ensure neighbors are updated

       // Debug.Log($"Tile at {position} (Type: {TileType}) has the following neighbors:");

        foreach (NeighborDirection direction in Enum.GetValues(typeof(NeighborDirection)))
        {
            int index = (int)direction;
            if (neighbors[index] != null)
            {
                //Debug.Log($"- {direction}: {neighbors[index].TileType}");
            }
            else
            {
                //Debug.Log($"- {direction}: None");
            }
        }
    }
    
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
        
        // Update neighbors when getting tile data
        UpdateNeighbors(position, tilemap);

        
        if (displayMode == TileDisplayMode.Sprite)
        {
            tileData.sprite = sprite;
            tileData.color = Color.white;
            tileData.transform = Matrix4x4.identity;
            tileData.gameObject = null;
        }
        else
        {
            tileData.sprite = null;
            tileData.color = Color.white;
            tileData.transform = Matrix4x4.identity;
            tileData.gameObject = prefab;
        }

        tileData.flags = TileFlags.LockAll;
        tileData.colliderType = Tile.ColliderType.Grid;
    }

    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        if (prefab != null && tilemap != null)
        {
            // Calculate the world position of the tile
            Vector3 worldPosition = tilemap.GetComponent<Tilemap>().GetCellCenterWorld(position);

            // Instantiate the prefab if it hasn't been instantiated yet
            if (spawnedObject == null)
            {
                spawnedObject = Object.Instantiate(prefab, worldPosition, Quaternion.identity);
                spawnedObject.name = $"{prefab.name}_at_{position}";
            }
        }

        return base.StartUp(position, tilemap, go);
    }
    
    public GameObject GetSpawnedObject()
    {
        return spawnedObject; // Provide access to the spawned prefab
    }

    private void UpdateNeighbors(Vector3Int position, ITilemap tilemap)
    {
        foreach(NeighborDirection direction in Enum.GetValues(typeof(NeighborDirection)))
        {
            Vector3Int neighborPos = position + NeighborOffsets[direction];
            neighbors[(int)direction] = tilemap.GetTile(neighborPos) as CustomTile;
        }
    }

    public bool HasNeighborOfType(NeighborDirection direction, TileType type)
    {
        int index = (int)direction;

        if (index < 0 || index >= neighbors.Length)
            return false;

        return neighbors[index]?.TileType == type;
    }

    public bool HasAnyNeighborOfType(TileType type)
    {
        return GetNeighborCountOfType(type) > 0;
    }   
    
    public int GetNeighborCountOfType(TileType type)
    {
        int count = 0;
        foreach (var neighbor in neighbors)
        {
            if (neighbor != null && neighbor.TileType == type)
                count++;
        }
        return count;
    }
}
