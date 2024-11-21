using UnityEngine;
using UnityEngine.Tilemaps;

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
    [SerializeField] public Sprite sprite;
    [SerializeField] public GameObject prefab;
    [SerializeField] private Vector3 objectOffset = Vector3.zero;
    [SerializeField] private Vector3 objectRotation = Vector3.zero;
    [SerializeField] private Vector3 objectScale = Vector3.one;
    
    private CustomTile[] neighbors = new CustomTile[8];

    public TileType TileType
    {
        get => tileType;
        set => tileType = value;
    }

    private GameObject spawnedObject;
    
    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        UpdateNeighbors(position, tilemap);
        base.RefreshTile(position, tilemap);
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
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
        if (displayMode == TileDisplayMode.GameObject && prefab != null && go != null)
        {
            go.transform.position += objectOffset;
            go.transform.rotation = Quaternion.Euler(objectRotation);
            go.transform.localScale = objectScale;
        }
        return base.StartUp(position, tilemap, go);
    }

    private void UpdateNeighbors(Vector3Int position, ITilemap tilemap)
    {
        Vector3Int[] neighborPositions = new Vector3Int[]
        {
            new Vector3Int(position.x, position.y + 1, position.z),    // N
            new Vector3Int(position.x + 1, position.y + 1, position.z),// NE
            new Vector3Int(position.x + 1, position.y, position.z),    // E
            new Vector3Int(position.x + 1, position.y - 1, position.z),// SE
            new Vector3Int(position.x, position.y - 1, position.z),    // S
            new Vector3Int(position.x - 1, position.y - 1, position.z),// SW
            new Vector3Int(position.x - 1, position.y, position.z),    // W
            new Vector3Int(position.x - 1, position.y + 1, position.z) // NW
        };

        for (int i = 0; i < 8; i++)
        {
            neighbors[i] = tilemap.GetTile(neighborPositions[i]) as CustomTile;
        }
    }

    public bool HasNeighborOfType(int direction, TileType type)
    {
        return neighbors[direction] != null && neighbors[direction].TileType == type;
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
