using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GridLoader : MonoBehaviour
{
    private GameObject grid; // Prefab of the grid object to instantiate
    private Tilemap tilemap; // Reference to the Tilemap component
    private string filePath = "Assets/CustomLevels/";
    private string prefabPath = "Assets/_Prefabs/";
    
    private Button button;
    public GameObject barrel;
        
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
            foreach (var gridObjectInformation in gridInformation.gridObjects)
            {
                createObject(gridObjectInformation.objectName, gridObjectInformation.position);
            }

            string tilePath = "Assets/GROUND TILESETS RULE TILES/Ground Tiles V3/Rule Tiles/";
            // Load tilemap tiles
            foreach (var tileInformation in gridInformation.tilemapData.tiles)
            {
                Debug.Log(tilePath + tileInformation.tileName);
                #if UNITY_EDITOR // TODO: make independent from Editor
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
                #endif
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
        else
        {
            return null;
        }
        Debug.Log(prefabPath + objectName);
        
        GameObject prefab = Resources.Load<GameObject>(objectName);

        if (prefab)
        {
            return Instantiate(prefab, objectPosition, Quaternion.identity);
        }

        return null;
    }

    private void CreateNewScene()
    {
        // Step 1: Create a new Scene
        Scene newScene = SceneManager.CreateScene("CustomLevel");
        
        // Get the current active scene
        Scene oldScene = SceneManager.GetActiveScene();

        // Step 2: Create a Grid GameObject in the new Scene
        grid = new GameObject("Grid");
        grid.AddComponent<Grid>();
        grid.GetComponent<Grid>().cellSwizzle = GridLayout.CellSwizzle.XZY;

        // Step 3: Move the Grid GameObject to the new Scene
        SceneManager.MoveGameObjectToScene(grid, newScene);

        // Step 4: Create a Tilemap GameObject as a child of the Grid
        GameObject tilemapObject = new GameObject("Tilemap");
        tilemapObject.transform.SetParent(grid.transform);
        tilemap = tilemapObject.AddComponent<UnityEngine.Tilemaps.Tilemap>();
        tilemap.orientation = Tilemap.Orientation.XZ;
        tilemapObject.AddComponent<UnityEngine.Tilemaps.TilemapRenderer>();
        
        // Step 5: Add a Camera to the new Scene
        GameObject cameraObject = new GameObject("Main Camera");
        Camera cameraComponent = cameraObject.AddComponent<Camera>();
        cameraComponent.clearFlags = CameraClearFlags.Skybox;
        cameraObject.tag = "MainCamera"; // Tag it as Main Camera
        cameraObject.transform.position = new Vector3(0, 10, -10); // Adjust position
        cameraObject.transform.rotation = Quaternion.Euler(45, 0, 0); // Adjust rotation
        SceneManager.MoveGameObjectToScene(cameraObject, newScene);

        // Step 6: Add a Directional Light to the new Scene
        GameObject lightObject = new GameObject("Directional Light");
        Light lightComponent = lightObject.AddComponent<Light>();
        lightComponent.type = LightType.Directional;
        lightComponent.intensity = 1f; // Set light intensity
        lightObject.transform.rotation = Quaternion.Euler(50, -30, 0); // Adjust rotation
        SceneManager.MoveGameObjectToScene(lightObject, newScene);

        // Step 5: Switch to the new Scene
        SceneManager.SetActiveScene(newScene);
        
        // Step 6: Unload the old Scene (asynchronously)
        SceneManager.UnloadSceneAsync(oldScene);
    }
}