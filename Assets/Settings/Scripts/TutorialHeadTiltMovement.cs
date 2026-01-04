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
using System;

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

    // --- Tilt tracking for logging ---
    private float totalTilt = 0f;
    private int tiltSamples = 0;

    // Movement tracking for logging
    private bool isMoving = false;
    private float movementTiltTotal = 0f;
    private int movementTiltSamples = 0;
    private float movementStartTime = 0f;
    private const float MOVEMENT_THRESHOLD = 0.001f;
    private const float STATIONARY_TIME_THRESHOLD = 0.5f;
    private const float PERIODIC_LOG_INTERVAL = 2.0f;
    private float stationaryTime = 0f;
    private float timeSinceLastPeriodicLog = 0f;

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

        // Track overall tilt
        totalTilt += Mathf.Abs(tiltNormalized);
        tiltSamples++;

        Vector3 right = Vector3.right;

        Vector3 targetVelocity = right * tiltNormalized * sideSpeed;

        currentVelocity = Vector3.Lerp(
            currentVelocity,
            targetVelocity,
            Time.fixedDeltaTime * movementSmoothing
        );

        // Detect if actually moving
        float moveMagnitude = currentVelocity.magnitude;
        bool currentlyMoving = moveMagnitude > MOVEMENT_THRESHOLD;

        // Calculate unclamped position
        Vector3 newPosition = rb.position + currentVelocity * Time.fixedDeltaTime;

        // Clamp left/right movement
        newPosition.x = Mathf.Clamp(
            newPosition.x,
            startX - maxSideDistance,
            startX + maxSideDistance
        );

        rb.MovePosition(newPosition);

        // Track movement periods for logging
        if (currentlyMoving)
        {
            if (!isMoving)
            {
                // Movement just started
                isMoving = true;
                movementStartTime = Time.time;
                movementTiltTotal = 0f;
                movementTiltSamples = 0;
                timeSinceLastPeriodicLog = 0f;
            }
            
            // Accumulate tilt during movement
            movementTiltTotal += Mathf.Abs(tiltNormalized);
            movementTiltSamples++;
            stationaryTime = 0f;
            
            // Periodic logging during continuous movement
            timeSinceLastPeriodicLog += Time.fixedDeltaTime;
            if (timeSinceLastPeriodicLog >= PERIODIC_LOG_INTERVAL)
            {
                LogMovementTilt();
                // Reset for next period
                movementStartTime = Time.time;
                movementTiltTotal = 0f;
                movementTiltSamples = 0;
                timeSinceLastPeriodicLog = 0f;
            }
        }
        else
        {
            if (isMoving)
            {
                // Was moving, now stationary
                stationaryTime += Time.fixedDeltaTime;
                
                // If stationary long enough, log the movement period
                if (stationaryTime >= STATIONARY_TIME_THRESHOLD)
                {
                    LogMovementTilt();
                    isMoving = false;
                    stationaryTime = 0f;
                    timeSinceLastPeriodicLog = 0f;
                }
            }
        }
    }

    private void LogMovementTilt()
    {
        if (movementTiltSamples > 0)
        {
            float avgTilt = movementTiltTotal / movementTiltSamples;
            float movementDuration = Time.time - movementStartTime;
            string timestamp = DateTime.UtcNow.ToString("o"); // ISO 8601 UTC
            
            // Log in format that will be captured by LogOutputHandler
            Debug.Log($"HEAD_TILT_MOVEMENT|timestamp:{timestamp}|averageTilt:{avgTilt:F4}|duration:{movementDuration:F2}|samples:{movementTiltSamples}");
        }
    }

    public float GetAverageTilt()
    {
        if (tiltSamples == 0) return 0f;
        return totalTilt / tiltSamples;
    }
}
