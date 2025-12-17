using System.Threading;
using UnityEngine;

/// <summary>
/// Handles player physics: gravity, ground detection, and collision with terrain.
/// Works with HeadTiltMovement to provide smooth physics-based movement.
/// </summary>
public class PlayerPhysics : MonoBehaviour
{
    [Header("Gravity Settings")]
    [Tooltip("Gravity strength (negative value for downward force).")]
    public float gravity = -9.81f;
    
    [Header("Ground Detection")]
    [Tooltip("Distance to check for terrain below the player.")]
    public float groundCheckDistance = 0.5f;
    
    [Tooltip("Player height for ground calculations.")]
    public float playerHeight = 1.8f;
    
    [Tooltip("Player radius for collision detection.")]
    public float playerRadius = 0.3f;
    
    [Tooltip("Layer mask for terrain/ground objects.")]
    public LayerMask groundLayer = -1;
    
    [Header("Tree Collision Settings")]
    [Tooltip("Layer mask for tree objects. Set this to the layer your trees are on.")]
    public LayerMask treeLayer = 0;
    
    [Tooltip("Distance to check ahead for tree collisions.")]
    public float treeCheckDistance = 1f;
    
    [Tooltip("Force to push player away from trees when collision detected.")]
    public float treePushForce = 3f;

    /*[Header("Speed Tracking")]
    private Vector3 lastPosition;
    private float totalDistanceTravelled = 0f;
    private float elapsedTime = 0f; */

    private Vector3 velocity;
    private Rigidbody rb;
    private CharacterController cc;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
    }
    
    /// <summary>
    /// Applies gravity and ground collision to the movement vector.
    /// Call this from your movement script after calculating horizontal movement.
    /// </summary>
    /// <param name="horizontalMovement">The horizontal movement vector (X and Z only).</param>
    /// <returns>The final movement vector including gravity and collision adjustments.</returns>
    public Vector3 ApplyPhysics(Vector3 horizontalMovement)
    {
        // Check for tree collisions and adjust movement
        horizontalMovement = CheckTreeCollisions(horizontalMovement);
        
        if (rb != null && rb.useGravity)
        {
            // Rigidbody handles gravity automatically
            rb.linearVelocity = new Vector3(horizontalMovement.x / Time.deltaTime, rb.linearVelocity.y, horizontalMovement.z / Time.deltaTime);
            return horizontalMovement;
        }
        else if (cc != null)
        {
            // CharacterController handles collision automatically
            velocity.y += gravity * Time.deltaTime;
            Vector3 movement = horizontalMovement + new Vector3(0, velocity.y * Time.deltaTime, 0);
            cc.Move(movement);
            if (cc.isGrounded) velocity.y = 0f;
            return Vector3.zero; // CharacterController already moved
        }
        else
        {
            // Manual physics handling
            return ApplyManualPhysics(horizontalMovement);
        }
    }
    
    private Vector3 ApplyManualPhysics(Vector3 horizontalMovement)
    {
        /* // Update elapsed time
        elapsedTime += Time.deltaTime;

        // Calculate distance travelled this frame
        if (lastPosition != Vector3.zero)
        {
            float distanceThisFrame = Vector3.Distance(transform.position, lastPosition);
            totalDistanceTravelled += distanceThisFrame;
        }

        // Update last position for next frame
        lastPosition = transform.position; */

        // Ground detection
        Vector3 feetPosition = transform.position + Vector3.down * (playerHeight * 0.5f);
        bool isGrounded = Physics.CheckSphere(feetPosition, playerRadius, groundLayer) || 
                         Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + playerHeight * 0.5f, groundLayer);
        
        float distanceToGround = 0f;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance + playerHeight, groundLayer))
        {
            distanceToGround = hit.distance;
            if (distanceToGround < playerHeight * 0.5f + 0.1f && velocity.y <= 0)
            {
                isGrounded = true;
            }
        }
        
        // Apply gravity
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
            // Snap to ground if very close
            if (distanceToGround > 0 && distanceToGround < playerHeight * 0.5f + 0.2f)
            {
                transform.position = new Vector3(transform.position.x, hit.point.y + playerHeight * 0.5f, transform.position.z);
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
        
        // Calculate movement
        Vector3 movement = horizontalMovement + new Vector3(0, velocity.y * Time.deltaTime, 0);
        
        // Simple ground collision check
        RaycastHit groundHit;
        if (Physics.Raycast(transform.position + movement, Vector3.down, out groundHit, playerHeight, groundLayer))
        {
            float groundY = groundHit.point.y + playerHeight * 0.5f;
            if (transform.position.y + movement.y < groundY)
            {
                movement.y = groundY - transform.position.y;
                if (velocity.y < 0) velocity.y = 0f;
            }
        }
        
        return movement;
    }
    
    /// <summary>
    /// Checks for tree collisions ahead and adjusts movement to push player to the side.
    /// </summary>
    private Vector3 CheckTreeCollisions(Vector3 horizontalMovement)
    {
        if (treeLayer == 0 || horizontalMovement.magnitude < 0.01f)
        {
            return horizontalMovement; // No tree layer set or no movement
        }

        // Check for trees in the movement direction
        Vector3 checkDirection = horizontalMovement.normalized;
        Vector3 checkPosition = transform.position + Vector3.up * (playerHeight * 0.5f); // Check at player center height
        
        RaycastHit treeHit;
        if (Physics.Raycast(checkPosition, checkDirection, out treeHit, treeCheckDistance, treeLayer))
        {
            // Tree detected ahead, calculate push direction
            Vector3 treePosition = treeHit.collider.transform.position;
            Vector3 playerPosition = transform.position;
            
            // Calculate which side of tree player is approaching from
            Vector3 playerToTree = (treePosition - playerPosition).normalized;
            Vector3 playerForward = checkDirection;
            playerForward.y = 0;
            playerForward.Normalize();
            
            // Determine push direction (left or right)
            Vector3 crossProduct = Vector3.Cross(playerForward, playerToTree);
            float dotProduct = Vector3.Dot(crossProduct, Vector3.up);
            
            Vector3 pushDirection;
            if (dotProduct > 0)
            {
                // Push to the left
                pushDirection = -transform.right;
            }
            else
            {
                // Push to the right
                pushDirection = transform.right;
            }
            
            pushDirection.y = 0;
            pushDirection.Normalize();
            
            // Apply push force perpendicular to movement
            Vector3 pushMovement = pushDirection * treePushForce * Time.deltaTime;
            
            // Reduce forward movement and add side push
            horizontalMovement = horizontalMovement * 0.3f + pushMovement;
        }
        
        return horizontalMovement;
    }
    
    /// <summary>
    /// Resets vertical velocity (useful for jumping or resetting physics state).
    /// </summary>
    public void ResetVelocity()
    {
        velocity = Vector3.zero;
    }

    /* public float getAverageSpeed()
    {
        if (elapsedTime <= 0f) return 0f;
        return totalDistanceTravelled / elapsedTime; //units per second
    } */
}

