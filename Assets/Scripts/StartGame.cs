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
}
