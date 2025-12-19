using UnityEngine;

/// <summary>
/// Handles tree collisions for XR Origin (Rigidbody-based player).
/// Pushes the player away on proximity and increments tree hit points.
/// </summary>
public class TreeCollision : MonoBehaviour
{
    [Header("Manager Reference")]
    public GameObject GameManager;

    [Header("Collision Settings")]
    public float pushForce = 5f;
    public float detectionRadius = 2.5f;
    public float collisionCooldown = 1f;
    public bool debugMode = false;
    public string playerTag = "Player"; // Optional: tag for XR Origin

    private Manager managerScript;
    private float lastCollisionTime = 0f;
    private Rigidbody playerRigidbody;
    private Transform xrRigTransform;

    void Start()
    {
        if (GameManager != null)
        {
            managerScript = GameManager.GetComponent<Manager>();
            if (managerScript == null)
            {
                Debug.LogError("TreeCollision: GameManager doesn't have a Manager component!");
            }
        }

        FindXRRig();
    }

    void Update()
    {
        CheckProximityCollision();
    }

    void FindXRRig()
    {
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag(playerTag);

        foreach (GameObject obj in allObjects)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                playerRigidbody = rb;
                xrRigTransform = obj.transform;
                return;
            }
        }

        // Fallback: search for XR Origin component
        System.Type xrOriginType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.XROrigin, Unity.XR.Interaction.Toolkit");
        if (xrOriginType != null)
        {
            foreach (GameObject obj in allObjects)
            {
                if (obj.GetComponent(xrOriginType) != null)
                {
                    xrRigTransform = obj.transform;
                    playerRigidbody = obj.GetComponent<Rigidbody>();
                    return;
                }
            }
        }

        if (xrRigTransform == null)
        {
            Debug.LogWarning("TreeCollision: Could not find XR Rig in the scene.");
        }
    }

    void CheckProximityCollision()
    {
        if (xrRigTransform == null || playerRigidbody == null) return;

        Vector3 horizontalDiff = xrRigTransform.position - transform.position;
        horizontalDiff.y = 0; // ignore vertical
        float distance = horizontalDiff.magnitude;

        if (debugMode && distance <= detectionRadius * 1.2f)
        {
            Debug.Log($"TreeCollision [{gameObject.name}] distance: {distance:F2}");
        }

        if (distance <= detectionRadius && Time.time - lastCollisionTime >= collisionCooldown)
        {
            HandleProximityCollision();
            lastCollisionTime = Time.time;
        }
    }

    void HandleProximityCollision()
    {
        if (playerRigidbody == null) return;

        // Compute push direction: away from tree, horizontal only
        Vector3 direction = (xrRigTransform.position - transform.position).normalized;
        direction.y = 0;

        // Optional: push perpendicular to player forward for fun arcade effect
        Vector3 pushDir = Vector3.Cross(Vector3.up, Vector3.Cross(direction, xrRigTransform.forward)).normalized;

        // Apply push instantly
        playerRigidbody.AddForce(pushDir * pushForce * 10f, ForceMode.VelocityChange);


        // Increment tree points
        if (managerScript != null)
        {
            managerScript.addPointTree();
        }
        else if (debugMode)
        {
            Debug.LogWarning("TreeCollision: Manager script not assigned!");
        }
    }

    // Draw detection radius
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
