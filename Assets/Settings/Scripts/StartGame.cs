using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Legacy StartGame script - now uses async loading for better performance
/// Consider migrating to MenuManager for full features
/// </summary>
public class StartGame : MonoBehaviour
{
    private bool isLoading = false;
    
    public void StartGameOnCLick()
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneAsync("SampleScene"));
        }
    }

    public void StartTutorialOnCLick()
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneAsync("TutorialScene"));
        }
    }

    public void MainMenuOnClick()
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneAsync("MainMenu"));
        }
    }

    public void QuitOnClick()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void RestartTutorial()
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneAsync("TutorialScene"));
        }
    }
    
    /// <summary>
    /// Async scene loading for better performance
    /// </summary>
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoading = true;
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        // Wait until scene is ready
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        
        // Small delay for smoother transition
        yield return new WaitForSeconds(0.1f);
        
        // Activate the scene
        asyncLoad.allowSceneActivation = true;
        
        isLoading = false;
    }
}
