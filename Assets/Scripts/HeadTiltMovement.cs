using UnityEngine;

public class HeadTiltMovement : MonoBehaviour
{
    public Transform vrCamera;
    public float sensitivity = 2f;
    public float speed = 3f;
    public float tiltSpeed = 3f;
    public float normalSpeed = 3f;
    public float crouchSpeed = 6f;       
    public float crouchThreshold = 1.2f; 
    private float standingHeadY;

    void Start()
    {
        if (!vrCamera) return;
        standingHeadY = vrCamera.position.y;
    }

    void Update()
    {

        float curY = vrCamera.position.y;
        bool isCrouched = curY < standingHeadY - crouchThreshold;

        float currentSpeed = isCrouched ? crouchSpeed : normalSpeed;


        float roll = vrCamera.localEulerAngles.z;
        if (roll > 180f) roll -= 360f;
        float tilt = Mathf.Clamp(roll / 45f, -1f, 1f);

        Vector3 forward = vrCamera.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = vrCamera.right;
        right.y = 0;
        right.Normalize();

        Vector3 movement = forward * currentSpeed * Time.deltaTime + right * - tilt * tiltSpeed * Time.deltaTime; // Move left/right from tilt

        transform.position += movement;
    }
}