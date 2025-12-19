using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingProgressBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    
    private bool isLoading = false;
    
    void Start()
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
        
        OptimizeMenuPerformance();
    }
    
    private void OptimizeMenuPerformance()
    {
        Application.targetFrameRate = 90;
        System.GC.Collect();
    }
    
    public void LoadSceneAsync(string sceneName)
    {
        if (isLoading) return;
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }
    
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        isLoading = true;
        
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }
        
        if (loadingText != null)
        {
            loadingText.text = "Loading...";
        }
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        while (asyncLoad.progress < 0.9f)
        {
            if (loadingProgressBar != null)
            {
                loadingProgressBar.value = asyncLoad.progress;
            }
            
            if (loadingText != null)
            {
                loadingText.text = "Loading... " + Mathf.RoundToInt(asyncLoad.progress * 100) + "%";
            }
            
            yield return null;
        }
        
        if (loadingProgressBar != null)
        {
            loadingProgressBar.value = 1f;
        }
        
        if (loadingText != null)
        {
            loadingText.text = "Ready!";
        }
        
        yield return new WaitForSeconds(0.2f);
        asyncLoad.allowSceneActivation = true;
        isLoading = false;
    }
    
    public void StartGameOnClick()
    {
        LoadSceneAsync("SampleScene");
    }
    
    public void StartTutorialOnClick()
    {
        LoadSceneAsync("TutorialScene");
    }
    
    public void MainMenuOnClick()
    {
        LoadSceneAsync("MainMenu");
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
        LoadSceneAsync("TutorialScene");
    }
}
