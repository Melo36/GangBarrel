using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class GridInformation
{
    public List<GridObjectInformation> gridObjects; // Only gridObjects now
    public TilemapInformation tilemapData;
}

[System.Serializable]
public class GridObjectInformation
{
    public string objectName;
    public Vector3 position;
    public string chestContent;
}

[System.Serializable]
public class TilemapInformation
{
    public List<TileInformation> tiles;
}

[System.Serializable]
public class TileInformation
{
    public Vector3Int position;
    public string tileName; // Name or ID of the tile
}