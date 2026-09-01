using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private MainUIManager mainUIManager;

    private Camera mainCamera;

    private float wallTiltAngleZ = 15f;
    private float wallTiltSpeedZ = 2.5f;
    private float wallTargetAngleZ;

    private float slideTiltAngle = 5f;
    private float slideTiltSpeed = 10f;
    private float slideTargetAngle;



    private void Start()
    {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
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
            float newLean = Mathf.LerpAngle(euler.z, wallTargetAngleZ, wallTiltSpeedZ * Time.deltaTime);
            mainCamera.transform.eulerAngles = new Vector3(euler.x, euler.y, newLean);
        }
        else if (wallTargetAngleZ == 0f) // slide
        {
            float newLean = Mathf.LerpAngle(euler.z, slideTargetAngle, slideTiltSpeed * Time.deltaTime);
            mainCamera.transform.eulerAngles = new Vector3(euler.x, euler.y, newLean);
        }
    }

    // FUNCTIONS //
    private void SetCameraEffects()
    {
        if (mainUIManager.wallDir == "Left") wallTargetAngleZ = -wallTiltAngleZ;
        else if (mainUIManager.wallDir == "Right") wallTargetAngleZ = wallTiltAngleZ;
        else wallTargetAngleZ = 0f;

        if (playerMovement.isCrouched && playerMovement.rb.linearVelocity.magnitude > 6f) slideTargetAngle = slideTiltAngle;
        else slideTargetAngle = 0f;
    }
}
