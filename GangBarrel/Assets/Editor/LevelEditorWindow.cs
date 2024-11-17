using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        // Collapsible Tile Palette
        showTilePalette = EditorGUILayout.Foldout(showTilePalette, "Tile Palette", true);
        if (showTilePalette)
        {
            EditorGUI.indentLevel++;  // Indent content under the foldout

            // Input field for changing tile palette size
            int newTilePaletteSize = EditorGUILayout.IntField("Tile Palette Size", tilePaletteSize);
            if (newTilePaletteSize != tilePaletteSize)
            {
                tilePaletteSize = Mathf.Max(1, newTilePaletteSize);  // Ensure size is at least 1
                ResizeTilePalette(tilePaletteSize);
            }

            // Display tile palette fields
            for (int i = 0; i < tilePalette.Length; i++)
            {
                tilePalette[i] = (TileBase)EditorGUILayout.ObjectField($"Tile {i + 1}", tilePalette[i], typeof(TileBase), false);
            }

            // Display buttons for selecting tiles from the palette
            GUILayout.Label("Select a Tile to Paint:");
            GUILayout.BeginHorizontal();
            for (int i = 0; i < tilePalette.Length; i++)
            {
                if (tilePalette[i] != null)
                {
                    if (GUILayout.Button($"Tile {i + 1}"))
                    {
                        selectedTileIndex = i;
                        selectedObjectIndex = -1; // Deselect any selected object
                    }
                }
            }
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel--;  // Reset indentation
        }

        // Collapsible Object Palette
        showObjectPalette = EditorGUILayout.Foldout(showObjectPalette, "Object Palette", true);
        if (showObjectPalette)
        {
            EditorGUI.indentLevel++;  // Indent content under the foldout

            // Input field for changing object palette size
            int newObjectPaletteSize = EditorGUILayout.IntField("Object Palette Size", objectPaletteSize);
            if (newObjectPaletteSize != objectPaletteSize)
            {
                objectPaletteSize = Mathf.Max(1, newObjectPaletteSize);  // Ensure size is at least 1
                ResizeObjectPalette(objectPaletteSize);
            }

            // Display object palette fields
            for (int i = 0; i < objectPalette.Length; i++)
            {
                objectPalette[i] = (GameObject)EditorGUILayout.ObjectField($"Object {i + 1}", objectPalette[i], typeof(GameObject), false);
            }

            // Display buttons for selecting objects from the palette
            GUILayout.Label("Select an Object to Place:");
            GUILayout.BeginHorizontal();
            for (int i = 0; i < objectPalette.Length; i++)
            {
                if (objectPalette[i] != null)
                {
                    if (GUILayout.Button($"Object {i + 1}"))
                    {
                        selectedObjectIndex = i;
                        selectedTileIndex = -1; // Deselect any selected tile
                    }
                }
            }
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel--;  // Reset indentation
        }

        if (tilemap != null)
        {
            if (!isPainting && GUILayout.Button("Start Painting"))
            {
                StartPainting();
            }

            if (isPainting && GUILayout.Button("Stop Painting"))
            {
                StopPainting();
            }
        }
    }

    private void StartPainting()
    {
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
        Event e = Event.current;

        // Check for left mouse button click
        if (e.type == EventType.MouseDown && e.button == 0 && tilemap != null)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            
            if (!plane.Raycast(ray, out float distance))
            {
                Debug.Log("Raycast hit nothing");
                return;
            }
            // Get the point on the XZ plane where the ray hit
            Vector3 worldPosition = ray.GetPoint(distance);
            
            Debug.Log("Raycast hit at position " + worldPosition);

            // Convert the world position to a cell position in the grid
            Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
            
            // Place the selected tile
            if (selectedTileIndex >= 0 && selectedTileIndex < tilePalette.Length && tilePalette[selectedTileIndex] != null)
            {
                Undo.RecordObject(tilemap, "Place Tile");
                tilemap.SetTile(cellPosition, tilePalette[selectedTileIndex]);
            }

            // Place the selected 3D object if any object from the palette is selected
            if (selectedObjectIndex >= 0 && selectedObjectIndex < objectPalette.Length && objectPalette[selectedObjectIndex] != null)
            {
                GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(objectPalette[selectedObjectIndex]);
                newObject.transform.position = tilemap.GetCellCenterWorld(cellPosition);
                newObject.transform.position = new Vector3(newObject.transform.position.x,
                    newObject.transform.localScale.y / 2, newObject.transform.position.z);
                Undo.RegisterCreatedObjectUndo(newObject, "Place Object");
            }

            // Mark the event as used to prevent other handlers from processing it
            e.Use();

            // Repaint the Scene view to show the updated tile
            SceneView.RepaintAll();
        }
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
