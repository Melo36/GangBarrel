using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] 
    private GameObject mouseIndicator, cellIndicator;

    [SerializeField] 
    private Grid grid;

    [SerializeField] 
    private InputManager inputManager;

    [SerializeField] 
    private ObjectsDatabaseSO database;
    private int selectedObjectIndex = -1;

    [SerializeField] 
    private GameObject gridVisualization;

    private AudioSource audioSource;

    private GridData floorData, furnitureData;

    private Renderer previewRenderer;

    private List<GameObject> placedGameObjects = new();

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StopPlacement();
        floorData = new();
        furnitureData = new();
        previewRenderer = cellIndicator.GetComponentInChildren<Renderer>();
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
        cellIndicator.SetActive(true);
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
            return;
        }
        audioSource.Play();
        GameObject newObject = Instantiate(database.objectsData[selectedObjectIndex].Prefab);
        newObject.transform.position = grid.CellToWorld(gridPosition);
        placedGameObjects.Add(newObject);
        GridData selectedData = database.objectsData[selectedObjectIndex].ID == -100 ? floorData : furnitureData;
        selectedData.AddObjectAt(gridPosition,database.objectsData[selectedObjectIndex].Size, database.objectsData[selectedObjectIndex].ID, placedGameObjects.Count-1);
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
        cellIndicator.SetActive(false);
        inputManager.OnClicked -= PlaceStructure;
        inputManager.OnExit -= StopPlacement;
    }

    private void Update()
    {
        if (selectedObjectIndex < 0)
        {
            return;
        }
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        bool placementValidity = CheckPlacementValidity(gridPosition);
        previewRenderer.material.color = placementValidity ? Color.white : Color.red;
        
        mouseIndicator.transform.position = mousePosition;
        cellIndicator.transform.position = grid.CellToWorld(gridPosition);
    }
}
