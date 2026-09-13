using NUnit.Framework.Internal;
using UnityEngine;

public class MainUIManager : MonoBehaviour
{
    private PlayerMovement playerMovement;

    private Camera mainCamera;

    public string wallDir;

    public GameObject wallLeft;
    public GameObject wallRight;
    public GameObject wallUp;
    public GameObject wallDown;

    private bool disableVisualsDebounce;



    private void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        playerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();

        disableVisualsDebounce = true;
    }

    private void Update()
    {
        if (playerMovement.isWallrunning)
        {
            disableVisualsDebounce = true;
            wallDir = GetScreenDirection(playerMovement.currentWallSide, mainCamera);
            SetVisuals();
        }
        else if (disableVisualsDebounce)
        {
            disableVisualsDebounce = false;
            wallDir = string.Empty;
            DisableVisuals();
        }
    }

    // FUNCTIONS //
    public static string GetScreenDirection(Vector3 worldNormal, Camera camera)
    {
        Vector3 camRight = camera.transform.right;
        Vector3 camForward = camera.transform.forward;

        float x = Vector3.Dot(worldNormal, camRight);
        float y = Vector3.Dot(worldNormal, camForward);

        if (Mathf.Abs(x) > Mathf.Abs(y))
            return x < 0 ? "Right" : "Left";
        else
            return y < 0 ? "Up" : "Down";
    }

    private void SetVisuals()
    {
        wallLeft.SetActive(wallDir == "Left");
        wallRight.SetActive(wallDir == "Right");
        wallUp.SetActive(wallDir == "Up");
        wallDown.SetActive(wallDir == "Down");
    }

    private void DisableVisuals()
    {
        wallLeft.SetActive(false);
        wallRight.SetActive(false);
        wallUp.SetActive(false);
        wallDown.SetActive(false);
    }
}