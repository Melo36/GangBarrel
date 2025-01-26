using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player; // The object the camera will initially follow
    [SerializeField] private float delay = 2f; // Time in seconds before the camera starts following
    [SerializeField] private float followSpeed = 2f; // Speed at which the camera follows the target
    [SerializeField] private float targetReachedTolerance = 0.1f;
    
    public Vector3 initialOffset; // Initial distance from the player or enemy
    private bool canFollow = false; // Flag to enable following
    public Transform currentTarget; // Current target for the camera to follow

    public bool targetReached;

    private void Awake()
    {
        player = FindObjectOfType<PlayerController>().gameObject.transform;
    }

    void Start()
    {
        // Place camera behind player automatically
        transform.position = player.position + new Vector3(0.33f, 4.9f, 0.889f);
        transform.rotation = Quaternion.Euler(65, -45, 0);
        
        // Calculate the initial offset between the camera and the player
        if (player != null)
        {
            initialOffset = transform.position - player.position;
            currentTarget = player; // Start following the player
        }
        else
        {
            Debug.LogError("Player transform is not assigned.");
        }

        // Start following after the specified delay
        Invoke(nameof(StartFollowing), delay);
    }

    void LateUpdate()
    {
        if (!canFollow || currentTarget == null) return;

        Vector3 targetPosition = currentTarget.position + initialOffset;
        Vector3 currentPosition = transform.position;
        
        // Calculate distance to target
        float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);
        
        // Update reached target status
        targetReached = distanceToTarget <= targetReachedTolerance;

        // Gradually move the camera towards the target position
        transform.position = Vector3.Lerp(currentPosition, targetPosition, followSpeed * Time.deltaTime);
    }


    private void StartFollowing()
    {
        canFollow = true;
    }

    // Public method to change the camera's target
    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget;
    }
}