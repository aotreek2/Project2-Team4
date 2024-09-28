using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;        // The object to orbit around
    public float distance = 20f;    // Default distance from the target
    public float zoomSpeed = 2f;    // Speed of zooming
    public float minDistance = 5f;  // Minimum zoom distance
    public float maxDistance = 50f; // Maximum zoom distance

    public float rotationSpeed = 100f; // Speed of rotation around the target

    private float currentX = 0f;    // Current rotation angle around the Y-axis
    private float currentY = 20f;   // Current rotation angle around the X-axis
    public float minYAngle = 10f;   // Minimum vertical angle
    public float maxYAngle = 80f;   // Maximum vertical angle

    public float smoothTime = 0.1f; // Smoothing time
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        // Optionally lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize currentX and currentY based on initial rotation
        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;
    }

    void Update()
    {
        // Mouse input
        currentX += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        currentY -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        // Clamp vertical angle
        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

        // Zoom with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed * Time.deltaTime * 1000f;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Optional: Reset camera position with a key press
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentX = 0f;
            currentY = 20f;
            distance = 20f;
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // Calculate rotation and position
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 direction = new Vector3(0, 0, -distance);
        Vector3 targetPosition = rotation * direction + target.position;

        // Smoothly move the camera
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.LookAt(target.position);
    }
}
