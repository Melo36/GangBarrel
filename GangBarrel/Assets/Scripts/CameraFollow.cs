using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player; // The object the camera will follow
    [SerializeField] private float delay = 2f; // Time in seconds before the camera starts following
    [SerializeField] private float followSpeed = 2f; // Speed at which the camera follows the player
    private Vector3 initialOffset; // Initial distance from the player
    private bool canFollow = false; // Flag to enable following

    void Start()
    {
        // Calculate the initial offset between the camera and the player
        if (player != null)
        {
            initialOffset = transform.position - player.position;
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
        if (canFollow && player != null)
        {
            // Target position is the player's position plus the initial offset
            Vector3 targetPosition = player.position + initialOffset;

            // Gradually move the camera towards the target position
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
    }

    private void StartFollowing()
    {
        canFollow = true;
    }
}