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
    private ObjectsDatabseSO database;
    private int selectedObjectIndex = -1;

    [SerializeField] 
    private GameObject gridVisualization;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StopPlacement();
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
        inputManager.onClicked += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    private void PlaceStructure()
    {
        if (inputManager.isPointerOverUI())
        {
            return;
        }
        audioSource.Play();
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        GameObject newObject = Instantiate(database.objectsData[selectedObjectIndex].Prefab);
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        newObject.transform.position = grid.CellToWorld(gridPosition);
    }

    private void StopPlacement()
    {
        selectedObjectIndex = -1;
        gridVisualization.SetActive(false);
        cellIndicator.SetActive(false);
        inputManager.onClicked -= PlaceStructure;
        inputManager.OnExit -= StopPlacement;
    }

    private void Update()
    {
        if (selectedObjectIndex < 0)
        {
            return;
        }
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        mouseIndicator.transform.position = mousePosition;
        
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        cellIndicator.transform.position = grid.CellToWorld(gridPosition);
    }
}
