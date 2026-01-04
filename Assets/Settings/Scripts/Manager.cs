using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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
    public TutorialHeadTiltMovement headTiltMovement;

    // --- Ball selection timing ---
    private float ballSpawnTime = 0f;
    private float totalBallDecisionTime = 0f;
    private int totalBallDecisions = 0;

    // --- Game timing ---
    private float gameStartTime = 0f;


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

    // X position tracking system
    private HashSet<float> occupiedXPositions = new HashSet<float>();
    private Dictionary<GameObject, float> objectXPositions = new Dictionary<GameObject, float>();

    void Start()
    {
        gameStartTime = Time.time;
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

            // Try to find TutorialHeadTiltMovement if not assigned
            if (headTiltMovement == null)
            {
                headTiltMovement = FindObjectOfType<TutorialHeadTiltMovement>();
            }

            if (headTiltMovement != null)
            {
                float avgTilt = headTiltMovement.GetAverageTilt();
                Debug.Log($"Average head tilt magnitude: {avgTilt:F2}");
            }
            else
            {
                Debug.LogWarning("Average head tilt magnitude: NOT AVAILABLE (TutorialHeadTiltMovement component not found)");
            }

            float totalGameTime = Time.time - gameStartTime;
            Debug.Log($"Total game time: {totalGameTime:F2} seconds");

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

    // Helper methods for X position tracking
    List<float> GetAvailableXPositions()
    {
        List<float> available = new List<float>();
        foreach (float xLane in treeXLanes)
        {
            if (!occupiedXPositions.Contains(xLane))
            {
                available.Add(xLane);
            }
        }
        return available;
    }

    float? GetRandomAvailableXPosition()
    {
        List<float> available = GetAvailableXPositions();
        if (available.Count == 0)
            return null;
        return available[Random.Range(0, available.Count)];
    }

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

        // Get available X positions
        List<float> availableXPositions = GetAvailableXPositions();
        
        // If excludeLastLane is true and we have a last tree lane, filter it out
        if (excludeLastLane && lastTreeLane >= 0 && lastTreeLane < treeXLanes.Length)
        {
            float lastTreeX = treeXLanes[lastTreeLane];
            availableXPositions.Remove(lastTreeX);
        }

        // If no available positions, don't spawn
        if (availableXPositions.Count == 0)
            return;

        // Randomly select from available X positions
        float chosenX = availableXPositions[Random.Range(0, availableXPositions.Count)];
        
        // Find the lane index for tracking (optional, for excludeLastLane logic)
        int laneIndex = System.Array.IndexOf(treeXLanes, chosenX);
        lastTreeLane = laneIndex;

        // Mark X position as occupied
        occupiedXPositions.Add(chosenX);
        objectXPositions[treeToUse] = chosenX;

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
        // Get available X positions
        float? availableX = GetRandomAvailableXPosition();
        if (availableX == null)
        {
            // No available X positions, skip spawning this time
            return;
        }
        float chosenX = availableX.Value;

        int laneIndex = Random.Range(0, ballYLanes.Length);
        float chosenY = ballYLanes[laneIndex];
        
        // Random ball type selection with equal final counts
        int ballType = Random.Range(0, 2);
        ObjectMover ballMover;
        GameObject ballToSpawn = null;
        
        if (ballType == 0)
        {
            if (spawnedHeadTotal < totalHeadBall)
            {
                ballToSpawn = ballHead;
                spawnedHeadTotal++;
            }
            else if (spawnedControllerTotal < totalControllerBall)
            {
                ballToSpawn = ballController;
                spawnedControllerTotal++;
            }
        }
        else
        {
            if (spawnedControllerTotal < totalControllerBall)
            {
                ballToSpawn = ballController;
                spawnedControllerTotal++;
            }
            else if (spawnedHeadTotal < totalHeadBall)
            {
                ballToSpawn = ballHead;
                spawnedHeadTotal++;
            }
        }

        if (ballToSpawn == null)
            return;

        ballToSpawn.transform.position = new Vector3(ballSpawnPoint.position.x + chosenX, ballSpawnPoint.position.y + chosenY, ballSpawnPoint.position.z);
        ballMover = ballToSpawn.GetComponent<ObjectMover>();

        // Mark X position as occupied
        occupiedXPositions.Add(chosenX);
        objectXPositions[ballToSpawn] = chosenX;

        ballMover.Initialize(currentSpeed);

        hasBallOnScreen = true;
        ballSpawnTimer = timeBetweenBalls;
        // Record spawn time
        ballSpawnTime = Time.time;
    }

    public void OnObjectDespawned(GameObject obj, DespawnReason reason)
    {
        // Clear X position when object despawns
        if (objectXPositions.ContainsKey(obj))
        {
            float xPosition = objectXPositions[obj];
            occupiedXPositions.Remove(xPosition);
            objectXPositions.Remove(obj);
        }

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