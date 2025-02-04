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
    private GameObject waterParent;

    private Transform playerTransform;
        
    // objects displayed in the ui
    private GameObject currentChest;
    Dictionary<Vector3Int, GameObject> waterObjectMap = new Dictionary<Vector3Int, GameObject>();
        
    private void Start()
    {
        string levelName = GetComponentInChildren<TextMeshProUGUI>().text;
        Debug.Log(Application.dataPath);
        filePath = Path.Combine(Application.persistentDataPath, levelName + ".json");
        button = GetComponent<Button>();
        button.onClick.AddListener(makeCustomLevel);
    }

    private void makeCustomLevel()
    {
        CreateNewScene();
        waterParent = instatiatedScripts.transform.Find("WaterObjects").gameObject;
        foreach (Transform water in waterParent.transform)
        {
            Vector3Int cellPosition = tilemap.layoutGrid.WorldToCell(water.transform.position);
            waterObjectMap[cellPosition] = water.gameObject;
        }
        LoadGridAndTilemapData();

        ScanManager.Instance.ScheduleScan(0.1f, "");
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

            foreach (var tileInformation in gridInformation.tilemapData.tiles)
            {
                TileBase tile = Resources.Load<TileBase>("Tiles/" + tileInformation.tileName);
                if (tile != null)
                {
                    if (waterObjectMap.ContainsKey(tileInformation.position))
                    {
                        Destroy(waterObjectMap[tileInformation.position]);
                    }
                    tilemap.SetTile(tileInformation.position, tile);
                }
                else
                {
                    Debug.LogError("Tile " + tileInformation.tileName + " not found in Resources.");
                }
            }
        }
        else
        {
            Debug.LogError("Grid data file not found.");
        }
        
        Debug.Log("Finished tilemap");
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