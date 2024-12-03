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
        bool barrel = false;

        if (Input.GetMouseButtonDown(0))
        {
            // Cast a ray from the camera to the point clicked on the screen
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            Vector3 direction = (Input.mousePosition - mainCamera.transform.position).normalized;
            if (Physics.Raycast(mainCamera.transform.position, direction, out RaycastHit hit, Mathf.Infinity))
            {
                Debug.Log("Hit something");
                if (hit.transform.gameObject.CompareTag("Barrel"))
                {
                    barrel = true;
                }
            }

            // Set a plane at y=0 to intersect with the ray (assuming the tilemap is on the XZ plane at y=0)
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            // Check if the ray hits the XZ plane
            if (plane.Raycast(ray, out float distance))
            {
                // Get the point on the XZ plane where the ray hit
                Vector3 worldPosition = ray.GetPoint(distance);

                // Convert the world position to a cell position in the tilemap
                location = tileMap.WorldToCell(worldPosition);

                // Check if a tile exists at this cell location
                TileBase tileBase = tileMap.GetTile(location);

                if (barrel)
                {
                    Debug.Log("Barrel detected at Position x: " + location.x + " y: " + location.y + " z: " + location.z);
                }
                else if (tileBase != null)
                {
                    if (tileBase is CustomTile customTile)
                    {
                        Debug.Log($"The tile with type {customTile.TileType} has been hit at Position x: {location.x}, y: {location.y}, z: {location.z}");
                        customTile.DebugNeighbors(location, tileMap);
                    }
                    else
                    {
                        Debug.Log("Tile detected at Position x: " + location.x + " y: " + location.y + " z: " + location.z);
                    }
                }
                else
                {
                    Debug.Log("No tile at Position x: " + location.x + " y: " + location.y + " z: " + location.z);
                }
            }
        }
    }
}
