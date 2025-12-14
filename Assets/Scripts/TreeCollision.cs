using UnityEngine;

/// <summary>
/// Handles collision detection with trees. When the XR Rig collides with a tree,
/// it pushes the player to the side based on approach direction and increments the tree collision counter.
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
    
    [Tooltip("Tag name for the XR Rig/Player GameObject.")]
    public string playerTag = "Player";
    
    private Manager managerScript;
    private float lastCollisionTime = 0f;
    private CharacterController playerCharacterController;
    private Transform playerTransform;

    void Start()
    {
        if (GameManager != null)
        {
            managerScript = GameManager.GetComponent<Manager>();
        }
        else
        {
            Debug.LogWarning("TreeCollision: GameManager reference not set on " + gameObject.name);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject, collision);
    }

    void OnCollisionStay(Collision collision)
    {
        // Also handle continuous collisions to prevent getting stuck
        HandleCollision(collision.gameObject, collision);
    }

    private void HandleCollision(GameObject collidingObject, Collision collision)
    {
        // Check if colliding object is the player (XR Rig)
        if (collidingObject.CompareTag(playerTag) || IsXRRig(collidingObject))
        {
            // Check cooldown
            if (Time.time - lastCollisionTime < collisionCooldown)
            {
                return;
            }

            // Get player components
            if (playerCharacterController == null)
            {
                playerCharacterController = collidingObject.GetComponent<CharacterController>();
            }
            if (playerTransform == null)
            {
                playerTransform = collidingObject.transform;
            }

            if (playerTransform == null)
            {
                return;
            }

            // Calculate push direction based on approach
            Vector3 pushDirection = CalculatePushDirection(collision, playerTransform);

            // Apply push to player
            PushPlayerAway(pushDirection, collidingObject);

            // Increment counter
            if (managerScript != null)
            {
                managerScript.addPointTree();
            }

            lastCollisionTime = Time.time;
        }
    }

    /// <summary>
    /// Calculates the push direction based on which side of the tree the player approached from.
    /// </summary>
    private Vector3 CalculatePushDirection(Collision collision, Transform playerTransform)
    {
        // Get the collision point
        Vector3 collisionPoint = collision.contacts[0].point;
        Vector3 treePosition = transform.position;
        Vector3 playerPosition = playerTransform.position;

        // Calculate direction from player to tree
        Vector3 playerToTree = (treePosition - playerPosition).normalized;
        
        // Get player's forward direction (horizontal only)
        Vector3 playerForward = playerTransform.forward;
        playerForward.y = 0;
        playerForward.Normalize();

        // Calculate which side of the tree the player is on
        // Using cross product to determine left/right
        Vector3 crossProduct = Vector3.Cross(playerForward, playerToTree);
        float dotProduct = Vector3.Dot(crossProduct, Vector3.up);

        // Positive dot product means player is on the right side of tree (relative to forward)
        // Negative means left side
        // Push away from tree: if on right, push left; if on left, push right
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

        // Make sure push direction is horizontal only
        pushDirection.y = 0;
        pushDirection.Normalize();

        return pushDirection;
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
    private bool IsXRRig(GameObject obj)
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
}
