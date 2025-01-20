using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Chest;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class LevelEditorWindow : EditorWindow
{
    private Tilemap tilemap;  // Reference to the target Tilemap
    private TileBase[] tilePalette;  // Array to store your predefined tiles
    private int selectedTileIndex = -1;  // The index of the currently selected tile
    private GameObject[] objectPalette;  // Array to store your predefined game objects
    private int selectedObjectIndex = -1;  // The index of the currently selected game object

    // Variables to track whether the palettes are collapsed or expanded
    private bool showTilePalette = true;
    private bool showObjectPalette = true;

    // Array sizes
    private int tilePaletteSize = 5;  // Default size for tile palette
    private int objectPaletteSize = 5;  // Default size for object palette

    private const string TilePaletteKey = "LevelEditor_TilePalette";
    private const string TilemapKey = "LevelEditor_Tilemap";
    private const string ObjectPaletteKey = "LevelEditor_ObjectPalette";
    private const string TilePaletteSizeKey = "LevelEditor_TilePaletteSize";
    private const string ObjectPaletteSizeKey = "LevelEditor_ObjectPaletteSize";
    
    private bool isPainting = false; // New flag to track painting mode
    
    // Placing chest-key pair onto the tile-map, requires the following members
    private bool isPlacingKeyChestPair = false;
    private GameObject chestPrefab;
    private GameObject keyPrefab;
    private GameObject placedChest;
    private GameObject placedKey;
    private Color currentKeyChestColor;

    private TileBase waterTile;
    private GameObject waterPrefab;
    private GameObject waterParent;
    
    private void FillEmptySpacesWithWater()
    {
        waterParent = new GameObject("WaterObjects");
        // Iterate through all positions within the bounds
        for (int x = -50; x < 50; x++)
        {
            for (int y = -50; y < 50; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                Vector3 worldPosition = tilemap.GetCellCenterWorld(tilePosition);
                
                // Check if the position is empty
                if (tilemap.GetTile(tilePosition) == null)
                {
                    // Set the default tile
                    tilemap.SetTile(tilePosition, waterTile);
                    GameObject waterObject;
                    
                    // Instantiate the prefab instead of the GameObject. (only useful in UNITY_EDITOR mode.)
                    #if UNITY_EDITOR
                    waterObject = PrefabUtility.InstantiatePrefab(waterPrefab) as GameObject;
                    waterObject.transform.position = worldPosition;
                    #else
                        waterObject = Instantiate(waterPrefab, worldPosition, Quaternion.identity);
                    #endif
                    waterObject.transform.SetParent(waterParent.transform);
                }
            }
        }
    }
    
    private void RemoveWaterTiles()
    {
        // Iterate through all positions within the bounds
        for (int x = -50; x < 50; x++)
        {
            for (int y = -50; y < 50; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                Vector3 worldPosition = tilemap.GetCellCenterWorld(tilePosition);
                
                // Check if the position is empty
                if (tilemap.GetTile(tilePosition) == waterTile)
                {
                    // Set the default tile
                    tilemap.SetTile(tilePosition, null);
                }
            }
        }
        DestroyImmediate(waterParent);
    }
    
    private List<Item> currentChestItems = new List<Item>(); // Store the current chest items
    
        public static Color GetRandomNearWhite(float maxDeviation = 0.6f)
        {
            // Clamp the deviation to ensure it's within valid range
            maxDeviation = Mathf.Clamp(maxDeviation, 0f, 1f);

            // Generate RGB values near 1.0 (white) with a small random deviation
            float r = 1.0f - Random.Range(0, maxDeviation);
            float g = 1.0f - Random.Range(0, maxDeviation);
            float b = 1.0f - Random.Range(0, maxDeviation);

            return new Color(r, g, b);
        }
        
    private void PlaceChest(Vector3Int cellPosition)
    {
        // currentKeyChestColor = GetRandomNearWhite();
        
        currentKeyChestColor = Color.white;
        
        Vector3 worldPosition = tilemap.GetCellCenterWorld(cellPosition);
        placedChest = (GameObject)PrefabUtility.InstantiatePrefab(chestPrefab);

        placedChest.GetComponent<Renderer>().sharedMaterial.color = currentKeyChestColor;

        placedChest.GetComponent<ChestContent>().items = new List<Item>(currentChestItems);
        
        placedChest.transform.position = worldPosition;
        Debug.Log("Chest placed at: " + cellPosition);
    }

    private void PlaceKey(Vector3Int cellPosition)
    {
        Vector3 worldPosition = tilemap.GetCellCenterWorld(cellPosition);
        placedKey = (GameObject)PrefabUtility.InstantiatePrefab(keyPrefab);
        placedKey.transform.position = worldPosition;
        
        placedKey.GetComponent<Renderer>().sharedMaterial.color = currentKeyChestColor;
        
        Debug.Log("Key placed at: " + cellPosition);

        FinalizeKeyChestPair();
    }

    private void FinalizeKeyChestPair()
    {
        isPlacingKeyChestPair = false;
        SceneView.duringSceneGui -= OnSceneGUI;

        var gameManager = FindObjectOfType<GameManager.GameManager>();
        Debug.Log($"gameManager == null : {gameManager == null}");
        if (gameManager != null)
        {
            gameManager.AddKeyChestPair(placedChest, placedKey);
        }

        EditorUtility.DisplayDialog("Success", "Key-chest pair successfully placed and added to GameManager!", "OK");

        placedChest = null;
        placedKey = null;
    }

    private void CheckAndCleanKeyChestPairs()
    {
        var gameManager = FindObjectOfType<GameManager.GameManager>();
        if (gameManager == null) return;

        var pairsToRemove = new List<GameObject>();

        if (gameManager.keyChestPairs == null)
            return;
        
        foreach (var pair in gameManager.keyChestPairs)
        {
            if (pair.Key == null || pair.Value == null)
            {
                if (pair.Key != null) DestroyImmediate(pair.Key);
                if (pair.Value != null) DestroyImmediate(pair.Value);

                pairsToRemove.Add(pair.Key);
            }
        }

        foreach (var key in pairsToRemove)
        {
            gameManager.keyChestPairs.Remove(key);
        }
    }

    [MenuItem("Tools/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnEnable()
    {
        LoadData();  // Load data when window opens
    }

    private void OnDisable()
    {
        SaveData();  // Save data when window closes
    }

    private void LoadData()
    {
        // Load Tilemap
        int tilemapID = EditorPrefs.GetInt(TilemapKey, -1);
        if (tilemapID != -1)
        {
            tilemap = EditorUtility.InstanceIDToObject(tilemapID) as Tilemap;
        }

        // Load Tile Palette
        string tilePaletteJson = EditorPrefs.GetString(TilePaletteKey, "");
        if (!string.IsNullOrEmpty(tilePaletteJson))
        {
            SerializableTilePalette loadedTilePalette = JsonUtility.FromJson<SerializableTilePalette>(tilePaletteJson);
            tilePalette = loadedTilePalette.tiles;
            tilePaletteSize = tilePalette.Length;
        }
        else
        {
            tilePalette = new TileBase[tilePaletteSize];
        }

        // Load Object Palette
        string objectPaletteJson = EditorPrefs.GetString(ObjectPaletteKey, "");
        if (!string.IsNullOrEmpty(objectPaletteJson))
        {
            SerializableObjectPalette loadedObjectPalette = JsonUtility.FromJson<SerializableObjectPalette>(objectPaletteJson);
            objectPalette = loadedObjectPalette.objects;
            objectPaletteSize = objectPalette.Length;
        }
        else
        {
            objectPalette = new GameObject[objectPaletteSize];
        }
    }

    private void SaveData()
    {
        // Save Tilemap
        if (tilemap != null)
        {
            EditorPrefs.SetInt(TilemapKey, tilemap.GetInstanceID());
        }
        else
        {
            EditorPrefs.DeleteKey(TilemapKey);
        }

        // Save Tile Palette
        SerializableTilePalette tilePaletteData = new SerializableTilePalette(tilePalette);
        string tilePaletteJson = JsonUtility.ToJson(tilePaletteData);
        EditorPrefs.SetString(TilePaletteKey, tilePaletteJson);

        // Save Object Palette
        SerializableObjectPalette objectPaletteData = new SerializableObjectPalette(objectPalette);
        string objectPaletteJson = JsonUtility.ToJson(objectPaletteData);
        EditorPrefs.SetString(ObjectPaletteKey, objectPaletteJson);
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Level Editor", EditorStyles.boldLabel);

        // Tilemap field to select target Tilemap
        tilemap = (Tilemap)EditorGUILayout.ObjectField("Target Tilemap", tilemap, typeof(Tilemap), true);

        // Ensure the Tilemap is assigned before showing further options
        if (tilemap == null)
        {
            EditorGUILayout.HelpBox("Please assign a Tilemap to use the Level Editor.", MessageType.Warning);
            return;
        }

        // Collapsible Tile Palette
        showTilePalette = EditorGUILayout.Foldout(showTilePalette, "Tile Palette", true);
        if (showTilePalette)
        {
            EditorGUI.indentLevel++;
            tilePaletteSize = Mathf.Max(1, EditorGUILayout.IntField("Tile Palette Size", tilePaletteSize));
            ResizeTilePalette(tilePaletteSize);

            for (int i = 0; i < tilePalette.Length; i++)
            {
                tilePalette[i] = (TileBase)EditorGUILayout.ObjectField($"Tile {i + 1}", tilePalette[i], typeof(TileBase), false);
            }

            GUILayout.Label("Select a Tile to Paint:");
            GUILayout.BeginHorizontal();
            for (int i = 0; i < tilePalette.Length; i++)
            {
                if (tilePalette[i] != null && GUILayout.Button($"Tile {i + 1}"))
                {
                    selectedTileIndex = i;
                    selectedObjectIndex = -1; // Deselect any object
                }
            }
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        // Collapsible Object Palette
        showObjectPalette = EditorGUILayout.Foldout(showObjectPalette, "Object Palette", true);
        if (showObjectPalette)
        {
            EditorGUI.indentLevel++;
            objectPaletteSize = Mathf.Max(1, EditorGUILayout.IntField("Object Palette Size", objectPaletteSize));
            ResizeObjectPalette(objectPaletteSize);

            for (int i = 0; i < objectPalette.Length; i++)
            {
                objectPalette[i] = (GameObject)EditorGUILayout.ObjectField($"Object {i + 1}", objectPalette[i], typeof(GameObject), false);
            }

            GUILayout.Label("Select an Object to Place:");
            GUILayout.BeginHorizontal();
            for (int i = 0; i < objectPalette.Length; i++)
            {
                if (objectPalette[i] != null && GUILayout.Button($"Object {i + 1}"))
                {
                    selectedObjectIndex = i;
                    selectedTileIndex = -1; // Deselect any tile
                }
            }
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        chestPrefab = (GameObject)EditorGUILayout.ObjectField("Chest Prefab", chestPrefab, typeof(GameObject), false);
        keyPrefab = (GameObject)EditorGUILayout.ObjectField("Key Prefab", keyPrefab, typeof(GameObject), false);
        
        // Display Chest Content Selection
        GUILayout.Label("Configure Chest Content:", EditorStyles.boldLabel);
        int itemCount = Mathf.Max(0, EditorGUILayout.IntField("Number of Items", currentChestItems.Count));
        while (currentChestItems.Count < itemCount)
        {
            currentChestItems.Add(null);
        }
        while (currentChestItems.Count > itemCount)
        {
            currentChestItems.RemoveAt(currentChestItems.Count - 1);
        }

        for (int i = 0; i < currentChestItems.Count; i++)
        {
            currentChestItems[i] = (Item)EditorGUILayout.ObjectField($"Item {i + 1}", currentChestItems[i], typeof(Item), false);
        }
        
        // Placing Key Chest Pair
        if (GUILayout.Button("Start Placing Key-Chest Pair"))
        {
            StartPlacingKeyChestPair();
        }
        
        // Painting Buttons
        if (!isPainting && GUILayout.Button("Start Painting"))
        {
            StartPainting();
        }

        waterTile = (TileBase)EditorGUILayout.ObjectField("Water Tile", waterTile, typeof(TileBase), false);
        waterPrefab = (GameObject)EditorGUILayout.ObjectField("Water Prefab", waterPrefab, typeof(GameObject), false);

        if (isPainting && GUILayout.Button("Stop Painting"))
        {
            StopPainting();
        }
        
        if (GUILayout.Button("Fill Empty Spaces With Water"))
        {
            FillEmptySpacesWithWater();
        }
        
        if (GUILayout.Button("Remove Water Tiles"))
        {
            RemoveWaterTiles();
        }
        
        if (isPlacingKeyChestPair && placedChest == null)
        {
            GUILayout.Label("You are currently placing a key-chest pair. First, place the chest.", EditorStyles.helpBox);
        }
        else if (isPlacingKeyChestPair && placedChest != null && placedKey == null)
        {
            GUILayout.Label("Now place the key.", EditorStyles.helpBox);
        }
        
    }
    
    private void StartPlacingKeyChestPair()
    {
        if (chestPrefab == null || keyPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign both a chest and a key prefab before starting.", "OK");
            return;
        }

        isPlacingKeyChestPair = true;
        SceneView.duringSceneGui += OnSceneGUI;
    }


    private void StartPainting()
    {
        Debug.Log("Start Painting");
        if (!isPainting)
        {
            SceneView.duringSceneGui += OnSceneGUI;
            isPainting = true;
        }
    }

    private void StopPainting()
    {
        if (isPainting)
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            isPainting = false;
        }
    }

    private void OnDestroy()
    {
        StopPainting();
    }

    // Resize the tile palette array while preserving existing values
    private void ResizeTilePalette(int newSize)
    {
        TileBase[] newTilePalette = new TileBase[newSize];
        for (int i = 0; i < Mathf.Min(newSize, tilePalette.Length); i++)
        {
            newTilePalette[i] = tilePalette[i];
        }
        tilePalette = newTilePalette;
    }

    // Resize the object palette array while preserving existing values
    private void ResizeObjectPalette(int newSize)
    {
        GameObject[] newObjectPalette = new GameObject[newSize];
        for (int i = 0; i < Mathf.Min(newSize, objectPalette.Length); i++)
        {
            newObjectPalette[i] = objectPalette[i];
        }
        objectPalette = newObjectPalette;
    }
    
    private void OnSceneGUI(SceneView sceneView)
    {
        if (tilemap == null) return;

        Debug.Log("OnSceneGUI");
        
        Event e = Event.current;
        if (isPlacingKeyChestPair && e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            if (!plane.Raycast(ray, out float distance)) return;

            Vector3 worldPosition = ray.GetPoint(distance);
            Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);

            if (placedChest == null)
            {
                PlaceChest(cellPosition);
            }
            else if (placedKey == null)
            {
                PlaceKey(cellPosition);
            }

            e.Use();
            SceneView.RepaintAll();
        }
        else if (e.type == EventType.MouseDown && e.button == 0 && tilemap != null)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            if (!plane.Raycast(ray, out float distance)) return;

            Vector3 worldPosition = ray.GetPoint(distance);
            Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);

            if (selectedTileIndex >= 0 && selectedTileIndex < tilePalette.Length && tilePalette[selectedTileIndex] != null)
            {
                Undo.RecordObject(tilemap, "Place Tile");
                tilemap.SetTile(cellPosition, tilePalette[selectedTileIndex]);

                if (tilePalette[selectedTileIndex] is CustomTile customTile && customTile.prefab != null)
                {
                    GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(customTile.prefab);
                    newObject.transform.position = tilemap.GetCellCenterWorld(cellPosition);

                    customTile.RefreshTile(cellPosition, tilemap);

                    Undo.RegisterCreatedObjectUndo(newObject, "Place Prefab");
                }
            }
            else if (selectedObjectIndex >= 0 && selectedObjectIndex < objectPalette.Length && objectPalette[selectedObjectIndex] != null)
            {
                GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(objectPalette[selectedObjectIndex]);

                Vector3 cellCenter = tilemap.GetCellCenterWorld(cellPosition);
                newObject.transform.position = new Vector3(cellCenter.x, cellCenter.y, cellCenter.z);

                Undo.RegisterCreatedObjectUndo(newObject, "Place Object");
            }

            e.Use();
            SceneView.RepaintAll();
        }

        CheckAndCleanKeyChestPairs();
    }



    // Helper classes for serialization
    [System.Serializable]
    private class SerializableTilePalette
    {
        public TileBase[] tiles;

        public SerializableTilePalette(TileBase[] tiles)
        {
            this.tiles = tiles;
        }
    }

    [System.Serializable]
    private class SerializableObjectPalette
    {
        public GameObject[] objects;

        public SerializableObjectPalette(GameObject[] objects)
        {
            this.objects = objects;
        }
    }
}
