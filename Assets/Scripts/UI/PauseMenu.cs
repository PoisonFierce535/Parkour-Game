using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;

    public InputActionAsset InputActions;
    private InputAction pauseMenuAction;



    // Enable InputSystem
    private void OnEnable()
    {
        InputActions.FindActionMap("UI").Enable();
    }
    private void OnDisable()
    {
        InputActions.FindActionMap("UI").Disable();
    }

    void Start()
    {
        pauseMenuAction = InputActions.FindAction("PauseMenu");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (pauseMenuAction.WasPressedThisFrame() && pauseMenuUI.activeSelf == false)
        {
            PauseGame();
        }
        else if (pauseMenuAction.WasPressedThisFrame() && pauseMenuUI.activeSelf == true)
        {
            ResumeGame();
        }
    }


    // FUNCTIONS //
    public void PauseGame()
    {
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }
    public void ToMenu()
    {
        SceneManager.LoadScene("_MainMenu");
    }
}
