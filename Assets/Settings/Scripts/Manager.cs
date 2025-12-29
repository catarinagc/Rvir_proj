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
    public HeadTiltMovement headTiltMovement;

    // --- Ball selection timing ---
    private float ballSpawnTime = 0f;
    private float totalBallDecisionTime = 0f;
    private int totalBallDecisions = 0;


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
    public float[] ballYLanes = new float[] { 1f, 0f };

    [Header("Speed Variables")]
    public float maxSpeed = 2.0f;
    public float startSpeed = 0.5f;
    public float speedIncreasePerSpawn = 0.05f;
    private float currentSpeed;

    private int activeTreeCount = 0;

    [Header("Tree Spawn Difficulty")]
    public float startTimeBetweenTrees = 3f;
    public float minTimeBetweenTrees = 0.8f;
    public float timeDecreasePerSpawn = 0.1f;

    private float ballsBeforeDoubleTree = 0;   // after this many spawns

    void Start()
    {
        currentSpeed = startSpeed;
        scoreTextComponentHead = scoreTextHead.GetComponent<TextMeshProUGUI>();
        scoreTextComponentButton = scoreTextButton.GetComponent<TextMeshProUGUI>();
        timeBetweenTrees = startTimeBetweenTrees;
        treeSpawnTimer = timeBetweenTrees;
        ballsBeforeDoubleTree = (totalControllerBall + totalHeadBall) * 0.7f;
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
        treeSpawnTimer -= Time.deltaTime;

        if (treeSpawnTimer > 0f)
            return;

        TrySpawnTree();

        if (spawnedControllerTotal + spawnedHeadTotal >= ballsBeforeDoubleTree)
        {
            TrySpawnTree(excludeLastLane: true);
        }

        treeSpawnTimer = timeBetweenTrees;
    }

    void HandleBallSpawning()
    {
        if (hasBallOnScreen)
            return;

        ballSpawnTimer -= Time.deltaTime;

        if (ballSpawnTimer > 0f)
            return;

        int totalSpawned = spawnedControllerTotal + spawnedHeadTotal;
        int totalBalls = totalControllerBall + totalHeadBall;

        // ✅ END GAME — STOP HERE
        if (totalSpawned >= totalBalls)
        {
            Debug.Log("=== END OF GAME SUMMARY ===");
            Debug.Log($"Green balls selected (button): {playerScoreButton}");
            Debug.Log($"Red balls selected (head): {playerScoreHead}");
            Debug.Log($"Total trees hit: {playerScoreTree}");

            if (headTiltMovement != null)
            {
                float avgTilt = headTiltMovement.GetAverageTilt();
                Debug.Log($"Average head tilt magnitude: {avgTilt:F2}");
            }

            if (totalBallDecisions > 0)
            {
                float averageTime = totalBallDecisionTime / totalBallDecisions;
                Debug.Log($"Average ball selection time: {averageTime:F2} seconds");
            }

            Debug.Log("=== END OF SUMMARY ===");

            SceneManager.LoadScene("EndMenu");
            return;
        }

        SpawnBall();
        IncreaseSpeed();
    }

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

        timeBetweenTrees = Mathf.Max(
            timeBetweenTrees - timeDecreasePerSpawn,
            minTimeBetweenTrees
        );

        treeSpawnTimer = timeBetweenTrees;
    }

    int lastTreeLane = -1;

    void TrySpawnTree(bool excludeLastLane = false)
    {
        if (activeTreeCount >= pooledTrees.Length)
            return;

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

        int laneIndex;

        if (excludeLastLane && treeXLanes.Length > 1)
        {
            do
            {
                laneIndex = Random.Range(0, treeXLanes.Length);
            }
            while (laneIndex == lastTreeLane);
        }
        else
        {
            laneIndex = Random.Range(0, treeXLanes.Length);
        }

        lastTreeLane = laneIndex;

        float chosenX = treeXLanes[laneIndex];

        treeToUse.transform.position = new Vector3(
            treeSpawnPoint.position.x + chosenX,
            treeSpawnPoint.position.y,
            treeSpawnPoint.position.z
        );

        ObjectMover mover = treeToUse.GetComponent<ObjectMover>();
        mover.Initialize(currentSpeed);

        activeTreeCount++;

        timeBetweenTrees = Mathf.Max(
            timeBetweenTrees - timeDecreasePerSpawn,
            minTimeBetweenTrees
        );
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
            if (spawnedControllerTotal < totalControllerBall)
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
        // Record spawn time
        ballSpawnTime = Time.time;
    }

    public void OnObjectDespawned(GameObject obj, DespawnReason reason)
    {
        if (obj == ballHead || obj == ballController)
        {
            hasBallOnScreen = false;

            float decisionTime = Time.time - ballSpawnTime;
            totalBallDecisionTime += decisionTime;
            totalBallDecisions++;

            if (reason == DespawnReason.PassedPlayer)
            {
                ballSpawnTimer = timeBetweenBalls;
            }
            // SelectedByPlayer → DO NOT reset timer
        }

        foreach (var tree in pooledTrees)
        {
            if (obj == tree)
            {
                activeTreeCount--;
                break;
            }
        }

    }


}