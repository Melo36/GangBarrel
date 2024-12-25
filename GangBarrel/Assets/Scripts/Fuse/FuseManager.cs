using System.Collections.Generic;
using UnityEngine;

public class FuseManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject fusePrefab;
    [SerializeField] private Material fuseMaterial;
    
    [Header("Settings")]
    [SerializeField] private float defaultBurnSpeed = 1f;
    [SerializeField] private Color fuseColor = Color.yellow;
    
    private List<Fuse> activeFuses = new List<Fuse>();

    private void Awake()
    {
        // Validate required references
        if (fusePrefab == null)
        {
            Debug.LogError("Fuse Prefab is not assigned to FuseManager!");
        }
    }

    public Fuse CreateFuse(Vector3[] pathPoints, BarrelExplosionController barrel)
    {
        if (fusePrefab == null) return null;

        GameObject fuseObj = Instantiate(fusePrefab);
        Fuse fuse = fuseObj.GetComponent<Fuse>();
        
        if (fuse == null)
        {
            Debug.LogError("Fuse component not found on fusePrefab!");
            Destroy(fuseObj);
            return null;
        }

        // Initialize the fuse
        fuse.Initialize(pathPoints, barrel);
        activeFuses.Add(fuse);

        // Configure LineRenderer if material is assigned
        LineRenderer lineRenderer = fuseObj.GetComponent<LineRenderer>();
        if (lineRenderer != null && fuseMaterial != null)
        {
            lineRenderer.material = fuseMaterial;
            lineRenderer.startColor = fuseColor;
            lineRenderer.endColor = fuseColor;
        }

        return fuse;
    }

    public void ClearAllFuses()
    {
        foreach (var fuse in activeFuses)
        {
            if (fuse != null)
                Destroy(fuse.gameObject);
        }
        activeFuses.Clear();
    }

    private void OnDestroy()
    {
        ClearAllFuses();
    }
}