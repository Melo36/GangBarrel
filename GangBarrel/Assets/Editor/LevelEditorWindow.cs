using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelEditorWindow : EditorWindow
{
    private Tilemap tilemap;  // Reference to the target Tilemap
    private TileBase selectedTile;  // The tile you want to paint
    private GameObject selectedObject;  // The 3D object to place
    private Vector3Int selectedPosition;  // The position to place the tile/object

    [MenuItem("Tools/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Editor", EditorStyles.boldLabel);

        // Tilemap field to select target Tilemap
        tilemap = (Tilemap)EditorGUILayout.ObjectField("Target Tilemap", tilemap, typeof(Tilemap), true);

        // Tile field to select the Tile to paint
        selectedTile = (TileBase)EditorGUILayout.ObjectField("Tile to Paint", selectedTile, typeof(TileBase), false);

        // Object field to select the 3D object
        selectedObject = (GameObject)EditorGUILayout.ObjectField("Object to Place", selectedObject, typeof(GameObject), false);

        if (tilemap != null)
        {
            if (GUILayout.Button("Start Painting"))
            {
                SceneView.duringSceneGui += OnSceneGUI;
            }

            if (GUILayout.Button("Stop Painting"))
            {
                SceneView.duringSceneGui -= OnSceneGUI;
            }
        }
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
            if (selectedTile != null)
            {
                Undo.RecordObject(tilemap, "Place Tile");
                tilemap.SetTile(cellPosition, selectedTile);
            }

            // Place the selected 3D object
            if (selectedObject != null)
            {
                GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(selectedObject);
                newObject.transform.position = tilemap.GetCellCenterWorld(cellPosition);
                Undo.RegisterCreatedObjectUndo(newObject, "Place Object");
            }

            // Mark the event as used to prevent other handlers from processing it
            e.Use();

            // Repaint the Scene view to show the updated tile
            SceneView.RepaintAll();
        }
    }
}
