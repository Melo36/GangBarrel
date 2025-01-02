using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.IO;
using TMPro;

public class GridSaver : MonoBehaviour
{
    public GameObject gridParent; // Parent object that contains all grid objects
    public Tilemap tilemap; // Reference to the Tilemap component
    public TextMeshProUGUI levelName;

    public void SaveGridAndTilemapData()
    {
        GridInformation gridInformation = new GridInformation();
        gridInformation.gridObjects = new List<GridObjectInformation>();
        gridInformation.tilemapData = new TilemapInformation();
        gridInformation.tilemapData.tiles = new List<TileInformation>();

        // Save grid objects (same as before)
        foreach (Transform child in gridParent.transform)
        {
            GridObjectInformation gridObjectInformation = new GridObjectInformation
            {
                objectName = child.name,
                position = child.position
            };
            gridInformation.gridObjects.Add(gridObjectInformation);
        }

        // Save tilemap tiles
        BoundsInt bounds = tilemap.cellBounds;
        foreach (var position in bounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(position);
            if (tile != null)
            {
                TileInformation tileInformation = new TileInformation
                {
                    position = position,
                    tileName = tile.name // or use tile.GetInstanceID() for more unique identification
                };
                gridInformation.tilemapData.tiles.Add(tileInformation);
            }
        }

        // Serialize to JSON
        string filePath = "Assets/CustomLevels/" + levelName.text + ".json";
        string json = JsonUtility.ToJson(gridInformation, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Grid and Tilemap data saved to: " + filePath);
    }
}