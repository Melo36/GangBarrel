using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Unity.VisualScripting.Member;

public class PlacementState : IBuildingState
{
    private int selectedObjectIndex = -1;
    int ID;
    Grid grid;
    PreviewSystem previewSystem;
    ObjectsDatabaseSO database;
    GridData floorData;
    GridData furnitureData;
    ObjectPlacer objectPlacer;
    private SoundFeedback soundFeedback;

    public PlacementState(int iD,
                          Grid grid,
                          PreviewSystem previewSystem,
                          ObjectsDatabaseSO database,
                          GridData floorData,
                          GridData furnitureData,
                          ObjectPlacer objectPlacer,
                          SoundFeedback soundFeedback)
    {
        ID = iD;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.database = database;
        this.floorData = floorData;
        this.furnitureData = furnitureData;
        this.objectPlacer = objectPlacer;
        this.soundFeedback = soundFeedback;

        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == ID);
        if (selectedObjectIndex > -1 && selectedObjectIndex >= database.amountOfTiles)
        {
            previewSystem.StartShowingPlacementPreview(
                database.objectsData[selectedObjectIndex].Prefab,
                database.objectsData[selectedObjectIndex].Size);
        }
    }

    public void EndState()
    {
        previewSystem.StopShowingPreview();
    }

    public void OnAction(Vector3Int gridPosition, Tilemap tilemap)
    {
        bool placeTile = database.objectsData[selectedObjectIndex].ID < database.amountOfTiles;

        if (placeTile)
        {
            soundFeedback.PlaySound(SoundType.Place);
            tilemap.SetTile(gridPosition, database.objectsData[selectedObjectIndex].Tile);
        }
        else
        {
            bool placementValidity = CheckPlacementValidity(gridPosition);
            if (placementValidity == false)
            {
                soundFeedback.PlaySound(SoundType.wrongPlacement);
                return;
            }

            soundFeedback.PlaySound(SoundType.Place);
            int index = objectPlacer.PlaceObject(database.objectsData[selectedObjectIndex].Prefab,
                grid.CellToWorld(gridPosition));
            
            furnitureData.AddObjectAt(gridPosition,
                database.objectsData[selectedObjectIndex].Size,
                database.objectsData[selectedObjectIndex].ID,
                index);
        }
        
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), false);
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition)
    {
        return furnitureData.CanPlaceObjectAt(gridPosition, database.objectsData[selectedObjectIndex].Size);
    }

    public void UpdateState(Vector3Int gridPosition)
    {
        bool placementValidity = CheckPlacementValidity(gridPosition);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity);
    }
}