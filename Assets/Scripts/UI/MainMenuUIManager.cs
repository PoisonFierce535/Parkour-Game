using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIManager : MonoBehaviour
{
    public GameObject mainUI;
    public GameObject levelsUI;



    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // FUNCTIONS //
    // MainUI
    public void PlayGame() { mainUI.SetActive(false); levelsUI.SetActive(true); }
    public void QuitGame()
    {
        Application.Quit();
    }

    // LevelsUI
    public void EnterLevel(string levelName) => SceneManager.LoadScene(levelName);
    public void GoBack() { mainUI.SetActive(true); levelsUI.SetActive(false); }
}
