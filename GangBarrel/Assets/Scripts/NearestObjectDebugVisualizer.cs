using UnityEditor;
using UnityEngine;

/// <summary>
/// Script is not really used and could be deleted!
/// </summary>
public class NearestObjectDebugVisualizer : MonoBehaviour
{
    [Header("References")]
    //public Transform player; // Reference to the player
    public BarrelDirectionHandler barrelDirectionHandler; // To access detection range

    [Header("Debug Settings")]
    public string barrelTag = "Barrel"; // Tag to identify barrels
    private Transform nearestBarrel;
    
    private void OnDrawGizmos()
    {
        if (barrelDirectionHandler == null)
            return;

        // Find the nearest barrel dynamically
        FindNearestBarrel();

        if (nearestBarrel == null)
            return;

        Vector3 barrelPosition = nearestBarrel.position;
        Vector3 diff = transform.position - barrelPosition;

        Debug.Log($"diff = {diff}");
        
        // Check if the barrel is within the detection range
        if (Mathf.Abs(diff.x) >= barrelDirectionHandler.detectionRange || Mathf.Abs(diff.z) >= barrelDirectionHandler.detectionRange)
            return;

        // Visualize the directional cone based on player's position relative to the barrel
        if (diff.z > Mathf.Abs(diff.x)) // North
        {
            DrawDirectionalCone(barrelPosition, Vector3.forward);
        }
        else if (diff.z < -Mathf.Abs(diff.x)) // South
        {
            DrawDirectionalCone(barrelPosition, Vector3.back);
        }
        else if (diff.x > 0) // East
        {
            DrawDirectionalCone(barrelPosition, Vector3.right);
        }
        else if (diff.x < 0) // West
        {
            DrawDirectionalCone(barrelPosition, Vector3.left);
        }
    }

    private void FindNearestBarrel()
    {
        GameObject[] barrels = GameObject.FindGameObjectsWithTag(barrelTag);

        float closestDistance = barrelDirectionHandler.detectionRange;
        Transform closestBarrel = null;

        foreach (GameObject barrel in barrels)
        {
            float distance = Vector3.Distance(transform.position, barrel.transform.position);
            if (distance <= closestDistance)
            {
                closestDistance = distance;
                closestBarrel = barrel.transform;
            }
        }

        if (closestBarrel != null)
        {
            barrelDirectionHandler = closestBarrel.gameObject.GetComponent<BarrelDirectionHandler>();
            nearestBarrel = closestBarrel;   
        }
    }

    private void DrawDirectionalCone(Vector3 origin, Vector3 direction)
    {
        direction = direction.normalized;
        float legLength = barrelDirectionHandler.detectionRange / Mathf.Cos(45f * Mathf.Deg2Rad);

        Vector3 baseLeft = Quaternion.Euler(0, -45, 0) * direction * legLength + origin;
        Vector3 baseRight = Quaternion.Euler(0, 45, 0) * direction * legLength + origin;

        Gizmos.color = Color.magenta;
        //Handles.DrawBezier(origin, baseLeft, origin, baseLeft, Color.magenta, null, 2f);
        //Handles.DrawBezier(origin, baseRight, origin, baseRight, Color.magenta, null, 2f);
        //Handles.DrawBezier(baseLeft, baseRight, baseLeft, baseRight, Color.magenta, null, 2f);
    }
}
