using UnityEngine;

public class HeadGestureDetector : MonoBehaviour
{
    [Header("Gesture Settings")]
    public float upwardAngleThreshold = -10f;  // head looking up (pitch is negative)
    public float minAngularSpeed = 30f;        // degrees per second
    public float gestureCooldown = 0.6f;
    private float averageTilt = 0.0f;

    private float lastPitch;
    private float cooldownTimer = 0f;

    public System.Action OnLookUpGesture;

    void Start()
    {
        lastPitch = GetPitch(transform.rotation);
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        float pitch = GetPitch(transform.rotation);
        float pitchDelta = pitch - lastPitch;
        float angularSpeed = pitchDelta / Time.deltaTime;  // deg per second

        // Detect quick look-up gesture
        if (cooldownTimer <= 0f &&
            angularSpeed < -minAngularSpeed &&      // rapid upward motion
            pitch < upwardAngleThreshold)          // AND looking upward
        {
            cooldownTimer = gestureCooldown;
            OnLookUpGesture?.Invoke();
        }

        lastPitch = pitch;
    }

    private float GetPitch(Quaternion q)
    {
        Vector3 e = q.eulerAngles;
        // Convert Unity's 0–360 pitch into -180–180
        float pitch = e.x > 180 ? e.x - 360 : e.x;
        return pitch;
    }

    public void debugAverageTilt()
    {
        Debug.Log(averageTilt);
    }
    
}
