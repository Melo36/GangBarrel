using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] 
    private GameObject mouseIndicator;

    [SerializeField] 
    private Grid grid;

    [SerializeField] 
    private InputManager inputManager;

    [SerializeField] 
    private ObjectsDatabaseSO database;
    private int selectedObjectIndex = -1;

    [SerializeField] 
    private GameObject gridVisualization;

    [SerializeField]
    private AudioSource audioSource;
    
    [SerializeField]
    private AudioSource wrongPlacementAudio;

    private GridData floorData, furnitureData;
    
    private List<GameObject> placedGameObjects = new();

    [SerializeField] 
    private PreviewSystem preview;

    private Vector3Int lastDetectedPosition = Vector3Int.zero;

    private void Start()
    {
        StopPlacement();
        floorData = new();
        furnitureData = new();
    }

    public void StartPlacement(int ID)
    {
        StopPlacement();
        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == ID);
        if (selectedObjectIndex < 0)
        {
            Debug.LogError("No ID found");
            return;
        }
        
        gridVisualization.SetActive(true);
        preview.StartShowingPlacemementPreview(database.objectsData[selectedObjectIndex].Prefab, database.objectsData[selectedObjectIndex].Size);
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    private void PlaceStructure()
    {
        if (inputManager.isPointerOverUI())
        {
            return;
        }
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        bool placementValidity = CheckPlacementValidity(gridPosition);
        if (!placementValidity)
        {
            wrongPlacementAudio.Play();
            return;
        }
        audioSource.Play();
        GameObject newObject = Instantiate(database.objectsData[selectedObjectIndex].Prefab);
        newObject.transform.position = grid.CellToWorld(gridPosition);
        placedGameObjects.Add(newObject);
        GridData selectedData = database.objectsData[selectedObjectIndex].ID == -100 ? floorData : furnitureData;
        selectedData.AddObjectAt(gridPosition,database.objectsData[selectedObjectIndex].Size, database.objectsData[selectedObjectIndex].ID, placedGameObjects.Count-1);
        preview.UpdatePosition(grid.CellToWorld(gridPosition), false);
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition)
    {
        // Adjust this index later on based on how we place tiles
        GridData selectedData = database.objectsData[selectedObjectIndex].ID == -100 ? floorData : furnitureData;
        return selectedData.CanPlaceObjectAt(gridPosition, database.objectsData[selectedObjectIndex].Size);
    }

    private void StopPlacement()
    {
        selectedObjectIndex = -1;
        gridVisualization.SetActive(false);
        preview.StopShowingPreview();
        inputManager.OnClicked -= PlaceStructure;
        inputManager.OnExit -= StopPlacement;
        lastDetectedPosition = Vector3Int.zero;
    }

    private void Update()
    {
        if (selectedObjectIndex < 0)
        {
            return;
        }
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        if (lastDetectedPosition == gridPosition)
        {
            return;
        }
        bool placementValidity = CheckPlacementValidity(gridPosition);
        
        mouseIndicator.transform.position = mousePosition;
        preview.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity);
        lastDetectedPosition = gridPosition;
    }
}
