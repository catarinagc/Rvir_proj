using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;

public class Manager : MonoBehaviour
{
    //public PlayerPhysics playerPhysics; 
    //public HeadTiltMovement headTiltMovement; 
    private int playerScoreHead = 0;
    private int playerScoreButton = 0;
    private int playerScoreTree = 0;
    public GameObject scoreTextHead;
    public GameObject scoreTextButton;
    public GameObject scoreTextTree;
    private TextMeshProUGUI scoreTextComponentHead;
    private TextMeshProUGUI scoreTextComponentButton;
    private TextMeshProUGUI scoreTextComponentTree;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scoreTextComponentHead = scoreTextHead.GetComponent<TextMeshProUGUI>();
        scoreTextComponentButton = scoreTextButton.GetComponent<TextMeshProUGUI>();
        if (scoreTextTree != null)
        {
            scoreTextComponentTree = scoreTextTree.GetComponent<TextMeshProUGUI>();
            if (scoreTextComponentTree == null)
            {
                Debug.LogError("Manager: ScoreTextTree GameObject '" + scoreTextTree.name + "' doesn't have a TextMeshProUGUI component!");
            }
        }
        else
        {
            Debug.LogWarning("Manager: ScoreTextTree GameObject is not assigned in the Inspector!");
        }
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

    public void addPointTree()
    {
        playerScoreTree++;
        
        // Try to get component if it's null (in case it wasn't set in Start)
        if (scoreTextComponentTree == null && scoreTextTree != null)
        {
            scoreTextComponentTree = scoreTextTree.GetComponent<TextMeshProUGUI>();
        }
        
        if (scoreTextComponentTree != null)
        {
            scoreTextComponentTree.text = "Trees Collisions: " + playerScoreTree;
        }
        else
        {
            if (scoreTextTree == null)
            {
                Debug.LogError("Manager: ScoreTextTree GameObject is NULL! Please assign it in the Inspector.");
            }
            else
            {
                Debug.LogError("Manager: scoreTextComponentTree is NULL! GameObject '" + scoreTextTree.name + "' might not have a TextMeshProUGUI component.");
            }
        }
    }

    /* public void Endgame()
     {
         Debug.Log("=== END OF GAME SUMMARY ===");
         Debug.Log($"Green balls selected (button): {playerScoreButton}");
         Debug.Log($"Red balls selected (head): {playerScoreHead}");
         Debug.Log($"Total trees hit: {playerScoreTree}");

         if (playerPhysics != null)
         {
            float avgSpeed = playerPhysics.GetAverageSpeed();
            Debug.Log($"Average player speed: {avgSpeed:F2} units/sec");
         }

         if (headTiltMovement != null)
         {
            float avgTilt = headTiltMovement.GetAverageTilt();
            Debug.Log($"Average head tilt magnitude: {avgTilt:F2}");
         }

         Debug.Log("=== END OF SUMMARY ===");
     } */
}
