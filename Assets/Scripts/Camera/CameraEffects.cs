using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraEffects : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private MainUIManager mainUIManager;

    private Camera mainCamera;

    private float wallTargetAngle;
    private float slideTargetAngle;

    // EDITABLE//
    private const float WALL_TILT_ANGLE = 15f;
    private const float WALL_TILT_SPEED = 2.5f;
    private const float SLIDE_TILT_ANGLE = 5f;
    private const float SLIDE_TILT_SPEED = 10f;
    // EDITABLE // 



    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        playerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();
        mainUIManager = GameObject.Find("UI").GetComponent<MainUIManager>();
    }

    private void Update()
    {
        // camera angle effects
        Vector3 euler = mainCamera.transform.eulerAngles;

        SetCameraEffects();

        if (slideTargetAngle == 0f) // wall Z
        {
            float newLean = Mathf.LerpAngle(euler.z, wallTargetAngle, WALL_TILT_SPEED * Time.deltaTime);
            mainCamera.transform.eulerAngles = new Vector3(euler.x, euler.y, newLean);
        }
        else if (wallTargetAngle == 0f) // slide
        {
            float newLean = Mathf.LerpAngle(euler.z, slideTargetAngle, SLIDE_TILT_SPEED * Time.deltaTime);
            mainCamera.transform.eulerAngles = new Vector3(euler.x, euler.y, newLean);
        }
    }

    // FUNCTIONS //
    private void SetCameraEffects()
    {
        if (mainUIManager.wallDir == "Left") wallTargetAngle = -WALL_TILT_ANGLE;
        else if (mainUIManager.wallDir == "Right") wallTargetAngle = WALL_TILT_ANGLE;
        else wallTargetAngle = 0f;

        if (playerMovement.isCrouched && playerMovement.rb.linearVelocity.magnitude > 6f) slideTargetAngle = SLIDE_TILT_ANGLE;
        else slideTargetAngle = 0f;
    }
}
