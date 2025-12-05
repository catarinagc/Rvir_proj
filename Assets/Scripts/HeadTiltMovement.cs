using UnityEngine;

public class HeadTiltMovement : MonoBehaviour
{
    public Transform vrCamera;
    public float sensitivity = 2f;
    public float speed = 3f;
    public float tiltSpeed = 3f;
    public float normalSpeed = 3f;
    public float crouchSpeed = 20f;
    public float crouchOffset = 0.05f;
    private float standingHeadY;
    private float calibratedStandingHeight;
    private bool hasCalibrated = false;

    void Start()
    {
        if (!vrCamera) return;
        //standingHeadY = vrCamera.position.y;
        Invoke(nameof(CalibrateHeight), 1.0f);
    }

    void CalibrateHeight()
    {
        calibratedStandingHeight = vrCamera.position.y;
        hasCalibrated = true;
        Debug.Log("Standing height calibrated at: " + calibratedStandingHeight);
    }

    void Update()
    {

        if (!vrCamera || !hasCalibrated) return;

        float currentHeadHeight = vrCamera.position.y;
        bool isCrouched = currentHeadHeight < calibratedStandingHeight - crouchOffset;

        float speed = isCrouched ? crouchSpeed : normalSpeed;

        if (isCrouched)
        {
            Debug.Log("CROUCH DETECTED");
            Debug.Log("current y position: " + currentHeadHeight);
        }

        float roll = vrCamera.localEulerAngles.z;
        if (roll > 180f) roll -= 360f;
        float tilt = Mathf.Clamp(roll / 45f, -1f, 1f);

        Debug.Log("current head tilt: " + tilt);

        Vector3 forward = vrCamera.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = vrCamera.right;
        right.y = 0;
        right.Normalize();

        Vector3 movement = forward * speed * Time.deltaTime + right * - tilt * tiltSpeed * Time.deltaTime; // Move left/right from tilt

        transform.position += movement;
    }
}