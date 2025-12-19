// using UnityEngine;

// [RequireComponent(typeof(Rigidbody))]
// public class TutorialHeadTiltMovement : MonoBehaviour
// {
//     [Header("References")]
//     public Transform vrCamera;

//     [Header("Movement")]
//     public float sideSpeed = 2.0f;          // How fast left/right movement is
//     public float maxTiltAngle = 25f;         // Degrees of head tilt for full speed
//     public float tiltDeadZone = 0.1f;        // Ignore tiny tilts

//     [Header("Smoothing")]
//     public float movementSmoothing = 8f;

//     Rigidbody rb;
//     Vector3 currentVelocity;

//     void Start()
//     {
//         rb = GetComponent<Rigidbody>();

//         // Lock rotation and vertical movement
//         rb.constraints = RigidbodyConstraints.FreezeRotation |
//                          RigidbodyConstraints.FreezePositionY;
//     }

//     void FixedUpdate()
//     {
//         if (!vrCamera) return;

//         float roll = vrCamera.localEulerAngles.z;
//         if (roll > 180f) roll -= 360f;

//         float tiltNormalized = Mathf.Clamp(roll / maxTiltAngle, -1f, 1f);

//         if (Mathf.Abs(tiltNormalized) < tiltDeadZone)
//             tiltNormalized = 0f;

//         Vector3 right = Vector3.right;
//         right.y = 0;
//         right.Normalize();

//         Vector3 targetVelocity = right * tiltNormalized * sideSpeed;

//         currentVelocity = Vector3.Lerp(
//             currentVelocity,
//             targetVelocity,
//             Time.fixedDeltaTime * movementSmoothing
//         );

//         rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);
//     }
// }


using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TutorialHeadTiltMovement : MonoBehaviour
{
    [Header("References")]
    public Transform vrCamera;

    [Header("Movement")]
    public float sideSpeed = 3.0f;          
    public float maxTiltAngle = 25f;        
    public float tiltDeadZone = 0.1f;       

    [Tooltip("Maximum left/right movement distance from start")]
    public float maxSideDistance = 2.5f;

    [Header("Smoothing")]
    public float movementSmoothing = 8f;

    Rigidbody rb;
    Vector3 currentVelocity;
    float startX;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Store starting position for clamping
        startX = rb.position.x;

        rb.constraints = RigidbodyConstraints.FreezeRotation |
                         RigidbodyConstraints.FreezePositionY;
    }

    void FixedUpdate()
    {
        if (!vrCamera) return;

        // Get head roll
        float roll = vrCamera.localEulerAngles.z;
        if (roll > 180f) roll -= 360f;

        float tiltNormalized = Mathf.Clamp(roll / maxTiltAngle, -1f, 1f);

        if (Mathf.Abs(tiltNormalized) < tiltDeadZone)
            tiltNormalized = 0f;

        Vector3 right = Vector3.right;

        Vector3 targetVelocity = right * tiltNormalized * sideSpeed;

        currentVelocity = Vector3.Lerp(
            currentVelocity,
            targetVelocity,
            Time.fixedDeltaTime * movementSmoothing
        );

        // Calculate unclamped position
        Vector3 newPosition = rb.position + currentVelocity * Time.fixedDeltaTime;

        // Clamp left/right movement
        newPosition.x = Mathf.Clamp(
            newPosition.x,
            startX - maxSideDistance,
            startX + maxSideDistance
        );

        rb.MovePosition(newPosition);
    }
}
