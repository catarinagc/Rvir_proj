using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager : MonoBehaviour
{
    private int playerScore = 0;
    public GameObject scoreText;
    private TextMeshProUGUI scoreTextComponent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scoreTextComponent = scoreText.GetComponent<TextMeshProUGUI>();
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}

    public void addPoint()
    {
        playerScore++;
        Debug.Log("score: " + playerScore);
        scoreTextComponent.text = "Score: " + playerScore;
    }
}
