using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotateAround : MonoBehaviour
{
    public Transform target; // Assign the spaceship or object to focus on
    public float rotationSpeed = 100f;
    public float zoomSpeed = 20f;
    public float minDistance = 10f;
    public float maxDistance = 50f;

    private Vector3 offset;

    void Start()
    {
        // Set the initial offset based on the camera's starting position
        offset = transform.position - target.position;
    }

    void Update()
    {
        // Rotate around the target using Q and E keys for horizontal rotation
        float horizontalInput = 0;

        if (Input.GetKey(KeyCode.Q)) // Rotate left
        {
            horizontalInput = -rotationSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.E)) // Rotate right
        {
            horizontalInput = rotationSpeed * Time.deltaTime;
        }

        if (horizontalInput != 0)
        {
            transform.RotateAround(target.position, Vector3.up, horizontalInput);
        }

        // Zoom in and out
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        offset = offset.normalized * (offset.magnitude - scroll * zoomSpeed * Time.deltaTime);
        offset = Vector3.ClampMagnitude(offset, maxDistance);
        offset = offset.normalized * Mathf.Clamp(offset.magnitude, minDistance, maxDistance);

        // Reposition the camera based on the updated offset
        transform.position = target.position + offset;

        // Keep the camera looking at the target
        transform.LookAt(target);
    }
}
