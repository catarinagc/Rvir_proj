using UnityEngine;

/// <summary>
/// Handles collision detection with trees. When the XR Rig collides with a tree,
/// it pushes the player to the side based on approach direction and increments the tree collision counter.
/// Uses proximity detection since CharacterController doesn't generate collision events.
/// </summary>
public class TreeCollision : MonoBehaviour
{
    [Header("Manager Reference")]
    [Tooltip("Reference to the GameManager GameObject that contains the Manager script.")]
    public GameObject GameManager;
    
    [Header("Collision Settings")]
    [Tooltip("Force applied to push player away from tree.")]
    public float pushForce = 5f;
    
    [Tooltip("Minimum time between collision triggers (prevents rapid counter increments).")]
    public float collisionCooldown = 1f;
    
    [Tooltip("Detection radius for proximity-based collision detection.")]
    public float detectionRadius = 2.5f;
    
    [Tooltip("Enable debug mode to see distance calculations in console.")]
    public bool debugMode = false;
    
    [Tooltip("Tag name for the XR Rig/Player GameObject.")]
    public string playerTag = "Player";
    
    private Manager managerScript;
    private float lastCollisionTime = 0f;
    private CharacterController playerCharacterController;
    private Transform playerTransform;
    private Transform xrRigTransform;

    void Start()
    {
        if (GameManager != null)
        {
            managerScript = GameManager.GetComponent<Manager>();
            if (managerScript == null)
            {
                Debug.LogError("TreeCollision: GameManager GameObject doesn't have a Manager component! GameObject: " + GameManager.name);
            }
        }
        else
        {
            Debug.LogWarning("TreeCollision: GameManager reference not set on " + gameObject.name);
        }
        
        // Find XR Rig at start
        FindXRRig();
    }

    void Update()
    {
        // Use proximity detection since CharacterController doesn't generate collision events
        CheckProximityCollision();
    }

    void FindXRRig()
    {
        // Find the XR Rig in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (IsXRRig(obj))
            {
                xrRigTransform = obj.transform;
                playerCharacterController = obj.GetComponent<CharacterController>();
                return;
            }
        }
        Debug.LogWarning("TreeCollision: Could not find XR Rig in scene. Will search each frame.");
    }

    void CheckProximityCollision()
    {
        // If we haven't found the XR Rig yet, try to find it
        if (xrRigTransform == null)
        {
            FindXRRig();
            if (xrRigTransform == null)
            {
                return; // Still can't find it
            }
        }

        // Calculate horizontal distance from tree to player (ignore Y-axis)
        Vector3 treePosition = transform.position;
        Vector3 playerPosition = xrRigTransform.position;
        
        // Use horizontal distance only (ignore Y-axis difference)
        Vector3 horizontalDiff = new Vector3(
            playerPosition.x - treePosition.x,
            0,
            playerPosition.z - treePosition.z
        );
        float distance = horizontalDiff.magnitude;

        // Debug logging when player is close
        if (debugMode && distance <= detectionRadius * 1.2f)
        {
            Debug.Log($"TreeCollision [{gameObject.name}]: Distance to player: {distance:F2} (radius: {detectionRadius})");
        }

        // Check if player is within detection radius
        if (distance <= detectionRadius)
        {
            // Check cooldown
            if (Time.time - lastCollisionTime < collisionCooldown)
            {
                return;
            }

            // Player is close enough, handle collision
            HandleProximityCollision(xrRigTransform.gameObject, distance);
        }
    }

    void HandleProximityCollision(GameObject playerObject, float distance)
    {
        // Get player components if needed
        if (playerCharacterController == null)
        {
            playerCharacterController = playerObject.GetComponent<CharacterController>();
        }
        if (playerTransform == null)
        {
            playerTransform = playerObject.transform;
        }

        if (playerTransform == null)
        {
            Debug.LogWarning("TreeCollision: Player transform is null!");
            return;
        }

        // Calculate push direction (push away from tree)
        Vector3 treePosition = transform.position;
        Vector3 playerPosition = playerTransform.position;
        Vector3 directionFromTree = (playerPosition - treePosition).normalized;
        directionFromTree.y = 0; // Keep horizontal
        directionFromTree.Normalize();
        
        // Get perpendicular direction (left or right based on approach)
        Vector3 playerForward = playerTransform.forward;
        playerForward.y = 0;
        playerForward.Normalize();
        
        Vector3 playerToTree = (treePosition - playerPosition).normalized;
        Vector3 crossProduct = Vector3.Cross(playerForward, playerToTree);
        float dotProduct = Vector3.Dot(crossProduct, Vector3.up);
        
        Vector3 pushDirection;
        if (dotProduct > 0)
        {
            // Player is on right side, push to the left
            pushDirection = -playerTransform.right;
        }
        else
        {
            // Player is on left side, push to the right
            pushDirection = playerTransform.right;
        }
        
        pushDirection.y = 0;
        pushDirection.Normalize();

        // Apply push to player
        PushPlayerAway(pushDirection, playerObject);

        // Increment counter
        if (managerScript != null)
        {
            managerScript.addPointTree();
        }
        else
        {
            Debug.LogError("TreeCollision: Manager script is null! Make sure GameManager is assigned in the Inspector.");
        }

        lastCollisionTime = Time.time;
    }

    // Keep these methods for backward compatibility (in case colliders are set up)
    void OnCollisionEnter(Collision collision)
    {
        if (IsXRRig(collision.gameObject))
        {
            HandleProximityCollision(collision.gameObject, 0f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsXRRig(other.gameObject))
        {
            HandleProximityCollision(other.gameObject, 0f);
        }
    }

    /// <summary>
    /// Pushes the player away from the tree using CharacterController or direct position adjustment.
    /// </summary>
    private void PushPlayerAway(Vector3 pushDirection, GameObject playerObject)
    {
        if (playerCharacterController != null)
        {
            // Use CharacterController.Move for smooth pushing
            Vector3 pushMovement = pushDirection * pushForce * Time.deltaTime;
            playerCharacterController.Move(pushMovement);
        }
        else
        {
            // Fallback: directly adjust position
            playerObject.transform.position += pushDirection * pushForce * Time.deltaTime;
        }
    }

    /// <summary>
    /// Checks if the GameObject is the XR Rig (by checking for CharacterController or XR Origin component).
    /// </summary>
    bool IsXRRig(GameObject obj)
    {
        // Check for CharacterController (XR Rig typically has this)
        if (obj.GetComponent<CharacterController>() != null)
        {
            return true;
        }

        // Check for XR Origin component (using reflection to avoid assembly dependency issues)
        System.Type xrOriginType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.XROrigin, Unity.XR.Interaction.Toolkit");
        if (xrOriginType != null)
        {
            if (obj.GetComponent(xrOriginType) != null)
            {
                return true;
            }

            // Check parent for XR Origin (XR Rig might be parent)
            Transform parent = obj.transform.parent;
            if (parent != null && parent.GetComponent(xrOriginType) != null)
            {
                return true;
            }
        }

        // Check if object name contains "XR" or "Rig" (fallback detection)
        if (obj.name.Contains("XR") || obj.name.Contains("Rig"))
        {
            return true;
        }

        return false;
    }

    // Visualize detection radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
