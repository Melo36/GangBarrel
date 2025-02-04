using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Movement speed of the camera
    public float cameraSpeed = 10f;

    // Vertical movement limits
    public float minY = 1f;
    public float maxY = 20f;

    void Update()
    {
        // Get input from WASD or arrow keys
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float verticalInput = Input.GetAxis("Vertical");     // W/S or Up/Down

        // Calculate horizontal and forward/backward movement
        Vector3 movement = new Vector3(horizontalInput, 0, verticalInput) * cameraSpeed * Time.deltaTime;

        // Apply movement to the camera's position
        transform.Translate(movement, Space.World);

        // Handle vertical movement (up and down)
        if (Input.GetAxis("Mouse ScrollWheel") > 0f ) // Move up
        {
            transform.position += new Vector3(0, cameraSpeed * Time.deltaTime * 10, 0);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f) // Move down
        {
            transform.position -= new Vector3(0, cameraSpeed * Time.deltaTime * 10, 0);
        }

        // Clamp the camera's vertical position
        Vector3 clampedPosition = transform.position;
        clampedPosition.y = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = clampedPosition;
    }
}