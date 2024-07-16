using UnityEngine;

public class LookAtCameraKen : MonoBehaviour
{
    public float rotationAngle = 0f; // Angle to rotate the text object around the y-axis
    private void LateUpdate()
    {
        // Get the position of the main camera
        Vector3 cameraPosition = Camera.main.transform.position;

        // Rotate the text to face the camera
        transform.LookAt(cameraPosition);
        transform.Rotate(Vector3.up, rotationAngle); // Rotate the text around the y-axis by the specified angle
    }
}
