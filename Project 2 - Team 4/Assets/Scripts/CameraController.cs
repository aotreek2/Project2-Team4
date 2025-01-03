// CameraController.cs
using UnityEngine;
using System.Collections;
using TMPro;

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

    private float currentXAngle = 0f;     // Current horizontal angle

    // Movement variables
    public float moveSpeed = 10f;         // Speed of camera movement (WASD and mouse drag)
    public float rotationSpeed = 100f;    // Speed of rotation when using Q and E keys
    public float zoomSpeed = 10f;         // Zoom speed
    public float zoomDampening = 10f;     // Zoom damping

    // Internal variables
    private Vector3 pivotPoint;           // The point the camera orbits around

    // First-person mode variables
    public bool isInFirstPerson = false;
    private CrewMember controlledCrewMember;
    private Transform mainCameraTransform;

    // Store original camera position and parent
    private Transform originalCameraParent;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    // Transition variables
    public float transitionDuration = 1f; // Duration of the camera transition

    // TextMeshPro UI for instructions
    public TextMeshProUGUI instructionText; // Assign this in the Inspector

    void Start()
    {
        mainCameraTransform = Camera.main.transform;

        // Parent the main camera to the CameraController
        mainCameraTransform.SetParent(transform);
        mainCameraTransform.localPosition = Vector3.zero;
        mainCameraTransform.localRotation = Quaternion.identity;

        // Initialize distances and angles
        currentDistance = desiredDistance = initialDistance;
        currentYAngle = initialYAngle;
        currentXAngle = 0f;

        // Set the initial pivot point (e.g., spaceship position or scene center)
        pivotPoint = Vector3.zero;

        // Position the camera
        UpdateCameraPosition();

        // Hide instruction text initially
        if (instructionText != null)
        {
            instructionText.enabled = false;
        }
    }

    void LateUpdate()
    {
        // Always handle zoom input
        HandleZoomInput();

        if (isInFirstPerson)
        {
            // In first-person mode, ensure third-person camera updates are skipped
            return;
        }

        // Proceed with camera movement and updates only if not in first-person mode
        HandleCameraMovement();
        UpdateCameraPosition();
        HandleCrewMemberSelection();
    }

    void HandleZoomInput()
    {
        if (isInFirstPerson)
            return; // Do not process zoom input in first-person mode

        // Handle zoom input
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0.0f)
        {
            desiredDistance -= scrollInput * zoomSpeed;
            desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        }

        // Smoothly interpolate the camera's distance
        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);
    }

    void HandleCameraMovement()
    {
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
    }

    void UpdateCameraPosition()
    {
        if (isInFirstPerson)
            return; // Skip updating camera position in first-person mode

        Quaternion rotation = Quaternion.Euler(currentYAngle, currentXAngle, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -currentDistance);
        Vector3 position = rotation * negDistance;

        // Apply rotation and position to the CameraController transform
        transform.rotation = rotation;
        transform.position = pivotPoint;

        // Update the camera's local position (relative to the CameraController)
        mainCameraTransform.localPosition = position;
        mainCameraTransform.localRotation = Quaternion.identity;
    }

    void HandleCrewMemberSelection()
    {
        if (isInFirstPerson)
            return; // Skip crew member selection in first-person mode

        bool canEnterFirstPerson = false;

        if (desiredDistance <= minDistance + 0.5f)
        {
            Ray ray = mainCameraTransform.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                CrewMember crewMember = hit.collider.GetComponentInParent<CrewMember>();
                if (crewMember != null)
                {
                    // Show instruction text
                    if (instructionText != null)
                    {
                        instructionText.enabled = true;
                        instructionText.text = "Press 'F' to enter first-person view.";
                    }
                    canEnterFirstPerson = true;

                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        EnterFirstPersonMode(crewMember);
                    }
                    return;
                }
            }
        }

        // Hide instruction text and reset flag
        if (instructionText != null)
        {
            instructionText.enabled = false;
        }
        canEnterFirstPerson = false;
    }

    public void EnterFirstPersonMode(CrewMember crewMember)
    {
        if (isInFirstPerson) return; // Already in first-person mode

        isInFirstPerson = true;
        controlledCrewMember = crewMember;

        // Store the camera's original parent, position, and rotation
        originalCameraParent = mainCameraTransform.parent;
        originalCameraPosition = mainCameraTransform.position;
        originalCameraRotation = mainCameraTransform.rotation;

        // Parent the main camera to the crew member's CameraPosition transform
        Transform cameraPosition = crewMember.fpsCameraPos;
        if (cameraPosition != null)
        {
            // Ensure the CameraPosition local transform remains intact
            cameraPosition.localPosition = Vector3.zero;
            cameraPosition.localRotation = Quaternion.identity;

            // Set the camera's position and rotation before parenting to prevent sudden jumps
            mainCameraTransform.SetParent(cameraPosition);
            mainCameraTransform.localPosition = Vector3.zero;
            mainCameraTransform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogError("CrewMember does not have a 'CameraPosition' transform. Please add one to the crewmate's hierarchy.");
            isInFirstPerson = false;
            return;
        }

        // Disable the camera controller movement and rotation to prevent interference
        enabled = false;

        // Enable the FirstPersonController script
        FirstPersonController fpController = controlledCrewMember.GetComponent<FirstPersonController>();
        if (fpController != null)
        {
            fpController.enabled = true;
            fpController.cameraTransform = mainCameraTransform; // Set the camera transform
        }

        // Disable NavMeshAgent and AI scripts
        controlledCrewMember.DisableAI();
    }

    public void ExitFirstPersonMode()
    {
        if (!isInFirstPerson) return;

        // Disable the FirstPersonController script
        FirstPersonController fpController = controlledCrewMember.GetComponent<FirstPersonController>();
        if (fpController != null)
        {
            fpController.enabled = false;
        }

        // Unparent the camera
        mainCameraTransform.SetParent(null);

        // Restore the camera's tag to "MainCamera"
        mainCameraTransform.tag = "MainCamera";

        // Reset desiredDistance and currentDistance
        desiredDistance = initialDistance;
        currentDistance = desiredDistance;

        // Start smooth transition back to original position and rotation
        StartCoroutine(SmoothTransitionToThirdPerson());

        // Re-enable NavMeshAgent and AI scripts
        controlledCrewMember.EnableAI();

        controlledCrewMember = null;
        isInFirstPerson = false;
    }

    IEnumerator SmoothTransitionToThirdPerson()
    {
        float elapsedTime = 0f;

        Vector3 startingPosition = mainCameraTransform.position;
        Quaternion startingRotation = mainCameraTransform.rotation;

        // Calculate the target position and rotation based on current camera controller settings
        Quaternion rotation = Quaternion.Euler(currentYAngle, currentXAngle, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -desiredDistance);
        Vector3 targetPosition = transform.position + rotation * negDistance;
        Quaternion targetRotation = rotation;

        // Smoothly interpolate the camera's position and rotation
        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration;
            mainCameraTransform.position = Vector3.Lerp(startingPosition, targetPosition, t);
            mainCameraTransform.rotation = Quaternion.Slerp(startingRotation, targetRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the camera reaches the exact target position and rotation
        mainCameraTransform.position = targetPosition;
        mainCameraTransform.rotation = targetRotation;

        // Parent the camera back to the CameraController
        mainCameraTransform.SetParent(transform);

        // Restore the camera's tag to "MainCamera"
        mainCameraTransform.tag = "MainCamera";

        // Re-enable camera controller
        enabled = true;
    }

    // Updated DoFirstPersonCameraShake method with improved shake
    private IEnumerator DoFirstPersonCameraShake(float duration, float magnitude)
    {
        Transform cameraTransform = mainCameraTransform;
        Vector3 originalLocalPosition = cameraTransform.localPosition;

        float elapsed = 0.0f;

        // Loop over the shake duration
        while (elapsed < duration)
        {
            // Reduce intensity over time for a damping effect
            float currentMagnitude = Mathf.Lerp(magnitude, 0, elapsed / duration);

            // Use Perlin noise for smooth transitions between shake points
            float x = (Mathf.PerlinNoise(Time.time * 2, 0) - 0.5f) * currentMagnitude;
            float y = (Mathf.PerlinNoise(0, Time.time * 2) - 0.5f) * currentMagnitude;
            float z = (Mathf.PerlinNoise(Time.time * 2, Time.time * 2) - 0.5f) * currentMagnitude * 0.2f;

            // Apply shake to the camera
            cameraTransform.localPosition = originalLocalPosition + new Vector3(x, y, z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset camera to its original position
        cameraTransform.localPosition = originalLocalPosition;
    }

    // Updated DoCameraShake method
    private IEnumerator DoCameraShake(float duration, float magnitude)
    {
        Transform cameraTransform = mainCameraTransform;

        Vector3 originalPosition = cameraTransform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            cameraTransform.localPosition = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraTransform.localPosition = originalPosition;
    }

    // Updated ShakeCamera method
    public void ShakeCamera(float duration, float magnitude)
    {
        if (isInFirstPerson)
        {
            StartCoroutine(DoFirstPersonCameraShake(duration, magnitude));
        }
        else
        {
            StartCoroutine(DoCameraShake(duration, magnitude));
        }
    }
}
