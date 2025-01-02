using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class GridLoader : MonoBehaviour
{
    public GameObject gridPrefab; // Prefab of the grid object to instantiate
    public Tilemap tilemap; // Reference to the Tilemap component
    private string filePath = "Assets/Resources/gridData.json";

    public void LoadGridAndTilemapData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            GridInformation gridInformation = JsonUtility.FromJson<GridInformation>(json);

            // Load grid objects
            foreach (var gridObjectInformation in gridInformation.gridObjects)
            {
                GameObject gridObject = Instantiate(gridPrefab, gridObjectInformation.position, Quaternion.identity);
                gridObject.name = gridObjectInformation.objectName;
                Debug.Log($"Loaded {gridObjectInformation.objectName} at {gridObjectInformation.position}");
            }

            // Load tilemap tiles
            foreach (var tileInformation in gridInformation.tilemapData.tiles)
            {
                TileBase tile = Resources.Load<TileBase>(tileInformation.tileName); // Assumes tiles are stored as assets in Resources
                if (tile != null)
                {
                    tilemap.SetTile(tileInformation.position, tile);
                    Debug.Log($"Loaded tile {tileInformation.tileName} at {tileInformation.position}");
                }
                else
                {
                    Debug.LogError($"Tile {tileInformation.tileName} not found.");
                }
            }
        }
        else
        {
            Debug.LogError("Grid data file not found.");
        }
    }
}