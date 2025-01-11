using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player; // The object the camera will initially follow
    [SerializeField] private float delay = 2f; // Time in seconds before the camera starts following
    [SerializeField] private float followSpeed = 2f; // Speed at which the camera follows the target
    [SerializeField] private float targetReachedTolerance = 0.1f;
    
    private Vector3 initialOffset; // Initial distance from the player
    private bool canFollow = false; // Flag to enable following
    private Transform currentTarget; // Current target for the camera to follow

    public bool ReachedTarget { get; private set; }
    
    void Start()
    {
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
        ReachedTarget = distanceToTarget <= targetReachedTolerance;

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
        if (currentTarget != null)
        {
            initialOffset = transform.position - currentTarget.position;
        }
        currentTarget = newTarget;
    }
}