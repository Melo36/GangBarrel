using UnityEngine;

public class Blinking : MonoBehaviour
{
    private Renderer objectRenderer;
    private float nextBlinkTime;
    [SerializeField] private float blinkInterval = 0.25f;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        nextBlinkTime = Time.time + blinkInterval;
    }

    void Update()
    {
        if (Time.time >= nextBlinkTime)
        {
            objectRenderer.enabled = !objectRenderer.enabled;
            nextBlinkTime = Time.time + blinkInterval;
        }
    }

    void OnDestroy()
    {
        if (objectRenderer != null)
        {
            objectRenderer.enabled = true;
        }
    }
}