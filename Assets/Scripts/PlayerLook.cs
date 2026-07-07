using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    public InputActionAsset InputActions;
    public GameObject cameraHolder;

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

        lookAction = InputActions.FindAction("Look");
    }

    private void Update()
    {
        lookInput = lookAction.ReadValue<Vector2>();

        yaw += lookInput.x * CAMERA_SENSITIVITY;

        pitch -= lookInput.y * CAMERA_SENSITIVITY;
        pitch = Mathf.Clamp(pitch, -MAX_VERTICAL_CAMERA_ANGLE, MAX_VERTICAL_CAMERA_ANGLE);
    }

    private void LateUpdate()
    {
        RotateCamera();
    }

    void RotateCamera()
    {
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraHolder.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}