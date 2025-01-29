using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The player or object the camera follows
    public float distance = 5.0f; // Distance from the target
    public float rotationSpeed = 3.0f; // Speed of rotation
    public Vector2 pitchLimits = new Vector2(-30, 60); // Limits for vertical rotation
    
    private float yaw = 0.0f;
    private float pitch = 20.0f;
    
    public Transform currentTarget; // Current target for the camera to follow

    void Start()
    {
        if (target != null)
        {
            Vector3 angles = transform.eulerAngles;
            yaw = angles.y;
            pitch = angles.x;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Rotate camera with right mouse button
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
            pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);
        }

        // Calculate new position and rotation
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        transform.position = target.position + offset;
        transform.LookAt(target.position);
    }

    // Public method to change the camera's target
    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget;
    }
}