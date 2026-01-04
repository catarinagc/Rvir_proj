using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class HeadTiltMovement : MonoBehaviour
{
    public Transform vrCamera;
    public float normalSpeed = 3f;
    public float crouchSpeed = 20f;
    public float tiltSpeed = 3f;       
    public float crouchOffset = 0.05f;
    public float tiltDeadZone = 0.15f; 

    private Rigidbody rb;
    private float calibratedStandingHeight;
    private bool hasCalibrated = false;

    private float totalTilt = 0f;
    private int tiltSamples = 0;

    // Movement tracking for logging
    private bool isMoving = false;
    private float movementTiltTotal = 0f;
    private int movementTiltSamples = 0;
    private float movementStartTime = 0f;
    private const float MOVEMENT_THRESHOLD = 0.001f; // Minimum movement to consider as "moving"
    private const float STATIONARY_TIME_THRESHOLD = 0.5f; // Time to be stationary before logging
    private const float PERIODIC_LOG_INTERVAL = 2.0f; // Log every N seconds during continuous movement
    private float stationaryTime = 0f;
    private float timeSinceLastPeriodicLog = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Prevent spinning

        if (!vrCamera) return;
        Invoke(nameof(CalibrateHeight), 1.0f);
    }

    void CalibrateHeight()
    {
        calibratedStandingHeight = vrCamera.localPosition.y;
        hasCalibrated = true;
    }

    void FixedUpdate()
    {
        if (!vrCamera || !hasCalibrated) return;

        // Crouch detection
        float headHeight = vrCamera.localPosition.y;
        bool isCrouched = headHeight < calibratedStandingHeight - crouchOffset;
        float speed = isCrouched ? crouchSpeed : normalSpeed;

        // Forward and right vectors
        Vector3 forward = vrCamera.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = vrCamera.right;
        right.y = 0;
        right.Normalize();

        // Tilt
        float roll = vrCamera.localEulerAngles.z;
        if (roll > 180f) roll -= 360f;
        float tilt = Mathf.Clamp(roll / 45f, -1f, 1f);
        if (Mathf.Abs(tilt) < tiltDeadZone) tilt = 0f;

        // Compute movement
        Vector3 horizontalMove = forward * speed * Time.fixedDeltaTime + right * -tilt * tiltSpeed * Time.fixedDeltaTime;

        // Detect if actually moving
        float moveMagnitude = horizontalMove.magnitude;
        bool currentlyMoving = moveMagnitude > MOVEMENT_THRESHOLD;

        // Move XR Origin without rotating
        rb.MovePosition(rb.position + horizontalMove);

        totalTilt += Math.Abs(tilt); // use absolute value to ignore left/right sign
        tiltSamples++;

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
            movementTiltTotal += Math.Abs(tilt);
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
