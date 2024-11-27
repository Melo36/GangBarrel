using UnityEngine;

public class BarrelDirectionHandler : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRange = 5f;
    private BarrelExplosionController explosionController;
    private Transform nearestBarrel;

    private void Update()
    {
        FindNearestBarrel();
    }

    private void FindNearestBarrel()
    {
        // Find all barrels in the scene (assuming they have a common tag "Barrel")
        GameObject[] barrels = GameObject.FindGameObjectsWithTag("Barrel");

        float closestDistance = detectionRange;
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

        nearestBarrel = closestBarrel;

        // Update the explosionController reference if a nearest barrel is found
        if (nearestBarrel != null)
        {
            explosionController = nearestBarrel.GetComponent<BarrelExplosionController>();
        }
    }

    public Quaternion GetExplosionRotation()
    {
        if (nearestBarrel == null) 
        {
            return Quaternion.identity; // Return default rotation if no barrel found
        }

        Vector3 barrelDiff = nearestBarrel.position - transform.position;

        // Determine rotation based on the barrel's relative position
        if (barrelDiff.z > Mathf.Abs(barrelDiff.x)) // North
        {
            return Quaternion.Euler(0, 0, 0);
        }
        else if (barrelDiff.z < -Mathf.Abs(barrelDiff.x)) // South
        {
            return Quaternion.Euler(0, 180, 0);
        }
        else if (barrelDiff.x > 0) // East
        {
            return Quaternion.Euler(0, 90, 0);
        }
        else // West
        {
            return Quaternion.Euler(0, -90, 0);
        }
    }
}
