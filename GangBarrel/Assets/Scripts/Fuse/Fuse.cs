using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class Fuse : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem burningEffect;
    
    [Header("Settings")]
    [SerializeField] private float burnSpeed = 1f;
    [SerializeField] private float fuseRadius = 0.025f;
    [SerializeField] private int radialSegments = 8;
    [SerializeField] private float maxFuseLength = 10f;
    
    [Header("Visual Settings")]
    [SerializeField] private Material fuseMaterialActive;
    [SerializeField] private Material fuseMaterialActiveInverted;
    [SerializeField] private Material fuseMaterialInactive;
    [SerializeField] private Color unburntColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    
    private Vector3[] pathPoints;
    private float totalLength;
    public float burnProgress;
    private bool isLit;
    public BarrelExplosionController targetBarrel;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    
    public FuseEndpoint connectedEndpoint; // Reference to any fuse endpoint connected to the start of this fuse
    private bool hasTriggeredNextFuse = false; // To ensure we only trigger once

    public ReactiveProperty<bool> fuseBurnt = new ReactiveProperty<bool>(true); // When fuse is burnt we wait for some time and then destroy all the objects
    public float destroyAfterSeconds;

    public bool directionStart2End; // lighting this fuse from start to end = true, otherwise from end to start.
    public bool listHasBeenReversed = false;
    
    // Add method to set connected endpoint
    public void SetConnectedEndpoint(FuseEndpoint endpoint)
    {
        connectedEndpoint = endpoint;
    }
    
    private void Awake()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        
        // Create instance of inactive material to avoid affecting other fuses
        Material inactiveMaterialInstance = new Material(fuseMaterialInactive);
        inactiveMaterialInstance.color = unburntColor;
        meshRenderer.material = inactiveMaterialInstance;
        
        // Create instance of active material and set its UnburntColor
        Material activeMaterialInstance = new Material(fuseMaterialActive);
        activeMaterialInstance.SetColor("_UnburntColor", unburntColor);
        fuseMaterialActive = activeMaterialInstance;
        
        fuseBurnt
            .Where(burnt => burnt) // Only when changing to true
            .Subscribe(_ =>
            {
                // Immediately destroy particles
                if (burningEffect != null)
                {
                    Destroy(burningEffect.gameObject);
                }

                // Destroy gameObject after delay
                Observable.Timer(TimeSpan.FromSeconds(destroyAfterSeconds))
                    .Subscribe(__ => 
                    {
                        Destroy(gameObject);
                    })
                    .AddTo(this); // Ensure proper cleanup if object is destroyed early
            })
            .AddTo(this);
        
    }


    // Modify Initialize to handle merged paths
    public void Initialize(Vector3[] points, BarrelExplosionController barrel)
    { 
        if(points == null || points.Length < 2)
        {
            Debug.LogError("Invalid path points provided to Fuse.Initialize");
            return;
        }

        // Manually reverse the path points array
        pathPoints = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            pathPoints[i] = points[points.Length - 1 - i];
        }
        
        targetBarrel = barrel;
        
        // Calculate total length for burn duration
        totalLength = 0f;
        for (int i = 1; i < pathPoints.Length; i++)
        {
            totalLength += Vector3.Distance(pathPoints[i - 1], pathPoints[i]);
        }

        CreateFuseMesh();

        burnProgress = 0f;
        isLit = false;

        if (burningEffect != null)
        {
            burningEffect.gameObject.SetActive(false);
        }

        UpdateFuseMaterial();
    }


    private void CreateFuseMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        // Calculate total path length and segment lengths
        float accumulatedLength = 0f;
        float[] segmentLengths = new float[pathPoints.Length - 1];
        
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            segmentLengths[i] = Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
            accumulatedLength += segmentLengths[i];
        }
        
        // Create vertices around the path
        float currentLength = 0f;
        for (int i = 0; i < pathPoints.Length; i++)
        {
            if (i > 0)
            {
                currentLength += segmentLengths[i - 1];
            }
            
            Vector3 center = pathPoints[i];
            Vector3 forward;

            // Handle edge cases for forward vector
            if (i == 0) // First point
            {
                forward = (pathPoints[1] - center).normalized;
            }
            else if (i == pathPoints.Length - 1) // Last point
            {
                forward = (center - pathPoints[i - 1]).normalized;
            }
            else // Middle points
            {
                forward = (pathPoints[i + 1] - center).normalized;
            }
            
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 up = Vector3.Cross(forward, right).normalized;

            // Create circle of vertices
            for (int j = 0; j < radialSegments; j++)
            {
                float angle = j * (2 * Mathf.PI / radialSegments);
                Vector3 offset = right * Mathf.Cos(angle) + up * Mathf.Sin(angle);
                vertices.Add(center + offset * fuseRadius);

                // UV coordinates based on actual path length
                uvs.Add(new Vector2((float)j / radialSegments, currentLength / accumulatedLength));
            }
        }

        // Create triangles
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            int baseIndex = i * radialSegments;
            for (int j = 0; j < radialSegments; j++)
            {
                int current = baseIndex + j;
                int next = baseIndex + (j + 1) % radialSegments;
                int nextRow = current + radialSegments;
                int nextRowNext = next + radialSegments;

                // First triangle
                triangles.Add(current);
                triangles.Add(nextRow);
                triangles.Add(next);

                // Second triangle
                triangles.Add(next);
                triangles.Add(nextRow);
                triangles.Add(nextRowNext);
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        meshFilter.mesh = mesh;
    }

    /// <summary>
    /// Handles the effect of the lighting.
    /// </summary>
    public void LightFuse()
    {
        if (!isLit)
        {
            isLit = true;
            meshRenderer.material = fuseMaterialActive;
            
            if (burningEffect != null)
            {
                burningEffect.gameObject.SetActive(true);
                burningEffect.Play();
            }
        }
    }

    private Vector3 CalculateBurningPosition(float progress)
    {
        float distanceToTravel = progress * totalLength;
        float distanceTraveled = 0f;

        if (!listHasBeenReversed && directionStart2End)
        {
            Array.Reverse(pathPoints);
            listHasBeenReversed = true;
            meshRenderer.material = fuseMaterialActiveInverted;
        }
        
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            float segmentLength = Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
            if (distanceTraveled + segmentLength >= distanceToTravel)
            {
                float t = (distanceToTravel - distanceTraveled) / segmentLength;
                return Vector3.Lerp(pathPoints[i], pathPoints[i + 1], t);
            }
            distanceTraveled += segmentLength;
        }
    
        return pathPoints[pathPoints.Length - 1];
    }

    
    private void Update()
    {
        if (!isLit) return;
    
        burnProgress += (burnSpeed * Time.deltaTime) / totalLength;
    
        // Calculate exact burning position
        Vector3 burningPosition = CalculateBurningPosition(burnProgress);
    
        // Update both effects with the same position data
        UpdateBurningEffect(burningPosition);
        UpdateFuseMaterial();

        // Check if we should trigger connected fuse
        if (!hasTriggeredNextFuse && burnProgress >= 1f)
        {
            hasTriggeredNextFuse = true;
            
            if (connectedEndpoint != null)
            {
                Debug.Log("TriggerConnectedFuse");
                // Trigger the connected fuse
                connectedEndpoint.TriggerConnectedFuse();
            } else if (targetBarrel != null)
            {
                Debug.Log("TriggerExplosion!");
                targetBarrel.ExplosionTrigger(null, true);
            }

            fuseBurnt.Value = true;
        }
    }

    private void UpdateFuseMaterial()
    {
        // Only update burn progress if using active material
        if (isLit && meshRenderer.material != null)
        {
            meshRenderer.material.SetFloat("_BurnProgress", burnProgress);
        }
    }
    
    private void UpdateBurningEffect(Vector3 burningPosition)
    {
        if (burningEffect == null || pathPoints == null || pathPoints.Length < 2) return;

        burningEffect.transform.position = burningPosition;

        // Find current segment for rotation
        int currentSegment = 0;
        float distanceTraveled = 0f;
        float targetDistance = burnProgress * totalLength;
    
        while (currentSegment < pathPoints.Length - 1)
        {
            float segmentLength = Vector3.Distance(pathPoints[currentSegment], pathPoints[currentSegment + 1]);
            if (distanceTraveled + segmentLength >= targetDistance)
            {
                Vector3 direction = (pathPoints[currentSegment + 1] - pathPoints[currentSegment]).normalized;
                burningEffect.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180, 0);
                break;
            }
            distanceTraveled += segmentLength;
            currentSegment++;
        }
    }

    private void OnDestroy()
    {
        // Clean up material instances
        if (meshRenderer != null && meshRenderer.material != null)
        {
            Destroy(meshRenderer.material);
        }
        if (fuseMaterialActive != null)
        {
            Destroy(fuseMaterialActive);
        }
        
        if (burningEffect != null)
        {
            Destroy(burningEffect.gameObject);
        }
    }
}