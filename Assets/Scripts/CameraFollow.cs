using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;        // Reference to the player's transform
    public Vector3 offset;          // Offset to maintain between the player and camera
    public float smoothSpeed = 0.125f;  // How smooth the camera movement should be

    void LateUpdate()
    {
        // Define the target position for the camera
        Vector3 desiredPosition = player.position + offset;
        
        // Smoothly move the camera to the target position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // Apply the new position to the camera
        transform.position = smoothedPosition;
    }

    
}
