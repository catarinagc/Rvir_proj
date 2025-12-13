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
    /// Resets vertical velocity (useful for jumping or resetting physics state).
    /// </summary>
    public void ResetVelocity()
    {
        velocity = Vector3.zero;
    }
}

