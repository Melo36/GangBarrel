using System.Collections.Generic;
using UnityEngine;

public class Fuse : MonoBehaviour

{
    [Header("References")] [SerializeField]
    private ParticleSystem burningEffect;


    [Header("Settings")] [SerializeField] private float burnSpeed = 1f;

    [SerializeField] private float fuseRadius = 0.025f;

    [SerializeField] private int radialSegments = 8;


    [Header("Visual Settings")] [SerializeField]
    private Material fuseMaterial;


    private Vector3[] pathPoints;

    private float totalLength;

    private float burnProgress;

    private bool isLit;

    private BarrelExplosionController targetBarrel;

    private MeshFilter meshFilter;

    private MeshRenderer meshRenderer;


    private void Awake()

    {
        meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshRenderer.material = fuseMaterial;
    }


    public void Initialize(Vector3[] points, BarrelExplosionController barrel)

    {
        pathPoints = points;

        targetBarrel = barrel;


        CreateFuseMesh();


        // Calculate total length for burn duration

        totalLength = 0f;

        for (int i = 1; i < points.Length; i++)

        {
            totalLength += Vector3.Distance(points[i - 1], points[i]);
        }


        burnProgress = 0f;

        isLit = false;


        if (burningEffect != null)

        {
            burningEffect.transform.position = points[0];

            burningEffect.Stop();
        }
    }


    private void CreateFuseMesh()

    {
        List<Vector3> vertices = new List<Vector3>();

        List<int> triangles = new List<int>();

        List<Vector2> uvs = new List<Vector2>();


        // Create vertices around the path

        for (int i = 0; i < pathPoints.Length; i++)

        {
            Vector3 center = pathPoints[i];

            Vector3 forward = i < pathPoints.Length - 1
                ? (pathPoints[i + 1] - center).normalized
                : (center - pathPoints[i - 1]).normalized;

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            Vector3 up = Vector3.Cross(forward, right).normalized;


            // Create circle of vertices

            for (int j = 0; j < radialSegments; j++)

            {
                float angle = j * (2 * Mathf.PI / radialSegments);

                Vector3 offset = right * Mathf.Cos(angle) + up * Mathf.Sin(angle);

                vertices.Add(center + offset * fuseRadius);


                // UV coordinates

                uvs.Add(new Vector2((float)j / radialSegments, (float)i / (pathPoints.Length - 1)));
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

        UpdateFuseMaterial();


        if (burnProgress >= 1f)

        {
            if (targetBarrel != null)

            {
                targetBarrel.ExplosionTrigger(null, true);
            }

            Destroy(gameObject);
        }
    }


    private void UpdateFuseMaterial()

    {
        // Update the material's burn progress property

        fuseMaterial.SetFloat("_BurnProgress", burnProgress);
    }


    private void UpdateBurningEffect()

    {
        if (burningEffect == null || pathPoints == null || pathPoints.Length < 2) return;


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