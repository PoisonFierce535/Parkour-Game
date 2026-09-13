using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public InputActionAsset InputActions;

    private Camera mainCamera;
    private Rigidbody rb;

    private InputAction lookAction;

    private Vector2 lookInput;
    private float yaw = 0f;
    private float pitch = 0f;

    // EDITABLE //
    private const float CAMERA_SENSITIVITY = 0.125f;
    private const float MAX_VERTICAL_CAMERA_ANGLE = 90f;
    // EDITABLE //



    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }
    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        lookAction = InputActions.FindAction("Look");

        mainCamera = GetComponent<Camera>();
        rb = GameObject.Find("Player").GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;

        // set rotation values
        lookInput = lookAction.ReadValue<Vector2>();

        yaw += lookInput.x * CAMERA_SENSITIVITY;

        pitch -= lookInput.y * CAMERA_SENSITIVITY;
        pitch = Mathf.Clamp(pitch, -MAX_VERTICAL_CAMERA_ANGLE, MAX_VERTICAL_CAMERA_ANGLE);

        // set camera's position based on player
        mainCamera.transform.position = rb.transform.position;
    }

    private void FixedUpdate()
    {
        // set player's rotation based on camera
        rb.transform.localEulerAngles = new Vector3(0f, mainCamera.transform.localEulerAngles.y, 0f);
    }

    private void LateUpdate()
    {
        // rotate camera
        mainCamera.transform.localRotation = Quaternion.Euler(pitch, yaw, mainCamera.transform.eulerAngles.z);
    }
}
