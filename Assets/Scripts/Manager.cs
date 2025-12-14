using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager : MonoBehaviour
{
    private int playerScoreHead = 0;
    private int playerScoreButton = 0;
    public GameObject scoreTextHead;
    public GameObject scoreTextButton;
    private TextMeshProUGUI scoreTextComponentHead;
    private TextMeshProUGUI scoreTextComponentButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scoreTextComponentHead = scoreTextHead.GetComponent<TextMeshProUGUI>();
        scoreTextComponentButton = scoreTextButton.GetComponent<TextMeshProUGUI>();
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}

    public void addPointHead()
    {
        playerScoreHead++;
        Debug.Log("score head: " + playerScoreHead);
        scoreTextComponentHead.text = "Score: " + playerScoreHead;
    }

    public void addPointButton()
    {
        playerScoreButton++;
        Debug.Log("score button: " + playerScoreButton);
        scoreTextComponentButton.text = "Score: " + playerScoreButton;
    }
}
