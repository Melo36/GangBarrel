using UnityEngine;

public class Fuse : MonoBehaviour
{
    [Header("References")]
    public LineRenderer lineRenderer;
    public ParticleSystem burningEffect;
    public Transform startPoint;
    public Transform endPoint;

    [Header("Settings")]
    public float burnSpeed = 1f; // Units per second
    public bool isLit = false;
    public bool canBeInteractedWith = true;

    private float burnProgress = 0f;
    private float totalDistance;
    private Vector3[] pathPoints;

    public event System.Action OnFuseComplete;

    private void Start()
    {
        SetupLineRenderer();
        burningEffect.Stop();
    }

    public void Initialize(Vector3[] path, Transform target)
    {
        pathPoints = path;
        endPoint = target;
        
        // Setup line renderer with path points
        lineRenderer.positionCount = pathPoints.Length;
        lineRenderer.SetPositions(pathPoints);
        
        // Calculate total distance for burn duration
        totalDistance = CalculatePathLength();
        
        // Position particle system at start
        if (burningEffect != null)
        {
            burningEffect.transform.position = pathPoints[0];
        }
    }

    private void Update()
    {
        if (isLit)
        {
            UpdateBurning();
        }
    }

    public void LightFuse()
    {
        if (!isLit)
        {
            isLit = true;
            burningEffect.Play();
        }
    }

    private void UpdateBurning()
    {
        burnProgress += (burnSpeed * Time.deltaTime) / totalDistance;
        burnProgress = Mathf.Clamp01(burnProgress);

        // Update particle effect position
        Vector3 currentPosition = GetPositionAlongPath(burnProgress);
        burningEffect.transform.position = currentPosition;

        if (burnProgress >= 1f)
        {
            OnFuseComplete?.Invoke();
            Destroy(gameObject);
        }
    }

    private Vector3 GetPositionAlongPath(float progress)
    {
        float distance = progress * totalDistance;
        float currentDistance = 0f;

        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            float segmentLength = Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
            if (currentDistance + segmentLength >= distance)
            {
                float t = (distance - currentDistance) / segmentLength;
                return Vector3.Lerp(pathPoints[i], pathPoints[i + 1], t);
            }
            currentDistance += segmentLength;
        }

        return pathPoints[pathPoints.Length - 1];
    }

    private float CalculatePathLength()
    {
        float length = 0f;
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            length += Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
        }
        return length;
    }

    private void SetupLineRenderer()
    {
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
    }
}