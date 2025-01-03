using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using UnityEditor;

public class GridLoader : MonoBehaviour
{
    public GameObject grid; // Prefab of the grid object to instantiate
    public Tilemap tilemap; // Reference to the Tilemap component
    private string filePath = "Assets/CustomLevels/test level\u200b.json";
    public GameObject barrel;
        
    private void Start()
    {
        LoadGridAndTilemapData();
    }

    public void LoadGridAndTilemapData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            GridInformation gridInformation = JsonUtility.FromJson<GridInformation>(json);

            // Load grid objects
            foreach (var gridObjectInformation in gridInformation.gridObjects)
            {
                createObject(gridObjectInformation.objectName, gridObjectInformation.position);
            }

            string tilePath = "Assets/GROUND TILESETS RULE TILES/Ground Tiles V3/Rule Tiles/";
            // Load tilemap tiles
            foreach (var tileInformation in gridInformation.tilemapData.tiles)
            {
                Debug.Log(tilePath + tileInformation.tileName);
                TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(tilePath + tileInformation.tileName + ".asset"); // Assumes tiles are stored as assets in Resources
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

    private GameObject createObject(string objectName, Vector3 objectPosition)
    {
        int cutParentheses = objectName.IndexOf("(");
        if (cutParentheses != -1)
        {
            objectName = objectName.Substring(0, cutParentheses);
        }
        
        Debug.Log(objectName);

       
        if (objectName == "LBarrel")
        {
            return Instantiate(barrel, objectPosition, Quaternion.identity);
        }
        return null;
    }
}