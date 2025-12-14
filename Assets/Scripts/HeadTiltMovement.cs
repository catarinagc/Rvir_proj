using UnityEngine;

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

        // Move XR Origin without rotating
        rb.MovePosition(rb.position + horizontalMove);
    }

    
}
