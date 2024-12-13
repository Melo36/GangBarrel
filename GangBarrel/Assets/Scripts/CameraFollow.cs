using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player; // The object the camera will initially follow
    [SerializeField] private float delay = 2f; // Time in seconds before the camera starts following
    [SerializeField] private float followSpeed = 2f; // Speed at which the camera follows the target
    private Vector3 initialOffset; // Initial distance from the player
    private bool canFollow = false; // Flag to enable following
    private Transform currentTarget; // Current target for the camera to follow

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
        if (canFollow && currentTarget != null)
        {
            // Target position is the current target's position plus the initial offset
            Vector3 targetPosition = currentTarget.position + initialOffset;

            // Gradually move the camera towards the target position
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
    }

    private void StartFollowing()
    {
        canFollow = true;
    }

    // Public method to change the camera's target
    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget;
        if (currentTarget != null)
        {
            initialOffset = transform.position - currentTarget.position;
        }
    }
}