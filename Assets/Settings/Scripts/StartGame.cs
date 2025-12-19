using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void StartGameOnCLick()
    {
        // Load the next scene by name OR index
        SceneManager.LoadScene("SampleScene");
    }

    public void StartTutorialOnCLick()
    {
        // Load the next scene by name OR index
        SceneManager.LoadScene("TutorialScene");
    }

    public void MainMenuOnClick()
    {
        // Load the next scene by name OR index
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitOnClick()
    {
        Application.Quit();
    }

    public void RestartTutorial()
    {
        // Load the next scene by name OR index
        SceneManager.LoadScene("TutorialScene");
    }
}
