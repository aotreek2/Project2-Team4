using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Distance variables
    public float initialDistance = 20.0f; // Starting distance from the scene origin
    public float minDistance = 5f;        // Minimum zoom distance
    public float maxDistance = 20f;       // Maximum zoom distance
    private float currentDistance;        // Current distance from the pivot point
    private float desiredDistance;        // Desired distance after scrolling

    // Angle variables
    public float initialYAngle = 45f;     // Initial vertical angle
    public float minYAngle = 40f;         // Minimum vertical angle when fully zoomed in
    public float maxYAngle = 45f;         // Maximum vertical angle when zoomed out
    private float currentYAngle;          // Current vertical angle

    private float currentXAngle = 0f;     // Current horizontal angle (added for Q and E rotation)

    // Movement variables
    public float moveSpeed = 10f;         // Speed of camera movement (WASD and mouse drag)
    public float rotationSpeed = 100f;    // Speed of rotation when using Q and E keys
    public float zoomSpeed = 5f;          // Speed of zooming
    public float zoomDampening = 5f;      // Damping for zooming

    // Internal variables
    private Vector3 pivotPoint;           // The point the camera orbits around

    void Start()
    {
        // Initialize distances and angles
        currentDistance = desiredDistance = initialDistance;
        currentYAngle = initialYAngle;
        currentXAngle = 0f; // Start with no horizontal rotation

        // Set the initial pivot point (e.g., spaceship position or scene center)
        pivotPoint = Vector3.zero; // You can set this to your spaceship's position if needed

        // Position the camera
        UpdateCameraPosition();
    }

    void LateUpdate()
    {
        // Handle zoom input
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0.0f)
        {
            desiredDistance -= scrollInput * zoomSpeed;
            desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        }

        // Smoothly interpolate the camera's distance
        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);

        // Adjust camera angle slightly when fully zoomed in
        float angleLerp = Mathf.InverseLerp(maxDistance, minDistance, currentDistance);
        currentYAngle = Mathf.Lerp(maxYAngle, minYAngle, angleLerp);

        // Rotate the camera with Q and E keys
        if (Input.GetKey(KeyCode.Q))
        {
            currentXAngle -= rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E))
        {
            currentXAngle += rotationSpeed * Time.deltaTime;
        }

        // Camera movement with WASD keys
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (movement != Vector3.zero)
        {
            Vector3 moveDirection = Quaternion.Euler(0, currentXAngle, 0) * movement.normalized;
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
            pivotPoint += moveDirection * moveSpeed * Time.deltaTime;
        }

        // Move the camera when both right and left mouse buttons are held down
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
        {
            float h = -Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
            float v = -Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;
            Vector3 moveDirection = Quaternion.Euler(0, currentXAngle, 0) * new Vector3(h, 0, v);
            transform.position += moveDirection;
            pivotPoint += moveDirection;
        }

        // Update the camera's position and rotation
        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentYAngle, currentXAngle, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -currentDistance);
        Vector3 position = rotation * negDistance + pivotPoint;

        transform.rotation = rotation;
        transform.position = position;
    }
}
