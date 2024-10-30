using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileDetection : MonoBehaviour
{
    private Tilemap tileMap;
    
    private Vector3Int location;

    public Camera mainCamera;
    void Start()
    {
        tileMap = GetComponentInChildren<Tilemap>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.transform.position.z);
            Vector3 mp = mainCamera.ScreenToWorldPoint(mousePosition);
            location = tileMap.WorldToCell(mp);
            Debug.Log("Position x: " + location.x + " y: " + location.y + " z: " + location.z);
        }
    }
}
