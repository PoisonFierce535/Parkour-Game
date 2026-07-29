using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelDoorScript : MonoBehaviour
{
    private int noLevelNext = 5;

    private void OnTriggerEnter(Collider other)
    {
        int nextBuildIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (other.gameObject.CompareTag("Player"))
        {
            if (nextBuildIndex == noLevelNext)
            {
                Cursor.lockState = CursorLockMode.None;
                SceneManager.LoadScene("_MainMenu");
            }
            else
            {
                SceneManager.LoadScene(nextBuildIndex);
            }
        }
        
    }
}
