using UnityEngine;

public enum DespawnReason
{
    PassedPlayer,
    SelectedByPlayer
}

public class ObjectMover : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 0.5f;

    [Tooltip("How far past the player (in Z) before despawning")]
    public float despawnDistance = 5f;

    public GameObject playerObj;
    private Transform player;

    private bool active;

    // Cached at spawn
    private float moveDirectionZ;
    public Manager manager;

    void Start()
    {
        player = playerObj.transform;
        gameObject.SetActive(false);
    }

    // Call this from Manager when spawning / reusing
    public void Initialize(float newSpeed)
    {
        speed = newSpeed;
        if (player == null)
        {
            Debug.LogError("ObjectMover: Player Transform not assigned!");
            return;
        }

        active = true;

        moveDirectionZ = 1.0f;

        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!active)
            return;

        // Move only in Z
        transform.position += new Vector3(
            0f,
            0f,
            moveDirectionZ * speed * Time.deltaTime
        );

        // Check if object has gone past the player
        float zOffset = transform.position.z - player.position.z;

        if (zOffset > despawnDistance)
        {
            StopMoving(DespawnReason.PassedPlayer);
        }

    }

    void StopMoving(DespawnReason reason)
    {
        active = false;
        gameObject.SetActive(false);

        if (manager != null)
            manager.OnObjectDespawned(gameObject, reason);
    }

    public void ForceDespawnByPlayer()
    {
        StopMoving(DespawnReason.SelectedByPlayer);
    }

}