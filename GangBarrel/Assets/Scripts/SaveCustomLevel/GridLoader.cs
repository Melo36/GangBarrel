using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using Chest;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GameManager;

public class GridLoader : MonoBehaviour
{
    private GameObject grid; // Prefab of the grid object to instantiate
    private Tilemap tilemap; // Reference to the Tilemap component
    private string filePath = "Assets/CustomLevels/";
    private string prefabPath = "Assets/_Prefabs/";
    
    private Button button;

    public List<Item> items;
    public GameObject necessaryScriptsPrefab;
    private GameObject instatiatedScripts;

    private GameManager.GameManager gameManager;

    private Transform playerTransform;
        
    // objects displayed in the ui
    private GameObject currentChest;
        
    private void Start()
    {
        string levelName = GetComponentInChildren<TextMeshProUGUI>().text;
        Debug.Log(Application.dataPath);
        filePath = filePath + levelName + ".json";
        button = GetComponent<Button>();
        button.onClick.AddListener(makeCustomLevel);
        
    }

    private void makeCustomLevel()
    {
        CreateNewScene();
        LoadGridAndTilemapData();
    }
    
    public void LoadGridAndTilemapData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            GridInformation gridInformation = JsonUtility.FromJson<GridInformation>(json);

            // Load grid objects
            for (int i=0; i < gridInformation.gridObjects.Count;i++)
            {
                GameObject newObject = createObject(gridInformation.gridObjects[i].objectName, gridInformation.gridObjects[i].position);
                if (newObject != null && newObject.name == "LChest(Clone)")
                {
                    string chestContent = gridInformation.gridObjects[i + 1].chestContent;
                    setChestContent(newObject, chestContent);
                    currentChest = newObject;
                } else if (newObject != null && newObject.name == "LKey(Clone)")
                {
                    GameObject keyObject = newObject.transform.Find("KeyPrefab").gameObject;
                    GameObject chestObject = currentChest.transform.Find("chest_close").gameObject;
                    gameManager.keyChestPairs.Add(keyObject, chestObject);
                }
            }

            string tilePath = "Assets/GROUND TILESETS RULE TILES/Ground Tiles V3/Rule Tiles/";
            // Load tilemap tiles
            foreach (var tileInformation in gridInformation.tilemapData.tiles)
            {
                TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(tilePath + tileInformation.tileName + ".asset"); // Assumes tiles are stored as assets in Resources
                if (tile != null)
                {
                    // Remove water tile in that position
                    // Convert grid position to world position using the grid's CellToWorld function
                    Vector3 tileWorldPosition = tilemap.layoutGrid.CellToWorld(tileInformation.position);

                    // Adjust for the XZY swizzle by swapping the Y and Z axes
                    Vector3 adjustedPosition = new Vector3(tileWorldPosition.x, tileWorldPosition.z, tileWorldPosition.y);

                    // Find the water object at this position
                    Collider[] colliders = Physics.OverlapSphere(adjustedPosition, 0.5f); // Use a small radius to find nearby objects
                    foreach (var collider in colliders)
                    {
                        if (collider.gameObject.CompareTag("Water")) // Ensure it matches the "Water" tag
                        {
                            Destroy(collider.gameObject); // Destroy the water object
                            Debug.Log($"Destroyed water object at {tileInformation.position}");
                            break;
                        }
                    }
                    
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

    private void setChestContent(GameObject chest, string chestContentString)
    {
        ChestContent chestContent = chest.GetComponentInChildren<ChestContent>();
        // Split the string into a string array
        string[] stringArray = chestContentString.Split(',');

        // Convert string array to int array
        int[] intArray = new int[stringArray.Length];
        for (int i = 0; i < stringArray.Length; i++)
        {
            intArray[i] = int.Parse(stringArray[i]); // Parse each string to an int
        }

        for (int i = 0; i < intArray.Length; i++)
        {
            for (int j = 0; j < intArray[i]; j++)
            {
                chestContent.AddItem(items[i]);
            }
        }
    }

    private GameObject createObject(string objectName, Vector3 objectPosition)
    {
        int cutParentheses = objectName.IndexOf("(");
        if (cutParentheses != -1)
        {
            objectName = objectName.Substring(0, cutParentheses);
        }
        else
        {
            return null;
        }
        Debug.Log(prefabPath + objectName);

        
        if (objectName == "LPlayer")
        {
            playerTransform = instatiatedScripts.transform.Find("Character_pirate");
            if (playerTransform)
            {
                Debug.Log("Found player");
            }
            playerTransform.position = objectPosition;
            playerTransform.rotation = Quaternion.identity;
        }
        else
        {
            GameObject prefab = Resources.Load<GameObject>(objectName);
            if (prefab)
            {
                return Instantiate(prefab, objectPosition, Quaternion.identity);
            }
        }
        return null;
    }

    private void CreateNewScene()
    {
        // Step 1: Create a new Scene
        Scene newScene = SceneManager.CreateScene("CustomLevel");
        
        // Get the current active scene
        Scene oldScene = SceneManager.GetActiveScene();
        
        // Step 5: Instantiate all necessary scripts
        instatiatedScripts = Instantiate(necessaryScriptsPrefab);
        SceneManager.MoveGameObjectToScene(instatiatedScripts, newScene);
        ItemUsage itemUsage = instatiatedScripts.GetComponentInChildren<ItemUsage>();
        tilemap = instatiatedScripts.GetComponentInChildren<Tilemap>();
        itemUsage.tilemapGrid = tilemap;

        gameManager = instatiatedScripts.GetComponentInChildren<GameManager.GameManager>();
        

        // Step 6: Switch to the new Scene
        SceneManager.SetActiveScene(newScene);
        
        // Step 7: Unload the old Scene (asynchronously)
        SceneManager.UnloadSceneAsync(oldScene);
    }
}