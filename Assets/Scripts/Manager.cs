using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager : MonoBehaviour
{
    private int playerScoreHead = 0;
    private int playerScoreButton = 0;
    private int playerScoreTree = 0;
    public GameObject scoreTextHead;
    public GameObject scoreTextButton;
    public GameObject scoreTextTree;
    private TextMeshProUGUI scoreTextComponentHead;
    private TextMeshProUGUI scoreTextComponentButton;
    private TextMeshProUGUI scoreTextComponentTree;

    [Header("Prefabs")]
    public GameObject pooledTree;
    public GameObject ballHead;
    public GameObject ballController;

    private bool hasBallOnScreen = false;
    private bool hasTreeOnScreen = false;

    [Header("Tree Spawning")]
    public Transform treeSpawnPoint;
    public float timeBetweenTrees = 3f;
    private float treeSpawnTimer = 0f;


    [Header("Ball Spawning")]
    public Transform ballSpawnPoint;
    public float timeBetweenBalls = 3f;
    private float ballSpawnTimer = 0f;

    [Header("Lanes positions")]
    public float[] treeXLanes = new float[] { -5f, 0f, 5f };
    public float[] ballXLanes = new float[] { -5f, 0f, 5f };
    public float[] ballYLanes = new float[] { 1f, 0f};

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
    void Update()
    {
        HandleTreeSpawning();
        HandleBallSpawning();
    }

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

     void HandleTreeSpawning()
    {
        if (hasTreeOnScreen)
            return;

        treeSpawnTimer -= Time.deltaTime;

        if (treeSpawnTimer <= 0f)
        {
            SpawnTree();
        }
    }

    void HandleBallSpawning()
    {
        if (hasBallOnScreen)
            return;

        ballSpawnTimer -= Time.deltaTime;

        if (ballSpawnTimer <= 0f)
        {
            SpawnBall();
        }
    }

    void SpawnTree()
    {
        int laneIndex = Random.Range(0, treeXLanes.Length);
        float chosenX = treeXLanes[laneIndex];
        //Debug.Log(chosenX);
        pooledTree.transform.position = new Vector3(treeSpawnPoint.position.x + chosenX, treeSpawnPoint.position.y, treeSpawnPoint.position.z);
        //pooledTree.SetActive(true);

        ObjectMover treeMover = pooledTree.GetComponent<ObjectMover>();
        treeMover.Initialize();

        hasTreeOnScreen = true;
        treeSpawnTimer = timeBetweenTrees;
    }

    void SpawnBall()
    {
        int laneIndex = Random.Range(0, ballXLanes.Length);
        float chosenX = ballXLanes[laneIndex];
        laneIndex = Random.Range(0, ballYLanes.Length);
        float chosenY = ballYLanes[laneIndex];
        //Debug.Log(chosenX);
        int ballType = Random.Range(0, 2);
        ObjectMover ballMover;
        if (ballType == 0)
        {
            ballHead.transform.position = new Vector3(ballSpawnPoint.position.x + chosenX, ballSpawnPoint.position.y + chosenY, ballSpawnPoint.position.z);
            ballMover = ballHead.GetComponent<ObjectMover>();
        }
        else
        {
            ballController.transform.position = new Vector3(ballSpawnPoint.position.x + chosenX, ballSpawnPoint.position.y + chosenY, ballSpawnPoint.position.z);
            ballMover = ballController.GetComponent<ObjectMover>();
        }

        ballMover.Initialize();

        hasBallOnScreen = true;
        ballSpawnTimer = timeBetweenBalls;
    }

    public void OnObjectDespawned(GameObject obj, DespawnReason reason)
    {
        if (obj == ballHead || obj == ballController)
        {
            hasBallOnScreen = false;

            if (reason == DespawnReason.PassedPlayer)
            {
                ballSpawnTimer = timeBetweenBalls;
            }
            // SelectedByPlayer → DO NOT reset timer
        }

        if (obj == pooledTree)
        {
            hasTreeOnScreen = false;
            treeSpawnTimer = timeBetweenTrees;
        }
    }


}
