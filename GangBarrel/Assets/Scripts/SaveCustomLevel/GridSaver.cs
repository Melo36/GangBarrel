using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.IO;
using TMPro;

public class GridSaver : MonoBehaviour
{
    public GameObject gridParent;
    public Tilemap tilemap;
    public TextMeshProUGUI levelName;
    private string chestContentPath;
    private int currentChest = 0;

    private void Start()
    {
        chestContentPath = Path.Combine(Application.persistentDataPath, "chest_content.txt");
    }

    public void SaveGridAndTilemapData()
    {
        GridInformation gridInformation = new GridInformation();
        gridInformation.gridObjects = new List<GridObjectInformation>();
        gridInformation.tilemapData = new TilemapInformation();
        gridInformation.tilemapData.tiles = new List<TileInformation>();

        foreach (Transform child in gridParent.transform)
        {
            GridObjectInformation gridObjectInformation = new GridObjectInformation
            {
                objectName = child.name,
                position = child.position
            };
            gridInformation.gridObjects.Add(gridObjectInformation);

            if (gridObjectInformation.objectName == "LChest(Clone)")
            {
                if (File.Exists(chestContentPath))
                {
                    string[] lines = File.ReadAllLines(chestContentPath);
                    if (currentChest < lines.Length)
                    {
                        GridObjectInformation chestInformation = new GridObjectInformation
                        {
                            objectName = "Chest Content " + currentChest,
                            chestContent = lines[currentChest]
                        };
                        gridInformation.gridObjects.Add(chestInformation);
                        currentChest++;
                    }
                }
            }
        }

        BoundsInt bounds = tilemap.cellBounds;
        foreach (var position in bounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(position);
            if (tile != null)
            {
                TileInformation tileInformation = new TileInformation
                {
                    position = position,
                    tileName = tile.name 
                };
                gridInformation.tilemapData.tiles.Add(tileInformation);
            }
        }

        string filePath = Path.Combine(Application.persistentDataPath, levelName.text + ".json");
        string json = JsonUtility.ToJson(gridInformation, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Grid and Tilemap data saved to: " + filePath);
    }
}
