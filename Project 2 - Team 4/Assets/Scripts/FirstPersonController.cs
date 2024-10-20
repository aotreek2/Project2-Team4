using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public Transform cameraTransform;
    public float walkSpeed = 5f;
    public float mouseSensitivity = 2f;

    private float verticalRotation = 0f;
    private CharacterController characterController;
    private float verticalVelocity = 0f;
    public float gravity = -9.81f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Disable the CharacterController and this script by default
        characterController.enabled = false;
        this.enabled = false;
    }

    void OnEnable()
    {
        if (characterController != null)
            characterController.enabled = true;

        // Lock the cursor when entering first-person mode
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize vertical rotation based on current camera rotation
        verticalRotation = 0f; // Start at zero
    }

    void OnDisable()
    {
        if (characterController != null)
            characterController.enabled = false;

        // Unlock the cursor when exiting first-person mode
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            Debug.Log("Camera Local Position: " + cameraTransform.localPosition);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        cameraTransform.localPosition = new Vector3(0, 0, 0);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // Movement
        float moveForward = Input.GetAxis("Vertical");
        float moveSideways = Input.GetAxis("Horizontal");

        Vector3 movement = transform.forward * moveForward + transform.right * moveSideways;
        movement *= walkSpeed;

        // Apply gravity
        if (characterController.isGrounded)
        {
            verticalVelocity = 0f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        movement.y = verticalVelocity;

        // Apply movement using Character Controller
        characterController.Move(movement * Time.deltaTime);

        // Exit first-person mode
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Reference your CameraController script and call the ExitFirstPersonMode method
            FindObjectOfType<CameraController>().ExitFirstPersonMode();
        }
    }
    
}
