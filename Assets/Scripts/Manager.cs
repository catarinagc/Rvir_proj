using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
    public GameObject[] pooledTrees; // size = 2 in Inspector

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
    public int totalHeadBall = 10;
    private int spawnedHeadTotal = 0;
    public int totalControllerBall = 10;
    private int spawnedControllerTotal = 0;

    [Header("Lanes positions")]
    public float[] treeXLanes = new float[] { -5f, 0f, 5f };
    public float[] ballXLanes = new float[] { -5f, 0f, 5f };
    public float[] ballYLanes = new float[] { 1f, 0f};

    [Header("Speed Variables")]
    public float maxSpeed = 2.0f;
    public float startSpeed = 0.5f;
    public float speedIncreasePerSpawn = 0.05f;
    private float currentSpeed;

    private int activeTreeCount = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentSpeed = startSpeed;
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

    void IncreaseSpeed()
    {
        currentSpeed = Mathf.Min(
            currentSpeed + speedIncreasePerSpawn,
            maxSpeed
        );
    }

    bool CanSpawnSecondTree()
    {
        int totalSpawnedBalls = spawnedHeadTotal + spawnedControllerTotal;
        int totalBalls = totalHeadBall + totalControllerBall;

        return totalSpawnedBalls >= totalBalls * 0.5f;
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
//1 TREE verion
    //  void HandleTreeSpawning()
    // {
    //     if (hasTreeOnScreen)
    //         return;

    //     treeSpawnTimer -= Time.deltaTime;

    //     if (treeSpawnTimer <= 0f)
    //     {
    //         SpawnTree();
    //     }
    // }

//Multiple tree version
    void HandleTreeSpawning()
    {
        int maxTreesAllowed = CanSpawnSecondTree() ? 2 : 1;

        if (activeTreeCount >= maxTreesAllowed)
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
            if (spawnedControllerTotal + spawnedHeadTotal == totalControllerBall + totalHeadBall)
            {
                Debug.Log("game ends");
                SceneManager.LoadScene("EndMenu");
            }
            SpawnBall();
            IncreaseSpeed();
        }
    }

//1 tree version
    // void SpawnTree()
    // {
    //     int laneIndex = Random.Range(0, treeXLanes.Length);
    //     float chosenX = treeXLanes[laneIndex];
    //     //Debug.Log(chosenX);
    //     pooledTree.transform.position = new Vector3(treeSpawnPoint.position.x + chosenX, treeSpawnPoint.position.y, treeSpawnPoint.position.z);
    //     //pooledTree.SetActive(true);

    //     ObjectMover treeMover = pooledTree.GetComponent<ObjectMover>();
    //     treeMover.Initialize(currentSpeed);

    //     hasTreeOnScreen = true;
    //     treeSpawnTimer = timeBetweenTrees;
    // }

//multiple trees version
    void SpawnTree()
    {
        GameObject treeToUse = null;

        foreach (var tree in pooledTrees)
        {
            if (!tree.activeSelf)
            {
                treeToUse = tree;
                break;
            }
        }

        if (treeToUse == null)
            return;

        int laneIndex = Random.Range(0, treeXLanes.Length);
        float chosenX = treeXLanes[laneIndex];

        treeToUse.transform.position = new Vector3(
            treeSpawnPoint.position.x + chosenX,
            treeSpawnPoint.position.y,
            treeSpawnPoint.position.z
        );

        ObjectMover mover = treeToUse.GetComponent<ObjectMover>();
        mover.Initialize(currentSpeed);

        activeTreeCount++;
        treeSpawnTimer = timeBetweenTrees;
    }



    void SpawnBall()
    {
        int laneIndex = Random.Range(0, ballXLanes.Length);
        float chosenX = ballXLanes[laneIndex];
        laneIndex = Random.Range(0, ballYLanes.Length);
        float chosenY = ballYLanes[laneIndex];
        int ballType = Random.Range(0, 2);
        ObjectMover ballMover;
        if (ballType == 0)
        {
            if (spawnedHeadTotal < totalHeadBall)
            {
                ballHead.transform.position = new Vector3(ballSpawnPoint.position.x + chosenX, ballSpawnPoint.position.y + chosenY, ballSpawnPoint.position.z);
                ballMover = ballHead.GetComponent<ObjectMover>();
                spawnedHeadTotal++;   
            }
            else
            {
                ballController.transform.position = new Vector3(ballSpawnPoint.position.x + chosenX, ballSpawnPoint.position.y + chosenY, ballSpawnPoint.position.z);
                ballMover = ballController.GetComponent<ObjectMover>();
                spawnedControllerTotal++;
            }
        }
        else
        {
            if(spawnedControllerTotal < totalControllerBall)
            {
                ballController.transform.position = new Vector3(ballSpawnPoint.position.x + chosenX, ballSpawnPoint.position.y + chosenY, ballSpawnPoint.position.z);
                ballMover = ballController.GetComponent<ObjectMover>();
                spawnedControllerTotal++;
            }
            else
            {
                ballHead.transform.position = new Vector3(ballSpawnPoint.position.x + chosenX, ballSpawnPoint.position.y + chosenY, ballSpawnPoint.position.z);
                ballMover = ballHead.GetComponent<ObjectMover>();
                spawnedHeadTotal++;   
            }
        }

        ballMover.Initialize(currentSpeed);

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

        // if (obj == pooledTree)
        // {
        //     hasTreeOnScreen = false;
        //     treeSpawnTimer = timeBetweenTrees;
        // }
        foreach (var tree in pooledTrees)
        {
            if (obj == tree)
            {
                activeTreeCount--;
                treeSpawnTimer = timeBetweenTrees;
                break;
            }
        }
    }


}
