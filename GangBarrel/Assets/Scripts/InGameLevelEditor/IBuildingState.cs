using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public interface IBuildingState
{
    void EndState();
    void OnAction(Vector3Int gridPosition, Tilemap tilemap);
    void UpdateState(Vector3Int gridPosition);
}
