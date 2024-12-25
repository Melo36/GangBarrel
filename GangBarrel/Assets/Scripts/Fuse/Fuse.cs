using UnityEngine;
using System.Collections.Generic;
using Pathfinding;

public class Fuse : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private ParticleSystem burningEffect;
    
    [Header("Settings")]
    [SerializeField] private float burnSpeed = 1f; // Units per second
    
    [Header("Visual Settings")]
    [SerializeField] private Color unburntColor = Color.black;
    [SerializeField] private Color burntColor = Color.red;
    
    private Vector3[] pathPoints;
    private float totalLength;
    private float burnProgress;
    private bool isLit;
    private BarrelExplosionController targetBarrel;

    private void Awake()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();
        if (burningEffect == null)
            burningEffect = GetComponentInChildren<ParticleSystem>();

        // Set up line renderer
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
    }

    public void Initialize(Vector3[] points, BarrelExplosionController barrel)
    {
        pathPoints = points;
        targetBarrel = barrel;
        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
        
        // Calculate total length for burn duration
        totalLength = 0f;
        for (int i = 1; i < points.Length; i++)
        {
            totalLength += Vector3.Distance(points[i - 1], points[i]);
        }

        burnProgress = 0f;
        isLit = false;

        // Set initial color
        lineRenderer.startColor = unburntColor;
        lineRenderer.endColor = unburntColor;

        // Set initial burning effect position
        if (burningEffect != null)
        {
            burningEffect.transform.position = points[0];
            burningEffect.Stop();
        }
    }

    public void LightFuse()
    {
        if (!isLit)
        {
            isLit = true;
            if (burningEffect != null)
            {
                burningEffect.Play();
            }
        }
    }

    private void Update()
    {
        if (!isLit) return;

        burnProgress += (burnSpeed * Time.deltaTime) / totalLength;
        UpdateBurningEffect();
        UpdateLineRendererColors();

        if (burnProgress >= 1f)
        {
            if (targetBarrel != null)
            {
                targetBarrel.ExplosionTrigger(null, true);
            }
            Destroy(gameObject);
        }
    }

    private void UpdateLineRendererColors()
    {
        // Find the current segment
        float currentDistance = burnProgress * totalLength;
        float coveredDistance = 0f;
        
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            float segmentLength = Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
            coveredDistance += segmentLength;
            
            // If this segment is before the burn point, make it burnt
            if (coveredDistance <= currentDistance)
            {
                lineRenderer.SetColors(burntColor, burntColor);
            }
            // If this segment is after the burn point, make it unburnt
            else
            {
                lineRenderer.SetColors(unburntColor, unburntColor);
            }
        }
    }

    private void UpdateBurningEffect()
    {
        if (burningEffect == null || pathPoints == null || pathPoints.Length < 2) return;

        // Calculate position along the path
        float distanceToTravel = burnProgress * totalLength;
        float distanceTraveled = 0f;
        int currentSegment = 0;

        while (currentSegment < pathPoints.Length - 1)
        {
            float segmentLength = Vector3.Distance(pathPoints[currentSegment], pathPoints[currentSegment + 1]);
            if (distanceTraveled + segmentLength >= distanceToTravel)
            {
                float t = (distanceToTravel - distanceTraveled) / segmentLength;
                Vector3 position = Vector3.Lerp(pathPoints[currentSegment], pathPoints[currentSegment + 1], t);
                burningEffect.transform.position = position;
                break;
            }
            distanceTraveled += segmentLength;
            currentSegment++;
        }
    }

    private void OnDestroy()
    {
        if (burningEffect != null)
        {
            Destroy(burningEffect.gameObject);
        }
    }
}