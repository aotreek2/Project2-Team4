using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public Transform target;        // The object to orbit around (the ship)
    public float distance = 20f;    // Default distance from the target
    public float zoomSpeed = 2f;    // Speed of zooming
    public float minDistance = 5f;  // Minimum zoom distance
    public float maxDistance = 50f; // Maximum zoom distance

    public float rotationSpeed = 100f; // Speed of rotation around the target
    public float moveSpeed = 10f;      // Speed of camera movement with WASD

    private float currentX = 0f;    // Current rotation angle around the Y-axis
    private float currentY = 20f;   // Current rotation angle around the X-axis
    public float minYAngle = 10f;   // Minimum vertical angle
    public float maxYAngle = 80f;   // Maximum vertical angle

    public float smoothTime = 0.1f; // Smoothing time
    private Vector3 velocity = Vector3.zero;

    private Vector3 desiredPosition;

    void Start()
    {
        // Unlock and show the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Initialize currentX and currentY based on initial rotation
        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;

        // Initialize desiredPosition
        UpdateCameraPosition();
    }

    void Update()
    {
        // Prevent camera control when pointer is over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Mouse input for rotation
        if (Input.GetMouseButton(1)) // Right mouse button held down
        {
            currentX += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            currentY -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            // Clamp vertical angle
            currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);
        }

        // Zoom with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed * Time.deltaTime * 1000f;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Camera movement with WASD keys
        Vector3 moveDirection = new Vector3();
        if (Input.GetKey(KeyCode.W))
            moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.S))
            moveDirection -= transform.forward;
        if (Input.GetKey(KeyCode.A))
            moveDirection -= transform.right;
        if (Input.GetKey(KeyCode.D))
            moveDirection += transform.right;

        // Move the camera position
        if (moveDirection != Vector3.zero)
        {
            transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
        }

        // Optional: Reset camera position with a key press
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentX = 0f;
            currentY = 20f;
            distance = 20f;
            transform.position = target.position - transform.forward * distance;
        }

        // Update desired position
        UpdateCameraPosition();
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // Smoothly move the camera
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        transform.LookAt(target.position);
    }

    void UpdateCameraPosition()
    {
        // Calculate rotation and position
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 direction = new Vector3(0, 0, -distance);
        desiredPosition = target.position + rotation * direction;
    }
}
